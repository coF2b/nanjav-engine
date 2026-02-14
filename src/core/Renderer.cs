using Silk.NET.OpenGL;
using StbImageSharp;

namespace nanjav.core;

public class Renderer
{
    private GL? _gl;
    private uint _vao;
    private uint _vbo;
    private uint _program;
    private uint _vertexShader;
    private uint _fragmentShader;
    private int _uProjectionLocation = -1;
    private float[] _projection = new float[16];
    private int _width;
    private int _height;

    private Dictionary<string, uint> _textureCache = new Dictionary<string, uint>();
    private List<GameObject> _rootObjects = new List<GameObject>();

    public Camera2D? Camera { get; set; }

    public void Load(GL gl, int width, int height)
    {
        _gl = gl ?? throw new ArgumentNullException(nameof(gl));
        _width = width;
        _height = height;

        _gl.ClearColor(0.10f, 0.12f, 0.15f, 1.0f);

        const string vertexSource = @"#version 330 core
            layout(location = 0) in vec2 aPos;  
            layout(location = 1) in vec3 aColor;
            layout(location = 2) in vec3 aNormal;
            layout(location = 3) in vec2 aTexCoord;

            out vec3 vColor;
            out vec2 TexCoord;

            uniform mat4 u_Projection;

            void main()
            {
                vColor = aColor;
                TexCoord = aTexCoord;
                gl_Position = u_Projection * vec4(aPos, 0.0, 1.0);
            }";

        const string fragmentSource = @"#version 330 core
            in vec3 vColor;
            in vec2 TexCoord;

            out vec4 FragColor;

            uniform sampler2D u_Texture;
            uniform bool u_HasTexture;

            void main()
            {
                if (u_HasTexture)
                {
                    vec4 texColor = texture(u_Texture, TexCoord);
                    FragColor = texColor * vec4(vColor, 1.0);
                }
                else
                {
                    FragColor = vec4(vColor, 1.0);
                }
            }";

        _program = ShaderUtils.CreateProgram(_gl, vertexSource, fragmentSource, out _vertexShader, out _fragmentShader);

        _uProjectionLocation = _gl.GetUniformLocation(_program, "u_Projection");

        _vao = _gl.GenVertexArray();
        _vbo = _gl.GenBuffer();

        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)0, (float[]?)null, BufferUsageARB.DynamicDraw);

        uint stride = (uint)(10 * sizeof(float));

        _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, (nint)0);

        _gl.EnableVertexAttribArray(1);
        _gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, (nint)(2 * sizeof(float)));

        _gl.EnableVertexAttribArray(2);
        _gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, stride, (nint)(5 * sizeof(float)));

        _gl.EnableVertexAttribArray(3);
        _gl.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, stride, (nint)(8 * sizeof(float)));

        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl.BindVertexArray(0);

        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        UpdateProjection();
        _gl.Viewport(0, 0, (uint)_width, (uint)_height);
    }

    public unsafe uint LoadTexture(string filePath)
    {
        if (_gl is null)
            throw new InvalidOperationException("GL не ініціалізований");

        if (_textureCache.ContainsKey(filePath))
            return _textureCache[filePath];

        ImageResult image = ImageResult.FromStream(
            File.OpenRead(filePath),
            ColorComponents.RedGreenBlueAlpha
        );

        uint textureHandle = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, textureHandle);

        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.Repeat);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.Repeat);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);

        fixed (byte* ptr = image.Data)
        {
            _gl.TexImage2D(
                TextureTarget.Texture2D,
                0,
                InternalFormat.Rgba,
                (uint)image.Width,
                (uint)image.Height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                ptr
            );
        }

        _gl.GenerateMipmap(TextureTarget.Texture2D);
        _gl.BindTexture(TextureTarget.Texture2D, 0);

        _textureCache[filePath] = textureHandle;
        return textureHandle;
    }

    public uint GetShaderProgramId()
    {
        return _program;
    }

    public void AddRootObject(GameObject obj)
    {
        _rootObjects.Add(obj);
    }

    public void RemoveRootObject(GameObject obj)
    {
        _rootObjects.Remove(obj);
    }

    public void Update(double deltaTime)
    {
        if (_gl is null) return;

        foreach (var obj in _rootObjects)
        {
            obj.Update(deltaTime);
        }
    }

    public void Render(double deltaTime)
    {
        if (_gl is null) return;

        _gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

        _gl.UseProgram(_program);
        _gl.BindVertexArray(_vao);

        int texUniform = _gl.GetUniformLocation(_program, "u_Texture");
        int hasTexUniform = _gl.GetUniformLocation(_program, "u_HasTexture");

        var renderGroups = CollectRenderGroups();

        foreach (var group in renderGroups)
        {
            if (!string.IsNullOrEmpty(group.TexturePath))
            {
                try
                {
                    uint textureId = LoadTexture(group.TexturePath);
                    _gl.ActiveTexture(TextureUnit.Texture0);
                    _gl.BindTexture(TextureTarget.Texture2D, textureId);

                    if (texUniform >= 0) _gl.Uniform1(texUniform, 0);
                    if (hasTexUniform >= 0) _gl.Uniform1(hasTexUniform, 1);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Помилка при завантаженні текстури {group.TexturePath}: {ex.Message}");
                    if (hasTexUniform >= 0) _gl.Uniform1(hasTexUniform, 0);
                }
            }
            else
            {
                if (hasTexUniform >= 0) _gl.Uniform1(hasTexUniform, 0);
            }

            if (group.Vertices.Length > 0)
            {
                _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
                _gl.BufferData(BufferTargetARB.ArrayBuffer,
                    (nuint)(group.Vertices.Length * sizeof(float)),
                    group.Vertices,
                    BufferUsageARB.DynamicDraw);

                _gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)(group.Vertices.Length / 10));
            }
        }

        _gl.BindVertexArray(0);
        _gl.UseProgram(0);

        foreach (var obj in _rootObjects)
        {
            obj.Render();
        }
    }

    private class RenderGroup
    {
        public string TexturePath { get; set; } = "";
        public List<float> VertexList { get; set; } = new List<float>();
        public float[] Vertices => VertexList.ToArray();
    }

    private List<RenderGroup> CollectRenderGroups()
    {
        var groups = new Dictionary<string, RenderGroup>();

        float cameraX = Camera?.Position.X ?? 0f;
        float cameraY = Camera?.Position.Y ?? 0f;

        void CollectFrom(GameObject obj)
        {
            if (!obj.IsActive) return;

            var sprite = obj.GetComponent<SpriteRenderer>();
            if (sprite != null)
            {
                string textureKey = sprite.TexturePath ?? "";

                if (!groups.ContainsKey(textureKey))
                {
                    groups[textureKey] = new RenderGroup { TexturePath = textureKey };
                }

                var group = groups[textureKey];

                float x = obj.Transform.X - cameraX;
                float y = obj.Transform.Y - cameraY;
                float w = sprite.Width * obj.Transform.ScaleX;
                float h = sprite.Height * obj.Transform.ScaleY;
                float r = sprite.Color.X;
                float g = sprite.Color.Y;
                float b = sprite.Color.Z;

                float nx = 0f, ny = 0f, nz = 1f;

                group.VertexList.Add(x); group.VertexList.Add(y + h);
                group.VertexList.Add(r); group.VertexList.Add(g); group.VertexList.Add(b);
                group.VertexList.Add(nx); group.VertexList.Add(ny); group.VertexList.Add(nz);
                group.VertexList.Add(0f); group.VertexList.Add(1f);

                group.VertexList.Add(x + w); group.VertexList.Add(y + h);
                group.VertexList.Add(r); group.VertexList.Add(g); group.VertexList.Add(b);
                group.VertexList.Add(nx); group.VertexList.Add(ny); group.VertexList.Add(nz);
                group.VertexList.Add(1f); group.VertexList.Add(1f);

                group.VertexList.Add(x + w); group.VertexList.Add(y);
                group.VertexList.Add(r); group.VertexList.Add(g); group.VertexList.Add(b);
                group.VertexList.Add(nx); group.VertexList.Add(ny); group.VertexList.Add(nz);
                group.VertexList.Add(1f); group.VertexList.Add(0f);

                group.VertexList.Add(x); group.VertexList.Add(y + h);
                group.VertexList.Add(r); group.VertexList.Add(g); group.VertexList.Add(b);
                group.VertexList.Add(nx); group.VertexList.Add(ny); group.VertexList.Add(nz);
                group.VertexList.Add(0f); group.VertexList.Add(1f);

                group.VertexList.Add(x + w); group.VertexList.Add(y);
                group.VertexList.Add(r); group.VertexList.Add(g); group.VertexList.Add(b);
                group.VertexList.Add(nx); group.VertexList.Add(ny); group.VertexList.Add(nz);
                group.VertexList.Add(1f); group.VertexList.Add(0f);

                group.VertexList.Add(x); group.VertexList.Add(y);
                group.VertexList.Add(r); group.VertexList.Add(g); group.VertexList.Add(b);
                group.VertexList.Add(nx); group.VertexList.Add(ny); group.VertexList.Add(nz);
                group.VertexList.Add(0f); group.VertexList.Add(0f);
            }

            foreach (var child in obj.GetChildren())
            {
                CollectFrom(child);
            }
        }

        foreach (var root in _rootObjects)
        {
            CollectFrom(root);
        }

        return groups.Values.ToList();
    }

    public void OnResize(int width, int height)
    {
        if (_gl is null) return;

        _width = width;
        _height = height;
        _gl.Viewport(0, 0, (uint)width, (uint)height);
        UpdateProjection();
    }

    public void Cleanup()
    {
        if (_gl is null) return;

        foreach (var texture in _textureCache.Values)
        {
            _gl.DeleteTexture(texture);
        }
        _textureCache.Clear();

        ShaderUtils.DeleteProgram(_gl, ref _program, ref _vertexShader, ref _fragmentShader);

        if (_vbo != 0)
        {
            _gl.DeleteBuffer(_vbo);
            _vbo = 0;
        }

        if (_vao != 0)
        {
            _gl.DeleteVertexArray(_vao);
            _vao = 0;
        }

        _gl = null;
    }

    private void UpdateProjection()
    {
        if (_gl is null) return;

        _projection = MathUtils.CreateOrtho(0f, _width, _height, 0f, -1f, 1f);

        if (_uProjectionLocation >= 0)
        {
            _gl.UseProgram(_program);
            _gl.UniformMatrix4(_uProjectionLocation, 1, false, ref _projection[0]);
            _gl.UseProgram(0);
        }
    }
}
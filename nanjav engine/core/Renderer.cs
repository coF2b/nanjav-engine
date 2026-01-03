using System;
using System.Collections.Generic;
using Silk.NET.OpenGL;
using nanjav.core;

namespace nanjav.core
{
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

        private List<GameObject> _rootObjects = new List<GameObject>();

        public void Load(GL gl, int width, int height)
        {
            _gl = gl ?? throw new ArgumentNullException(nameof(gl));
            _width = width;
            _height = height;

            _gl.ClearColor(0.10f, 0.12f, 0.15f, 1.0f);

            const string vertexSource = @"#version 330 core
            layout(location = 0) in vec2 aPos;
            layout(location = 1) in vec3 aColor;

            out vec3 vColor;

            uniform mat4 u_Projection;

            void main()
            {
                vColor = aColor;
                gl_Position = u_Projection * vec4(aPos.xy, 0.0, 1.0);
            }";

            const string fragmentSource = @"#version 330 core
            in vec3 vColor;
            out vec4 FragColor;

            void main()
            {
                FragColor = vec4(vColor, 1.0);
            }";

            _vertexShader = CompileShader(ShaderType.VertexShader, vertexSource);
            _fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentSource);

            _program = _gl.CreateProgram();
            _gl.AttachShader(_program, _vertexShader);
            _gl.AttachShader(_program, _fragmentShader);
            _gl.LinkProgram(_program);

            _gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int linkStatus);
            if (linkStatus == 0)
            {
                string log = _gl.GetProgramInfoLog(_program) ?? string.Empty;
                throw new Exception($"Program link error: {log}");
            }

            _uProjectionLocation = _gl.GetUniformLocation(_program, "u_Projection");

            _vao = _gl.GenVertexArray();
            _vbo = _gl.GenBuffer();

            _gl.BindVertexArray(_vao);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)0, (float[]?)null, BufferUsageARB.DynamicDraw);

            uint stride = (uint)(5 * sizeof(float));
            _gl.EnableVertexAttribArray(0);
            _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, (nint)0);

            _gl.EnableVertexAttribArray(1);
            _gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, (nint)(2 * sizeof(float)));

            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            _gl.BindVertexArray(0);

            _gl.Enable(EnableCap.Blend);
            _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            UpdateProjection();
            _gl.Viewport(0, 0, (uint)_width, (uint)_height);
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

            var vertices = CollectVertices();

            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), vertices, BufferUsageARB.DynamicDraw);

            _gl.UseProgram(_program);
            _gl.BindVertexArray(_vao);
            _gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)(vertices.Length / 5));
            _gl.BindVertexArray(0);
            _gl.UseProgram(0);

            foreach (var obj in _rootObjects)
            {
                obj.Render();
            }
        }

        private float[] CollectVertices()
        {
            var vertexList = new List<float>();

            void CollectFrom(GameObject obj)
            {
                if (!obj.IsActive) return;

                var sprite = obj.GetComponent<SpriteRenderer>();
                if (sprite != null)
                {
                    float x = obj.Transform.X;
                    float y = obj.Transform.Y;
                    float w = sprite.Width * obj.Transform.ScaleX;
                    float h = sprite.Height * obj.Transform.ScaleY;
                    float r = sprite.Color.X;
                    float g = sprite.Color.Y;
                    float b = sprite.Color.Z;

                    vertexList.Add(x); vertexList.Add(y + h); vertexList.Add(r); vertexList.Add(g); vertexList.Add(b);
                    vertexList.Add(x + w); vertexList.Add(y + h); vertexList.Add(r); vertexList.Add(g); vertexList.Add(b);
                    vertexList.Add(x + w); vertexList.Add(y); vertexList.Add(r); vertexList.Add(g); vertexList.Add(b);

                    vertexList.Add(x); vertexList.Add(y + h); vertexList.Add(r); vertexList.Add(g); vertexList.Add(b);
                    vertexList.Add(x + w); vertexList.Add(y); vertexList.Add(r); vertexList.Add(g); vertexList.Add(b);
                    vertexList.Add(x); vertexList.Add(y); vertexList.Add(r); vertexList.Add(g); vertexList.Add(b);
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

            return vertexList.ToArray();
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

            if (_program != 0)
            {
                if (_vertexShader != 0)
                {
                    _gl.DetachShader(_program, _vertexShader);
                    _gl.DeleteShader(_vertexShader);
                    _vertexShader = 0;
                }

                if (_fragmentShader != 0)
                {
                    _gl.DetachShader(_program, _fragmentShader);
                    _gl.DeleteShader(_fragmentShader);
                    _fragmentShader = 0;
                }

                _gl.DeleteProgram(_program);
                _program = 0;
            }

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

        private uint CompileShader(ShaderType type, string source)
        {
            if (_gl is null)
                throw new InvalidOperationException("GL not initialized");

            var shader = _gl.CreateShader(type);
            _gl.ShaderSource(shader, source);
            _gl.CompileShader(shader);

            _gl.GetShader(shader, ShaderParameterName.CompileStatus, out int status);
            if (status == 0)
            {
                string log = _gl.GetShaderInfoLog(shader) ?? string.Empty;
                _gl.DeleteShader(shader);
                throw new Exception($"Shader compilation error ({type}): {log}");
            }

            return shader;
        }

        private void UpdateProjection()
        {
            if (_gl is null) return;

            _projection = CreateOrtho(0f, _width, _height, 0f, -1f, 1f);

            if (_uProjectionLocation >= 0)
            {
                _gl.UseProgram(_program);
                _gl.UniformMatrix4(_uProjectionLocation, 1, false, ref _projection[0]);
                _gl.UseProgram(0);
            }
        }

        private float[] CreateOrtho(float left, float right, float bottom, float top, float near, float far)
        {
            var m = new float[16];

            float rl = 1.0f / (right - left);
            float tb = 1.0f / (top - bottom);
            float fn = 1.0f / (far - near);

            m[0] = 2f * rl;
            m[1] = 0f;
            m[2] = 0f;
            m[3] = 0f;

            m[4] = 0f;
            m[5] = 2f * tb;
            m[6] = 0f;
            m[7] = 0f;

            m[8] = 0f;
            m[9] = 0f;
            m[10] = -2f * fn;
            m[11] = 0f;

            m[12] = -(right + left) * rl;
            m[13] = -(top + bottom) * tb;
            m[14] = -(far + near) * fn;
            m[15] = 1f;

            return m;
        }
    }
}
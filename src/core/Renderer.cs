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

            _program = ShaderUtils.CreateProgram(_gl, vertexSource, fragmentSource, out _vertexShader, out _fragmentShader);

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

            float cameraX = Camera?.Position.X ?? 0f;
            float cameraY = Camera?.Position.Y ?? 0f;

            void CollectFrom(GameObject obj)
            {
                if (!obj.IsActive) return;

                var sprite = obj.GetComponent<SpriteRenderer>();
                if (sprite != null)
                {
                    float x = obj.Transform.X - cameraX;
                    float y = obj.Transform.Y - cameraY;
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
}

using System;
using Silk.NET.OpenGL;

namespace nanjav.core
{
    internal static class ShaderUtils
    {
        public static uint CompileShader(GL gl, ShaderType type, string source)
        {
            if (gl is null)
                throw new ArgumentNullException(nameof(gl));

            var shader = gl.CreateShader(type);
            gl.ShaderSource(shader, source);
            gl.CompileShader(shader);

            gl.GetShader(shader, ShaderParameterName.CompileStatus, out int status);
            if (status == 0)
            {
                string log = gl.GetShaderInfoLog(shader) ?? string.Empty;
                gl.DeleteShader(shader);
                throw new Exception($"Shader compilation error ({type}): {log}");
            }

            return shader;
        }

        public static uint CreateProgram(GL gl, string vertexSource, string fragmentSource, out uint vertexShader, out uint fragmentShader)
        {
            vertexShader = CompileShader(gl, ShaderType.VertexShader, vertexSource);
            fragmentShader = CompileShader(gl, ShaderType.FragmentShader, fragmentSource);

            var program = gl.CreateProgram();
            gl.AttachShader(program, vertexShader);
            gl.AttachShader(program, fragmentShader);
            gl.LinkProgram(program);

            gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int linkStatus);
            if (linkStatus == 0)
            {
                string log = gl.GetProgramInfoLog(program) ?? string.Empty;
                // cleanup on failure
                if (vertexShader != 0) gl.DeleteShader(vertexShader);
                if (fragmentShader != 0) gl.DeleteShader(fragmentShader);
                if (program != 0) gl.DeleteProgram(program);

                throw new Exception($"Program link error: {log}");
            }

            return program;
        }

        public static void DeleteProgram(GL gl, ref uint program, ref uint vertexShader, ref uint fragmentShader)
        {
            if (gl is null) return;

            if (program != 0)
            {
                if (vertexShader != 0)
                {
                    gl.DetachShader(program, vertexShader);
                    gl.DeleteShader(vertexShader);
                    vertexShader = 0;
                }

                if (fragmentShader != 0)
                {
                    gl.DetachShader(program, fragmentShader);
                    gl.DeleteShader(fragmentShader);
                    fragmentShader = 0;
                }

                gl.DeleteProgram(program);
                program = 0;
            }
        }
    }
}

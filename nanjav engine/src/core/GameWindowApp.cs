using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System;
using System.Numerics;
using nanjav.core;
using nanjav.input;

namespace nanjav.core
{
    public class GameWindowApp : IDisposable
    {
        private IWindow _window = null!;
        private GL? _gl;
        private Renderer _renderer = null!;

        public Keyboard Keyboard { get; private set; } = null!;
        public Mouse Mouse { get; private set; } = null!;
        public Renderer Renderer => _renderer;
        public int Width => _window.Size.X;
        public int Height => _window.Size.Y;
        public double Time => _window.Time;
        public bool IsRunning { get; private set; }

        public event Action? OnLoad;
        public event Action<double>? OnUpdate;
        public event Action<double>? OnRender;
        public event Action<int, int>? OnResize;
        public event Action? OnClose;

        public GameWindowApp(string title = "nanjav Game", int width = 800, int height = 600, bool vSync = true)
        {
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(width, height);
            options.Title = title;
            options.VSync = vSync;
            options.API = new GraphicsAPI(ContextAPI.OpenGL, new APIVersion(3, 3));

            _window = Window.Create(options);
            _renderer = new Renderer();
            Keyboard = new nanjav.input.Keyboard();
            Mouse = new nanjav.input.Mouse();

            _window.Load += OnWindowLoad;
            _window.Update += OnUpdateFrame;
            _window.Render += OnRenderFrame;
            _window.Resize += OnWindowResize;
            _window.Closing += OnWindowClose;
        }

        private void OnWindowLoad()
        {
            _gl = GL.GetApi(_window);
            SetupInput();
            _renderer.Load(_gl, Width, Height);
            CenterWindow();
            IsRunning = true;
            OnLoad?.Invoke();
        }

        private void SetupInput()
        {
            var input = _window.CreateInput();

            if (input.Keyboards.Count > 0)
            {
                var keyboard = input.Keyboards[0];
                keyboard.KeyDown += (kb, key, scancode) =>
                {
                    var neoKey = (nanjav.input.Keys)(int)key;
                    Keyboard.KeyDown(neoKey);
                };

                keyboard.KeyUp += (kb, key, scancode) =>
                {
                    var neoKey = (nanjav.input.Keys)(int)key;
                    Keyboard.KeyUp(neoKey);
                };
            }

            if (input.Mice.Count > 0)
            {
                var mouse = input.Mice[0];
                mouse.MouseDown += (m, button) =>
                {
                    var neoButton = (nanjav.input.MouseButton)(int)button;
                    Mouse.ButtonDown(neoButton);
                };
                mouse.MouseUp += (m, button) =>
                {
                    var neoButton = (nanjav.input.MouseButton)(int)button;
                    Mouse.ButtonUp(neoButton);
                };
                mouse.MouseMove += (m, position) =>
                {
                    Mouse.UpdatePosition(new Vector2(position.X, position.Y));
                };
                mouse.Scroll += (m, wheel) =>
                {
                    Mouse.UpdateScroll(wheel.Y);
                };
            }
        }

        private void OnUpdateFrame(double deltaTime)
        {
            Keyboard.Update();
            Mouse.Update();

            if (Keyboard.IsKeyPressed(Keys.Escape))
                Close();

            _renderer.Update(deltaTime);
            OnUpdate?.Invoke(deltaTime);
        }

        private void OnRenderFrame(double deltaTime)
        {
            _renderer.Render(deltaTime);
            OnRender?.Invoke(deltaTime);
        }

        private void OnWindowResize(Vector2D<int> newSize)
        {
            if (_gl is not null)
                _gl.Viewport(0, 0, (uint)newSize.X, (uint)newSize.Y);
            _renderer?.OnResize(newSize.X, newSize.Y);
            OnResize?.Invoke(newSize.X, newSize.Y);
        }

        private void OnWindowClose()
        {
            IsRunning = false;
            _renderer?.Cleanup();
            OnClose?.Invoke();
        }

        public void CenterWindow()
        {
            var monitor = _window.Monitor;
            if (monitor != null)
            {
                var videoMode = monitor.VideoMode;
                _window.Position = new Vector2D<int>(
                    (videoMode.Resolution.Value.X - _window.Size.X) / 2,
                    (videoMode.Resolution.Value.Y - _window.Size.Y) / 2
                );
            }
        }

        public void SetTitle(string title) => _window.Title = title;
        public void SetSize(int width, int height) => _window.Size = new Vector2D<int>(width, height);

        public void ToggleFullscreen()
        {
            _window.WindowState = _window.WindowState == WindowState.Fullscreen
                ? WindowState.Normal
                : WindowState.Fullscreen;
        }

        public void Run() => _window.Run();
        public void Close() => _window.Close();

        public void Dispose()
        {
            _renderer?.Cleanup();
            _window?.Dispose();
            _gl?.Dispose();
        }

        public void AddGameObject(GameObject obj)
        {
            _renderer.AddRootObject(obj);
        }
    }
}
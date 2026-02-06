using Silk.NET.OpenGL;

public class Data
{
    public string Version = "0.1.4 Alpha-2";
    public string OSver = Environment.OSVersion.ToString();
    public string TimeNow = DateTime.Now.ToString();

    public string GLVersion { get; }
    public string GLVendor { get; }
    public string GLRenderer { get; }
    public string GLSLVersion { get; }

    public double FPS { get; private set; } = 0.0;

    private double _frameTimeAccumulator = 0.0;
    private int _frameCount = 0;
    private const double UpdateInterval = 0.1;

    public Data(GL gl)
    {
        if (gl == null)
        {
            GLVersion = GLVendor = GLRenderer = GLSLVersion = "GL context not available";
            return;
        }

        try
        {
            GLVersion = gl.GetStringS(StringName.Version) ?? "Unknown";
            GLVendor = gl.GetStringS(StringName.Vendor) ?? "Unknown";
            GLRenderer = gl.GetStringS(StringName.Renderer) ?? "Unknown";
            GLSLVersion = gl.GetStringS(StringName.ShadingLanguageVersion) ?? "Unknown";
        }
        catch (Exception ex)
        {
            GLVersion = $"Error: {ex.Message}";
        }
    }

    public void UpdateFPS(double deltaTime)
    {
        _frameTimeAccumulator += deltaTime;
        _frameCount++;

        if (_frameTimeAccumulator >= UpdateInterval)
        {
            FPS = _frameCount / _frameTimeAccumulator;
            _frameTimeAccumulator = 0.0;
            _frameCount = 0;
        }
    }
}
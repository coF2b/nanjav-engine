using Silk.NET.OpenGL;

public class Data
{
    public string Version = "0.1.3.1";
    public string OSver = Environment.OSVersion.ToString();
    public string TimeNow = DateTime.Now.ToString();

    public string GLVersion { get; }
    public string GLVendor { get; }
    public string GLRenderer { get; }
    public string GLSLVersion { get; }

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
}
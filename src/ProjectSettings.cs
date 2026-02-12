using System.Text.Json;
using System.Text.Json.Serialization;
using System.Numerics;

namespace nanjav.core;

public class ProjectSettings
{
    private static ProjectSettings? _instance;
    private static readonly string DefaultFilePath = "ProjectSettings.json";

    public static ProjectSettings Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = LoadOrCreate();
            }
            return _instance;
        }
    }

    [JsonPropertyName("window")]
    public WindowSettings Window { get; set; } = new WindowSettings();

    [JsonPropertyName("rendering")]
    public RenderingSettings Rendering { get; set; } = new RenderingSettings();

    [JsonPropertyName("physics")]
    public PhysicsSettings Physics { get; set; } = new PhysicsSettings();

    [JsonPropertyName("game")]
    public GameSettings Game { get; set; } = new GameSettings();

    public void Save(string filePath = "")
    {
        if (string.IsNullOrEmpty(filePath))
            filePath = DefaultFilePath;

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };

        string jsonString = JsonSerializer.Serialize(this, options);
        File.WriteAllText(filePath, jsonString);
    }

    public static ProjectSettings Load(string filePath = "")
    {
        if (string.IsNullOrEmpty(filePath))
            filePath = DefaultFilePath;

        if (!File.Exists(filePath))
        {
            return new ProjectSettings();
        }

        try
        {
            string jsonString = File.ReadAllText(filePath);
            var settings = JsonSerializer.Deserialize<ProjectSettings>(jsonString);

            if (settings == null)
            {
                return new ProjectSettings();
            }

            return settings;
        }
        catch (Exception)
        {
            return new ProjectSettings();
        }
    }

    private static ProjectSettings LoadOrCreate(string filePath = "")
    {
        if (string.IsNullOrEmpty(filePath))
            filePath = DefaultFilePath;

        if (File.Exists(filePath))
        {
            return Load(filePath);
        }
        else
        {
            var settings = new ProjectSettings();
            settings.Save(filePath);
            return settings;
        }
    }

    public void ResetToDefault()
    {
        Window = new WindowSettings();
        Rendering = new RenderingSettings();
        Physics = new PhysicsSettings();
        Game = new GameSettings();
    }

    public void ApplyToGame(GameWindowApp game)
    {
        game.SetTitle(Window.Title);
        game.SetSize(Window.Width, Window.Height);

        if (Window.Fullscreen)
            game.ToggleFullscreen();

        if (Window.CenterOnStart)
            game.CenterWindow();
    }
}

public class WindowSettings
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = "nanjav Game";

    [JsonPropertyName("width")]
    public int Width { get; set; } = 1280;

    [JsonPropertyName("height")]
    public int Height { get; set; } = 720;

    [JsonPropertyName("fullscreen")]
    public bool Fullscreen { get; set; } = false;

    [JsonPropertyName("vsync")]
    public bool VSync { get; set; } = true;

    [JsonPropertyName("resizable")]
    public bool Resizable { get; set; } = true;

    [JsonPropertyName("centerOnStart")]
    public bool CenterOnStart { get; set; } = true;
}

public class RenderingSettings
{
    [JsonPropertyName("clearColorR")]
    public float ClearColorR { get; set; } = 0.1f;

    [JsonPropertyName("clearColorG")]
    public float ClearColorG { get; set; } = 0.1f;

    [JsonPropertyName("clearColorB")]
    public float ClearColorB { get; set; } = 0.1f;

    [JsonPropertyName("clearColorA")]
    public float ClearColorA { get; set; } = 1.0f;

    [JsonPropertyName("targetFPS")]
    public int TargetFPS { get; set; } = 60;

    [JsonPropertyName("antialiasing")]
    public bool Antialiasing { get; set; } = true;

    [JsonPropertyName("defaultCameraZoom")]
    public float DefaultCameraZoom { get; set; } = 1.0f;

    [JsonIgnore]
    public Vector4 ClearColor => new Vector4(ClearColorR, ClearColorG, ClearColorB, ClearColorA);
}

public class PhysicsSettings
{
    [JsonPropertyName("gravity")]
    public float Gravity { get; set; } = 9.81f;

    [JsonPropertyName("maxVelocityX")]
    public float MaxVelocityX { get; set; } = 500f;

    [JsonPropertyName("maxVelocityY")]
    public float MaxVelocityY { get; set; } = 1000f;

    [JsonPropertyName("defaultMass")]
    public float DefaultMass { get; set; } = 1.0f;

    [JsonPropertyName("defaultFriction")]
    public float DefaultFriction { get; set; } = 0.1f;

    [JsonPropertyName("fixedTimeStep")]
    public double FixedTimeStep { get; set; } = 1.0 / 60.0;
}

public class GameSettings
{
    [JsonPropertyName("projectName")]
    public string ProjectName { get; set; } = "My nanjav Project";

    [JsonPropertyName("version")]
    public string Version { get; set; } = "0.1.0";

    [JsonPropertyName("company")]
    public string Company { get; set; } = "";

    [JsonPropertyName("debugMode")]
    public bool DebugMode { get; set; } = true;

    [JsonPropertyName("showFPS")]
    public bool ShowFPS { get; set; } = true;

    [JsonPropertyName("startScene")]
    public string StartScene { get; set; } = "MainScene";
}

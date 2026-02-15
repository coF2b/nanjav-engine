// A component for playing sound (2D or 3D), controlling volume, looping, and 
using System.Numerics;

namespace nanjav.core;

public class AudioSource : Component
{
    private static AudioSystem? _audioSystem;

    public string? SoundName { get; set; }
    public float Volume { get; set; } = 1.0f;
    public float Pitch { get; set; } = 1.0f;
    public bool Loop { get; set; } = false;
    public bool PlayOnStart { get; set; } = false;

    public bool Is3D { get; set; } = false;
    public float MinDistance { get; set; } = 1.0f;
    public float MaxDistance { get; set; } = 100.0f;

    private bool _isPlaying = false;
    private bool _hasStarted = false;

    public static void SetAudioSystem(AudioSystem audioSystem)
    {
        _audioSystem = audioSystem;
    }

    public void Initialize()
    {
        if (!_hasStarted)
        {
            _hasStarted = true;
            if (PlayOnStart && !string.IsNullOrEmpty(SoundName))
            {
                Play();
            }
        }
    }

    public void Update(double deltaTime)
    {
        Initialize();
    }

    public void Play()
    {
        if (_audioSystem == null)
        {
            Console.WriteLine("AudioSource: AudioSystem not initialized");
            return;
        }

        if (string.IsNullOrEmpty(SoundName))
        {
            Console.WriteLine("AudioSource: SoundName not set");
            return;
        }

        if (Is3D && GameObject != null)
        {
            Vector3 position = new Vector3(
                GameObject.Transform.X,
                GameObject.Transform.Y,
                0f
            );
            _audioSystem.PlaySound3D(SoundName, position, Volume, Loop);
        }
        else
        {
            _audioSystem.PlaySound(SoundName, Volume, Pitch, Loop);
        }

        _isPlaying = true;
    }

    public void Stop()
    {
        if (_audioSystem != null)
        {
            _audioSystem.StopAllSounds();
        }
        _isPlaying = false;
    }

    public bool IsPlaying()
    {
        return _isPlaying;
    }
}
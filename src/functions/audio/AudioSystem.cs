using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Silk.NET.OpenAL;

namespace nanjav.audio
{
    public class AudioSystem : IDisposable
    {
        private AL _al;
        private ALContext _alc;
        private unsafe Device* _device;
        private unsafe Context* _context;
        private bool _disposed = false;

        private Dictionary<string, uint> _audioBuffers = new();
        private List<uint> _activeSources = new();

        public AudioSystem()
        {
            Initialize();
        }

        private unsafe void Initialize()
        {
            _al = AL.GetApi();
            _alc = ALContext.GetApi();

            _device = _alc.OpenDevice("");
            if (_device == null)
            {
                throw new Exception("Could not open OpenAL audio device");
            }

            _context = _alc.CreateContext(_device, null);
            if (_context == null)
            {
                _alc.CloseDevice(_device);
                throw new Exception("Unable to create OpenAL context");
            }

            _alc.MakeContextCurrent(_context);
            CheckALError("Initialize");

            Console.WriteLine("AudioSystem: OpenAL initialized successfully");
        }

        public bool LoadSound(string name, string filePath)
        {
            if (_audioBuffers.ContainsKey(name))
            {
                Console.WriteLine($"AudioSystem: Sound '{name}' already loaded");
                return true;
            }

            try
            {
                var wavData = LoadWav(filePath, out int channels, out int sampleRate, out int bitsPerSample);

                uint buffer = _al.GenBuffer();
                CheckALError("GenBuffer");

                BufferFormat format = GetBufferFormat(channels, bitsPerSample);

                _al.BufferData(buffer, format, wavData, sampleRate);
                CheckALError("BufferData");

                _audioBuffers[name] = buffer;
                Console.WriteLine($"AudioSystem: Sound '{name}' loaded successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AudioSystem: Audio loading error '{name}': {ex.Message}");
                return false;
            }
        }

        public void PlaySound(string name, float volume = 1.0f, float pitch = 1.0f, bool loop = false)
        {
            if (!_audioBuffers.ContainsKey(name))
            {
                Console.WriteLine($"AudioSystem: Sound '{name}' not found");
                return;
            }

            uint source = _al.GenSource();
            CheckALError("GenSource");

            _al.SetSourceProperty(source, SourceInteger.Buffer, (int)_audioBuffers[name]);
            _al.SetSourceProperty(source, SourceFloat.Gain, volume);
            _al.SetSourceProperty(source, SourceFloat.Pitch, pitch);
            _al.SetSourceProperty(source, SourceBoolean.Looping, loop);

            _al.SourcePlay(source);
            CheckALError("SourcePlay");

            _activeSources.Add(source);
        }

        public void PlaySound3D(string name, Vector3 position, float volume = 1.0f, bool loop = false)
        {
            if (!_audioBuffers.ContainsKey(name))
            {
                Console.WriteLine($"AudioSystem: Sound '{name}' not found");
                return;
            }

            uint source = _al.GenSource();
            CheckALError("GenSource");

            _al.SetSourceProperty(source, SourceInteger.Buffer, (int)_audioBuffers[name]);
            _al.SetSourceProperty(source, SourceFloat.Gain, volume);
            _al.SetSourceProperty(source, SourceBoolean.Looping, loop);

            _al.SetSourceProperty(source, SourceVector3.Position, position.X, position.Y, position.Z);

            _al.SourcePlay(source);
            CheckALError("SourcePlay");

            _activeSources.Add(source);
        }

        public void SetListenerPosition(Vector3 position)
        {
            _al.SetListenerProperty(ListenerVector3.Position, position.X, position.Y, position.Z);
            CheckALError("SetListenerPosition");
        }

        public void SetListenerOrientation(Vector3 forward, Vector3 up)
        {
            float[] orientation = new float[]
            {
                forward.X, forward.Y, forward.Z,
                up.X, up.Y, up.Z
            };

            unsafe
            {
                fixed (float* ptr = orientation)
                {
                    _al.SetListenerProperty(ListenerFloatArray.Orientation, ptr);
                }
            }
            CheckALError("SetListenerOrientation");
        }

        public void Update()
        {
            for (int i = _activeSources.Count - 1; i >= 0; i--)
            {
                uint source = _activeSources[i];
                _al.GetSourceProperty(source, GetSourceInteger.SourceState, out int state);

                if (state != (int)SourceState.Playing)
                {
                    _al.DeleteSource(source);
                    _activeSources.RemoveAt(i);
                }
            }
        }

        public void StopAllSounds()
        {
            foreach (var source in _activeSources)
            {
                _al.SourceStop(source);
                _al.DeleteSource(source);
            }
            _activeSources.Clear();
        }

        public void SetMasterVolume(float volume)
        {
            _al.SetListenerProperty(ListenerFloat.Gain, Math.Clamp(volume, 0f, 1f));
            CheckALError("SetMasterVolume");
        }

        private BufferFormat GetBufferFormat(int channels, int bitsPerSample)
        {
            if (channels == 1)
                return bitsPerSample == 8 ? BufferFormat.Mono8 : BufferFormat.Mono16;
            else
                return bitsPerSample == 8 ? BufferFormat.Stereo8 : BufferFormat.Stereo16;
        }

        private byte[] LoadWav(string filePath, out int channels, out int sampleRate, out int bitsPerSample)
        {
            using var stream = File.OpenRead(filePath);
            using var reader = new BinaryReader(stream);

            string signature = new string(reader.ReadChars(4));
            if (signature != "RIFF")
                throw new Exception("Invalid WAV file: missing RIFF header");

            reader.ReadInt32();
            string format = new string(reader.ReadChars(4));
            if (format != "WAVE")
                throw new Exception("Invalid WAV file: missing WAVE format");

            string fmtSignature = new string(reader.ReadChars(4));
            if (fmtSignature != "fmt ")
                throw new Exception("Invalid WAV file: missing fmt chunk");

            int fmtChunkSize = reader.ReadInt32();
            reader.ReadInt16();
            channels = reader.ReadInt16();
            sampleRate = reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt16();
            bitsPerSample = reader.ReadInt16();

            if (fmtChunkSize > 16)
                reader.ReadBytes(fmtChunkSize - 16);

            string dataSignature = new string(reader.ReadChars(4));
            while (dataSignature != "data" && stream.Position < stream.Length)
            {
                int chunkSize = reader.ReadInt32();
                reader.ReadBytes(chunkSize);
                if (stream.Position >= stream.Length)
                    break;
                dataSignature = new string(reader.ReadChars(4));
            }

            if (dataSignature != "data")
                throw new Exception("Invalid WAV file: missing data chunk");

            int dataSize = reader.ReadInt32();
            return reader.ReadBytes(dataSize);
        }

        private void CheckALError(string operation)
        {
            AudioError error = _al.GetError();
            if (error != AudioError.NoError)
            {
                Console.WriteLine($"AudioSystem OpenAL Error in {operation}: {error}");
            }
        }

        public unsafe void Dispose()
        {
            if (_disposed) return;

            StopAllSounds();

            foreach (var buffer in _audioBuffers.Values)
            {
                _al.DeleteBuffer(buffer);
            }
            _audioBuffers.Clear();

            _alc.MakeContextCurrent(null);
            _alc.DestroyContext(_context);
            _alc.CloseDevice(_device);

            _disposed = true;
            Console.WriteLine("AudioSystem: Resources released");
        }
    }
}
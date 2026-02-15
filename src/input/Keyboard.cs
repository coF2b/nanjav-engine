// Handles keyboard state (current/previous), allowing checks for key presses, holds, and releases.
namespace nanjav.core;

public class Keyboard
{
    private HashSet<Keys> _currentKeys = new();
    private HashSet<Keys> _previousKeys = new();

    public void Update()
    {
        _previousKeys = new HashSet<Keys>(_currentKeys);
    }

    internal void KeyDown(Keys key)
    {
        _currentKeys.Add(key);
    }

    internal void KeyUp(Keys key)
    {
        _currentKeys.Remove(key);
    }

    public bool IsKeyDown(Keys key)
    {
        return _currentKeys.Contains(key);
    }

    public bool IsKeyPressed(Keys key)
    {
        return _currentKeys.Contains(key) && !_previousKeys.Contains(key);
    }

    public bool IsKeyReleased(Keys key)
    {
        return !_currentKeys.Contains(key) && _previousKeys.Contains(key);
    }

    public bool IsAnyKeyDown()
    {
        return _currentKeys.Count > 0;
    }

    public IReadOnlySet<Keys> GetPressedKeys()
    {
        return _currentKeys;
    }
}
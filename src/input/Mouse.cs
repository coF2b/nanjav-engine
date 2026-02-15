// Tracks mouse position, button states, movement delta, and scroll wheel input.
using System.Numerics;

namespace nanjav.core;

public class Mouse
{
    private HashSet<MouseButton> _currentButtons = new();
    private HashSet<MouseButton> _previousButtons = new();

    private Vector2 _currentPosition;
    private Vector2 _previousPosition;

    private float _scrollDelta;
    private float _previousScrollDelta;

    public Vector2 Position => _currentPosition;

    public Vector2 PreviousPosition => _previousPosition;

    public Vector2 Delta => _currentPosition - _previousPosition;

    public float ScrollDelta => _scrollDelta;

    public void Update()
    {
        _previousButtons = new HashSet<MouseButton>(_currentButtons);
        _previousPosition = _currentPosition;
        _previousScrollDelta = _scrollDelta;
        _scrollDelta = 0;
    }

    internal void ButtonDown(MouseButton button)
    {
        _currentButtons.Add(button);
    }

    internal void ButtonUp(MouseButton button)
    {
        _currentButtons.Remove(button);
    }

    internal void UpdatePosition(Vector2 position)
    {
        _currentPosition = position;
    }

    internal void UpdateScroll(float delta)
    {
        _scrollDelta += delta;
    }

    public bool IsButtonDown(MouseButton button)
    {
        return _currentButtons.Contains(button);
    }

    public bool IsButtonPressed(MouseButton button)
    {
        return _currentButtons.Contains(button) && !_previousButtons.Contains(button);
    }

    public bool IsButtonReleased(MouseButton button)
    {
        return !_currentButtons.Contains(button) && _previousButtons.Contains(button);
    }

    public bool IsAnyButtonDown()
    {
        return _currentButtons.Count > 0;
    }

    public IReadOnlySet<MouseButton> GetPressedButtons()
    {
        return _currentButtons;
    }

    public bool IsMoving()
    {
        return Delta.LengthSquared() > 0.001f;
    }

    public bool IsScrolling()
    {
        return System.Math.Abs(_scrollDelta) > 0.001f;
    }
}
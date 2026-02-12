using System.Numerics;

namespace nanjav.core;

public class Camera2D
{
    public Vector2 Position { get; set; } = Vector2.Zero;

    public float Zoom { get; set; } = 1.0f;

    public Vector2 Offset { get; set; } = Vector2.Zero;

    public Camera2D(float screenWidth, float screenHeight)
    {
        Offset = new Vector2(screenWidth / 2, screenHeight / 2);
    }

    public Matrix4x4 GetViewMatrix()
    {
        return Matrix4x4.CreateTranslation(new Vector3(-Position, 0)) * Matrix4x4.CreateScale(Zoom) * Matrix4x4.CreateTranslation(new Vector3(Offset, 0));
    }
}
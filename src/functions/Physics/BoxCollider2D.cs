// A rectangular collider component; maintains a list of all colliders and checks 
namespace nanjav.core;

public class BoxCollider2D : Component
{
    public static List<BoxCollider2D> AllColliders = new List<BoxCollider2D>();

    public float Width { get; set; } = 50f;
    public float Height { get; set; } = 50f;

    public BoxCollider2D()
    {
        AllColliders.Add(this);
    }

    public void Destroy()
    {
        AllColliders.Remove(this);
    }

    public float Left => Transform?.X ?? 0;
    public float Right => (Transform?.X ?? 0) + Width;
    public float Top => Transform?.Y ?? 0;
    public float Bottom => (Transform?.Y ?? 0) + Height;

    public bool IsColliding(BoxCollider2D other)
    {
        return !(Right < other.Left ||
                 Left > other.Right ||
                 Bottom < other.Top ||
                 Top > other.Bottom);
    }
}

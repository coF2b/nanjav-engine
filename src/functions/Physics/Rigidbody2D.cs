// Manages physics behavior of the object: movement under gravity, jumping, and collision handling
using System.Numerics;

namespace nanjav.core;

public class Rigidbody2D : Component
{
    public float Mass = 1.0f;
    public float GravityScale = 9.81f;
    public Vector2 Velocity = Vector2.Zero;
    public float ReflectionForce = 0.5f;
    float slopeThreshold = 0.7f;

    public void ApplyForce(Vector2 force)
    {
        Velocity += force;
    }

    public void Jump(float jumpForce)
    {
        Velocity.Y = jumpForce;
    }

    public override void Update(double deltaTime)
    {
        if (Transform == null) return;

        Velocity.Y += GravityScale * (float)deltaTime;

        float nextX = Transform.X + (Velocity.X * (float)deltaTime);
        float nextY = Transform.Y + (Velocity.Y * (float)deltaTime);

        var myCollider = GameObject?.GetComponent<BoxCollider2D>();

        if (myCollider != null)
        {
            bool grounded = false;

            float minCollisionDistance = float.MaxValue;
            BoxCollider2D? collidingWith = null;

            foreach (var other in BoxCollider2D.AllColliders.ToList())
            {
                if (other == null || other == myCollider) continue;
                if (!other.GameObject?.IsActive ?? false) continue;

                if (CheckCollision(Transform.X, nextY, myCollider, other))
                {
                    float distance = Math.Abs(other.Top - (nextY + myCollider.Height));
                    if (distance < minCollisionDistance)
                    {
                        minCollisionDistance = distance;
                        collidingWith = other;
                    }
                }
            }

            if (collidingWith != null)
            {
                if (Velocity.Y > 0)
                {
                    Transform.Y = collidingWith.Top - myCollider.Height;
                }
                else if (Velocity.Y < 0)
                {
                    Transform.Y = collidingWith.Bottom;
                }

                Velocity.Y = 0;
                grounded = true;
            }

            if (!grounded && Velocity.Y != 0)
            {
                Transform.Y = nextY;
            }

            bool canMoveX = true;
            foreach (var other in BoxCollider2D.AllColliders)
            {
                if (other == myCollider) continue;

                if (CheckCollision(nextX, Transform.Y, myCollider, other))
                {
                    canMoveX = false;
                    Velocity.X = 0;
                    break;
                }
            }

            if (canMoveX)
            {
                Transform.X = nextX;
            }
        }
        else
        {
            Transform.Y = nextY;
            Transform.X = nextX;
        }

    }

    private bool CheckCollision(float x, float y, BoxCollider2D me, BoxCollider2D other)
    {
        return x < other.Right &&
               x + me.Width > other.Left &&
               y < other.Bottom &&
               y + me.Height > other.Top;
    }
}
using nanjav.core;
using System.Numerics;
using System;

namespace nanjav.physics2D
{
    public class Rigidbody2D : Component
    {
        public float Mass = 1.0f;
        public float GravityScale = 9.7f;
        public Vector2 Velocity = Vector2.Zero;

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

                foreach (var other in BoxCollider2D.AllColliders)
                {
                    if (other == myCollider) continue;

                    if (Velocity.Y > 0 && CheckCollision(Transform.X, nextY, myCollider, other))
                    {
                        Transform.Y = other.Top - myCollider.Height;
                        Velocity.Y = 0;
                        grounded = true;
                        break;
                    }

                    if (Velocity.Y < 0 && CheckCollision(Transform.X, nextY, myCollider, other))
                    {
                        Transform.Y = other.Bottom;
                        Velocity.Y = 0;
                        break;
                    }
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
}
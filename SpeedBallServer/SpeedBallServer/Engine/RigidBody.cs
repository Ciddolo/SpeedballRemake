using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer
{
    public class RigidBody
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }

        public Rect BoundingBox { get; protected set; }

        //object owning this rigid body
        public GameObject GameObject { get; protected set; }
        public bool IsGravityAffected { get; set; }
        public bool IsCollisionsAffected { get; set; }
        
        public uint Type { get; set; }
        public uint CollisionMask { get; protected set; }

        private Vector2 gravity = new Vector2(4.0f, 4.0f);

        public RigidBody(Vector2 position, GameObject owner, bool collisionAffected = true, bool gravityAffected = false)
        {
            Position = position;
            GameObject = owner;

            IsCollisionsAffected = collisionAffected;
            IsGravityAffected = gravityAffected;

            BoundingBox = new Rect(this, GameObject.Width, GameObject.Height);
        }

        public void ResetCollisionMask()
        {
            CollisionMask = 0;
        }

        public void AddCollision(ColliderType mask)
        {
            AddCollision((uint)mask);
        }

        public void AddCollision(uint mask)
        {
            CollisionMask |= mask;
        }

        public bool CheckCollisionWith(RigidBody rb)
        {
            return (CollisionMask & rb.Type) != 0;
        }

        public bool Collides(RigidBody other, ref Collision collisionInfo)
        {

            if (BoundingBox != null && other.BoundingBox != null)
            {
                //rect vs rect
                return BoundingBox.Collides(other.BoundingBox, ref collisionInfo);
            }
            else
            {
                return false;
            }

        }

        public void AddVelocity(float vX, float vY)
        {
            AddVelocity(new Vector2(vX, vY));
        }

        public void AddVelocity(Vector2 newVelocity)
        {
            Velocity += newVelocity;
        }

        public void SetXVelocity(float vX)
        {
            Velocity = new Vector2(vX, Velocity.Y);
        }

        public void SetYVelocity(float vY)
        {
            Velocity = new Vector2(Velocity.X, vY);
        }

        public void Update(float deltaTime)
        {
            this.Position += Velocity * deltaTime;

            if (IsGravityAffected)
            {
                Vector2 gravityToApply = gravity * deltaTime;
                Vector2 normalizedAbsoluteVelocity = Vector2.Abs(Vector2.Normalize(Velocity));
                gravityToApply *= normalizedAbsoluteVelocity;

                if (Velocity.X > 0.05f)
                {
                    AddVelocity(-gravityToApply.X, 0.0f);
                }
                else if (Velocity.X < -0.05f)
                {
                    AddVelocity(gravityToApply.X, 0.0f);
                }
                else
                {
                    SetXVelocity(0.0f);
                }

                if (Velocity.Y > 0.05f)
                {
                    AddVelocity(0.0f, -gravityToApply.Y);
                }
                else if (Velocity.Y < -0.05f)
                {
                    AddVelocity(0.0f, gravityToApply.Y);
                }
                else
                {
                    SetYVelocity(0.0f);
                }
            }
        }
    }
}

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
        protected uint CollisionMask;

        public RigidBody(Vector2 position, GameObject owner, bool collisionAffected = true, bool gravityAffected = false)
        {
            Position = position;
            GameObject = owner;

            IsCollisionsAffected = collisionAffected;
            IsGravityAffected = gravityAffected;

            BoundingBox = new Rect(this, GameObject.Width, GameObject.Height);
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
            Velocity += new Vector2(vX, vY);
        }

        public void SetXVelocity(float vX)
        {
            Velocity = new Vector2(vX, Velocity.Y);
        }

        public void Update(float deltaTime)
        {
            this.Position += Velocity * deltaTime;
            //if (IsGravityAffected)
            //{
            //    AddVelocity(0, Gravity * DeltaTime);
            //}
        }
    }
}

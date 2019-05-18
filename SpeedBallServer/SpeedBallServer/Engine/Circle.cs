using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace SpeedBallServer
{
    public class Circle
    {
        protected Vector2 relativePosition;

        public Vector2 Position { get { return RigidBody.Position; } }
        public RigidBody RigidBody { get; set; }
        public float Radius { get; protected set; }

        public Circle(RigidBody owner, float ray)
        {
            RigidBody = owner;
            Radius = ray;
        }

        public bool Collides(Circle circle, ref Collision collisionInfo)
        {
            Vector2 distance = circle.Position - Position;
            bool collision = distance.Length() <= Radius + circle.Radius;

            if (collision)
            {
                //setting collision's info
                float deltaX = Math.Abs(distance.X) - (Radius + circle.Radius);
                float deltaY = Math.Abs(distance.Y) - (Radius + circle.Radius);
                collisionInfo.Type = Collision.CollisionType.CirclesIntersection;
                collisionInfo.Delta = new Vector2(-deltaX, -deltaY);
            }
            return collision;
        }

    }
}

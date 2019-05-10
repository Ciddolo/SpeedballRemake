using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer
{
    public class Rect
    {
        public Vector2 Position { get { return RigidBody.Position; } }
        public RigidBody RigidBody { get; set; }

        public float HalfWidth { get; protected set; }
        public float HalfHeight { get; protected set; }

        public Rect(RigidBody owner, float width, float height)
        {
            RigidBody = owner;
            HalfWidth = width / 2;
            HalfHeight = height / 2;
        }

        public bool Collides(Rect rect, ref Collision collisionInfo)
        {
            Vector2 distance = rect.Position - Position;

            float deltaX = Math.Abs(distance.X) - (HalfWidth + rect.HalfWidth);
            float deltaY = Math.Abs(distance.Y) - (HalfHeight + rect.HalfHeight);

            if (deltaX <= 0 && deltaY <= 0)
            {
                //setting collision's info
                collisionInfo.Type = Collision.CollisionType.RectsIntersection;
                collisionInfo.Delta = new Vector2(-deltaX, -deltaY);
                return true;
            }
            return false;
        }

    }
}

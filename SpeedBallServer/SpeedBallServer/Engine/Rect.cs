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

        public Vector2 BottomLeft
        {
            get
            {
                return new Vector2(this.Position.X - HalfWidth, this.Position.Y - HalfHeight);
            }
        }

        public Vector2 TopRight
        {
            get
            {
                return new Vector2(this.Position.X + HalfWidth, this.Position.Y + HalfHeight);
            }
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

        public bool Collides(Circle circle, ref Collision collisionInfo)
        {
            bool collision = false;

            float left = this.BottomLeft.X;
            float right = this.TopRight.X;
            float top = this.TopRight.X;
            float bottom = this.BottomLeft.Y;

            //searching for the nearest point to the circle center
            float nearestX = Math.Max(left, Math.Min(circle.Position.X, right));
            float nearestY = Math.Max(top, Math.Min(circle.Position.Y, bottom));

            //check collision
            float deltaX = circle.Position.X - nearestX;
            float deltaY = circle.Position.Y - nearestY;

            collision = ((deltaX * deltaX + deltaY * deltaY) <= circle.Radius * circle.Radius);

            if (collision)
            {
                //setting collision's info
                collisionInfo.Type = Collision.CollisionType.RectCircleIntersection;
                collisionInfo.Delta = new Vector2(-deltaX, -deltaY);
            }

            return collision;
        }

    }
}

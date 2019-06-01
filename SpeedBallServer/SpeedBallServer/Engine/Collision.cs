using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer
{
    public enum ColliderType : uint { Player = 1, Ball = 2, Net = 4, Obstacle = 8}


    public struct Collision
    {
        public enum CollisionType
        {
            None,
            RectsIntersection,
            RectCircleIntersection,
            CirclesIntersection
        }

        public CollisionType Type;
        public Vector2 Delta;
        public GameObject Collider;
        public GameObject WhoIsPushing;
    }
}

using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer
{
    public class Warp : GameObject
    {
        public Warp ConnectedWarp;

        public Warp(GameServer server, float Height, float Width)
            : base((int)InternalObjectsId.Warp, server, Height, Width)
        {
            RigidBody.Type = (uint)ColliderType.Warp;
            RigidBody.AddCollision((uint)ColliderType.Ball);
        }

        public override Packet GetSpawnPacket()
        {
            return new Packet((byte)PacketsCommands.Spawn, true, ObjectType, Id, X, Y, Height, Width);
        }

        public override void OnCollide(Collision collisionInfo)
        {
            if (collisionInfo.Collider is Ball)
            {
                if (ConnectedWarp == null)
                    throw new Exception("Warp does not have a connected warp.");

                Ball ball = (Ball)collisionInfo.Collider;

                if (collisionInfo.Delta.Y < ball.Height)
                {
                    //if the ball does not fully enter on the y axis you risk a collision with a wall on the warp
                    //resulting in the ball looping between being warped and changing direction
                    return;
                }

                Vector2 distanceBetweenWarps = ConnectedWarp.Position - this.Position;
                int sign = -Math.Sign(distanceBetweenWarps.X);
                ball.Position = new Vector2(ConnectedWarp.Position.X + (((ConnectedWarp.Width / 2f) + (ball.Width / 1.9f)) * sign), ball.Position.Y);
            }
        }
    }
}

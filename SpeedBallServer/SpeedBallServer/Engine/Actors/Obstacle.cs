using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer
{
    public class Obstacle : GameObject
    {
        public Obstacle(GameServer server, float Height, float Width )
            : base((int)InternalObjectsId.Obstacle, server, Height, Width)
        {
            RigidBody.Type = (uint)ColliderType.Obstacle;
        }

        public override Packet GetSpawnPacket()
        {
            return new Packet((byte)PacketsCommands.Spawn, true,ObjectType, Id, X, Y, Height,Width);
        }

        public override void OnCollide(Collision collisionInfo)
        {
            throw new Exception("Wall on collide is not intended to be called.");
        }
    }
}

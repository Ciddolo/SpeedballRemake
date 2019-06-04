using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer
{
    public class Bumper : GameObject
    {
        public Bumper(GameServer server, float Height, float Width)
            : base((int)InternalObjectsId.Bumper, server, Height, Width)
        {
            RigidBody.Type = (uint)ColliderType.Obstacle;
        }

        public override Packet GetSpawnPacket()
        {
            return new Packet((byte)PacketsCommands.Spawn, true, ObjectType, Id, X, Y, Height, Width);
        }

        public override void OnCollide(Collision collisionInfo)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer
{
    public class Net : GameObject
    {
        public uint TeamId;

        public Net(GameServer server, float Height, float Width)
            : base((int)InternalObjectsId.Net, server, Height, Width)
        {
            RigidBody.Type = (uint)ColliderType.Net;
        }

        public override Packet GetSpawnPacket()
        {
            return new Packet((byte)PacketsCommands.Spawn, true, ObjectType, Id, X, Y, Height, Width, TeamId);
        }

        public override void OnCollide(Collision collisionInfo)
        {
            throw new Exception("Net on collide is not intended to be called.");
        }
    }
}

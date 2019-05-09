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

        }

        public override void Destroy()
        {
            throw new NotImplementedException();
        }

        public override Packet GetSpawnPacket()
        {
            return new Packet((byte)PacketsCommands.Spawn, true,ObjectType, Id, X, Y, Height,Width);
        }

        public override void Tick()
        {
            throw new NotImplementedException();
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }
    }
}

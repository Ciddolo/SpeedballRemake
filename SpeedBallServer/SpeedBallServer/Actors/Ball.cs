using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer
{
    class Ball : GameObject,IUpdatable
    {
        public Ball(GameServer server, float Height, float Width) 
            : base((int)InternalObjectsId.Ball, server, Height, Width)
        {

        }

        public override void Destroy()
        {
            throw new NotImplementedException();
        }

        public override Packet GetSpawnPacket()
        {
            return new Packet((byte)PacketsCommands.Spawn, true, ObjectType, Id, X, Y, Height, Width);
        }

        public override void OnCollide(Collision collisionInfo)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void Update(float deltaTime)
        {
            //todo
        }
    }
}

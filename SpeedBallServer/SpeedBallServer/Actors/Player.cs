using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer
{
    public class Player : GameObject
    {
        //private Vector2 lastUpdatePosition;
        private Vector2 startingPosition;
        public uint TeamId;

        public Player(GameServer server)
            : base((int)InternalObjectsId.Player, server, 1, 1)
        {

        }

        public void SetStartingPosition(Vector2 startingPos)
        {
            this.Position = startingPos;
            startingPosition = startingPos;
        }

        public void SetStartingPosition(float x, float y)
        {
            this.SetStartingPosition(new Vector2(x,y));
        }

        public override Packet GetSpawnPacket()
        {
            return new Packet((byte)PacketsCommands.Spawn, true, ObjectType, Id,X,Y, Height, Width,TeamId);
        }

        public void Reset()
        {
            this.Position=startingPosition;
        }

        public override void Tick()
        {
            throw new NotImplementedException();
        }

        public override void Destroy()
        {
            throw new NotImplementedException();
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer
{
    public enum PlayerState
    {
        Idle,
        Tackling,
        Stunned
    }

    public class Player : GameObject, IUpdatable
    {
        private Vector2 startingPosition;
        public Vector2 LookingDirection;
        private Vector2 lastUpdatePosition;
        public uint TeamId;
        public PlayerState State;

        public Player(GameServer server)
            : base((int)InternalObjectsId.Player, server, 1, 1)
        {
            this.Reset();

            RigidBody.Type = (uint)ColliderType.Player;
            RigidBody.AddCollision((uint)ColliderType.Obstacle);
        }

        public void SetStartingPosition(Vector2 startingPos)
        {
            lastUpdatePosition = startingPos;
            this.Position = startingPos;
            startingPosition = startingPos;
        }

        public void SetStartingPosition(float x, float y)
        {
            this.SetStartingPosition(new Vector2(x,y));
        }

        public void SetLookingRotation(Vector2 newLookingRotation)
        {
            this.LookingDirection = newLookingRotation;
        }

        public void SetLookingRotation(float x, float y)
        {
            this.SetLookingRotation(new Vector2(x,y));
        }

        public override Packet GetSpawnPacket()
        {
            return new Packet((byte)PacketsCommands.Spawn, true,
                ObjectType, Id, X, Y, Height, Width, TeamId);
        }

        public void Reset()
        {
            this.Position = startingPosition;
            State = PlayerState.Idle;
            LookingDirection = Vector2.Zero;
        }

        public void Tick()
        {
            throw new NotImplementedException();
        }

        public override void Destroy()
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            //to do
            //update player logic
            if (IsActive && RigidBody != null)
            {
                Position = RigidBody.Position;
            }
        }

        protected Packet GetUpdatePacket()
        {
            return new Packet((byte)PacketsCommands.Update, false, 
                Id, X, Y, LookingDirection.X, LookingDirection.Y, (uint)State);
        }
    }
}

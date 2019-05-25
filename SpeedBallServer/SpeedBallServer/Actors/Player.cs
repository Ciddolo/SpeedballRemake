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

        public Player(GameServer server,float Width,float Height)
            : base((int)InternalObjectsId.Player, server, Height, Width)
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
            server.SendToAllClients(GetUpdatePacket());
        }

        protected Packet GetUpdatePacket()
        {
            return new Packet((byte)PacketsCommands.Update, false, 
                Id, X, Y, LookingDirection.X, LookingDirection.Y, (uint)State);
        }

        public override void OnCollide(Collision collisionInfo)
        {
            if (collisionInfo.Collider is Obstacle)
            {
                float deltaX = -collisionInfo.Delta.X;
                float deltaY = -collisionInfo.Delta.Y;

                if (deltaX < 0 && deltaY < 0)
                {
                    if (deltaX > deltaY)
                    {
                        if (this.Position.X < collisionInfo.Collider.Position.X)
                            deltaX = -deltaX;

                        this.Position -= new Vector2(deltaX, 0);
                    }
                    else
                    {
                        if (this.Position.Y < collisionInfo.Collider.Position.Y)
                            deltaY = -deltaY;

                        this.Position -= new Vector2(0, deltaY);
                    }
                }
            }
        }
    }
}

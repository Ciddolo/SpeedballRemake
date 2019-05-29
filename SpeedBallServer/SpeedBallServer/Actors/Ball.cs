using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace SpeedBallServer
{
    public class Ball : GameObject,IUpdatable
    {
        public Player PlayerWhoOwnsTheBall { get; private set; }
        private Vector2 startingPosition;

        public GameLogic gameLogic;

        public Ball(GameServer server, float Height, float Width)
            : base((int)InternalObjectsId.Ball, server, Height, Width)
        {
            RigidBody.Type = (uint)ColliderType.Ball;
            RigidBody.AddCollision((uint)ColliderType.Obstacle);
            RigidBody.IsGravityAffected = true;
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

                if (deltaX > deltaY)
                    this.RigidBody.SetXVelocity(-RigidBody.Velocity.X);
                else
                    this.RigidBody.SetYVelocity(-RigidBody.Velocity.Y);
            }
        }

        public void SetStartingPosition(float x, float y)
        {
            this.SetStartingPosition(new Vector2(x, y));
        }

        public void SetStartingPosition(Vector2 startingPos)
        {
            this.Position = startingPos;
            startingPosition = startingPos;
        }

        public void Reset()
        {
            this.Position = startingPosition;
            this.RigidBody.Velocity = Vector2.Zero;
        }

        public void Update(float deltaTime)
        {
            if (PlayerWhoOwnsTheBall != null)
                this.Position = PlayerWhoOwnsTheBall.Position;

            server.SendToAllClients(GetUpdatePacket());
        }

        protected Packet GetUpdatePacket()
        {
            return new Packet((byte)PacketsCommands.Update, false,
                Id, X, Y);//could send owner and float magnify value
        }

        public void SetBallOwner(Player newOwner)
        {
            this.RigidBody.IsCollisionsAffected = false;
            PlayerWhoOwnsTheBall = newOwner;
            if (gameLogic != null)
                gameLogic.OnBallTaken(newOwner);
        }
    }
}

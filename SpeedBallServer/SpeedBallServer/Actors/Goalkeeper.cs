using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace SpeedBallServer
{
    public class Goalkeeper : GameObject, IUpdatable
    {
        public static float ThrowOffset = 3f;

        private Vector2 startingPosition;
        private float velocity;
        private Vector2 lastUpdatePosition;
        public uint TeamId;
        public PlayerState State;

        public Ball Ball;

        public Goalkeeper(GameServer server, float Width, float Height)
            : base((int)InternalObjectsId.Goalkeeper, server, Height, Width)
        {
            velocity = 6.5f;
            RigidBody.Type = (uint)ColliderType.Goalkeeper;
            RigidBody.AddCollision((uint)ColliderType.Obstacle);
            RigidBody.AddCollision((uint)ColliderType.Player);
            RigidBody.AddCollision((uint)ColliderType.Ball);
            this.Reset();
        }

        private void ThrowBall(Vector2 direction, float force)
        {
            Vector2 ballStartingPos = this.Position + (direction * ThrowOffset);

            Ball.Shot(ballStartingPos, direction * force);
        }

        public void SetStartingPosition(Vector2 startingPos)
        {
            lastUpdatePosition = startingPos;
            this.Position = startingPos;
            startingPosition = startingPos;
        }

        public void SetStartingPosition(float x, float y)
        {
            this.SetStartingPosition(new Vector2(x, y));
        }

        public void SetMovingDirection(Vector2 newMovingDirection)
        {
            this.RigidBody.Velocity = newMovingDirection * velocity;
        }

        public void SetMovingDirection(float x, float y)
        {
            this.SetMovingDirection(new Vector2(x, y));
        }

        private void AI()
        {
            if (Vector2.Distance(startingPosition, Ball.Position) <= 8.0f)
            {
                if (Ball.PlayerWhoOwnsTheBall != null)
                {
                    if (Ball.PlayerWhoOwnsTheBall is Player)
                    {
                        if (((Player)(Ball.PlayerWhoOwnsTheBall)).TeamId != TeamId)
                        {
                            SetMovingDirection(Vector2.Normalize(Ball.Position - Position));

                        }
                        else
                        {
                            SetMovingDirection(Vector2.Zero);
                        }
                    }
                }
                else
                    SetMovingDirection(Vector2.Normalize(Ball.Position - Position));
            }
            else
            {
                if (Vector2.Distance(startingPosition, Position) > .5f)
                    SetMovingDirection(Vector2.Normalize(startingPosition - Position));
                else if (Vector2.Distance(startingPosition, Position) <= 0.1f)
                    Position = startingPosition;
                else
                    SetMovingDirection(Vector2.Zero);
            }
        }

        public override Packet GetSpawnPacket()
        {
            return new Packet((byte)PacketsCommands.Spawn, true,
                ObjectType, Id, X, Y, Height, Width, TeamId);
        }

        public void Reset()
        {
            this.Position = startingPosition;
            this.SetMovingDirection(Vector2.Zero);
        }

        public void Update(float deltaTime)
        {
            AI();
        }

        public Packet GetUpdatePacket()
        {
            return new Packet((byte)PacketsCommands.Update, false,
                Id, X, Y, (uint)State);
        }

        public override void OnCollide(Collision collisionInfo)
        {
            //Console.WriteLine("Coll "+ collisionInfo.Collider.Id+" "+this.Id);

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
            else if (collisionInfo.Collider is Ball)
            {
                if (Position.Y > 0.0f)
                    ThrowBall(new Vector2(0.0f, -1.0f), 12.5f);
                else
                    ThrowBall(new Vector2(0.0f, 1.0f), 12.5f);

                Console.WriteLine(Ball.RigidBody.Velocity);
            }
            else if (collisionInfo.Collider is Player)
            {
                Player other = (Player)collisionInfo.Collider;

                float deltaX = -collisionInfo.Delta.X;
                float deltaY = -collisionInfo.Delta.Y;

                if (deltaX < 0 && deltaY < 0)
                {
                    if (deltaX > deltaY)
                    {
                        if (this.Position.X < collisionInfo.Collider.Position.X)
                            deltaX = -deltaX;

                        other.Position += new Vector2(deltaX, 0);
                    }
                    else
                    {
                        if (this.Position.Y < collisionInfo.Collider.Position.Y)
                            deltaY = -deltaY;

                        other.Position += new Vector2(0, deltaY);
                    }
                }

                if (this.TeamId == other.TeamId || !other.HasBall)
                    return;

                other.GetStunned();

                Ball ball = other.Ball;
                ball.RemoveToPlayer();

                if (Position.Y > 0.0f)
                    ThrowBall(new Vector2(0.0f, -1.0f), 12.5f);
                else
                    ThrowBall(new Vector2(0.0f, 1.0f), 12.5f);
            }
        }
    }
}

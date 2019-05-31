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
        public static float TacklingTime = .3f;
        public static float StunnedTime = .8f;
        public static float ThrowOffset = 3f;

        private Timer stateTimer;
        private Vector2 startingPosition;
        private float velocity;
        private Vector2 lastUpdatePosition;
        public uint TeamId;
        public PlayerState State;

        private Ball ball;
        public Ball Ball { get { return ball; } set { ball = value; } }
        public bool HasBall { get { return ball != null; } }

        public Player(GameServer server, float Width, float Height)
            : base((int)InternalObjectsId.Player, server, Height, Width)
        {

            velocity = 5;
            RigidBody.Type = (uint)ColliderType.Player;
            RigidBody.AddCollision((uint)ColliderType.Obstacle);
            RigidBody.AddCollision((uint)ColliderType.Player);
            RigidBody.AddCollision((uint)ColliderType.Ball);
            stateTimer = new Timer(0f, ResetState);
            this.Reset();
        }

        public void ThrowBall(Vector2 direction,float force)
        {
            Vector2 ballStartingPos = this.Position + (direction * ThrowOffset);

            ball.Shot(ballStartingPos, direction * force);
        }

        public void SetStartingPosition(Vector2 startingPos)
        {
            lastUpdatePosition = startingPos;
            this.Position = startingPos;
            startingPosition = startingPos;
        }

        private void ResetState()
        {
            //Console.WriteLine("reset state of "+this.Id);
            State = PlayerState.Idle;
        }

        public void StartTackling()
        {
            State = PlayerState.Tackling;
            //Console.WriteLine("start tackling");
            stateTimer.Reset(TacklingTime);
            stateTimer.Start();
        }

        public void GetStunned()
        {
            State = PlayerState.Stunned;
            //Console.WriteLine("getting stunned");
            stateTimer.Reset(StunnedTime);
            stateTimer.Start();
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

        public override Packet GetSpawnPacket()
        {
            return new Packet((byte)PacketsCommands.Spawn, true,
                ObjectType, Id, X, Y, Height, Width, TeamId);
        }

        public void Reset()
        {
            this.Position = startingPosition;
            this.stateTimer.Reset();
            ResetState();
            this.SetMovingDirection(Vector2.Zero);
        }

        public void Update(float deltaTime)
        {
            stateTimer.Update(deltaTime);
        }

        public Packet GetUpdatePacket()
        {
            return new Packet((byte)PacketsCommands.Update, false,
                Id, X, Y, (uint)State);
        }

        public override void OnCollide(Collision collisionInfo)
        {
            //Console.WriteLine(State + " " + this.Id);

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
                //ball too high, cant catch it
                if (((Ball)collisionInfo.Collider).Magnification > .01f)
                    return;

                ball = (Ball)collisionInfo.Collider;
                ((Ball)collisionInfo.Collider).AttachToPlayer(this);
            }
            else if (collisionInfo.Collider is Player)
            {
                //Console.WriteLine("collision with player player");

                //getting out of collided player
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

                //if tackling 
                if (State == PlayerState.Tackling)
                {
                    Player other = (Player)collisionInfo.Collider;

                    if (this.TeamId == other.TeamId)
                    {
                        //Console.WriteLine("hitting teammate" + this.TeamId + " " + other.TeamId);
                        return;
                    }

                    other.GetStunned();

                    if (!other.HasBall)
                    {
                        //Console.WriteLine("opponent does not have ball");
                        return;
                    }

                    Ball ball = other.Ball;
                    ball.RemoveToPlayer();
                    ball.AttachToPlayer(this);
                    ResetState();
                }
            }
        }
    }
}

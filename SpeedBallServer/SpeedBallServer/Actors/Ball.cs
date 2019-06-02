using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace SpeedBallServer
{
    public class Ball : GameObject, IUpdatable
    {
        public GameObject PlayerWhoOwnsTheBall { get; private set; }
        public bool HasPlayer { get { return PlayerWhoOwnsTheBall != null; } }
        private Vector2 startingPosition;

        public GameLogic gameLogic;

        private float maxVelocity, velocityMagnificationTreshold;

        public float Magnification { get; private set; }

        public Ball(GameServer server, float Height, float Width)
            : base((int)InternalObjectsId.Ball, server, Height, Width)
        {
            RigidBody.Type = (uint)ColliderType.Ball;
            RigidBody.AddCollision((uint)ColliderType.Obstacle);
            RigidBody.AddCollision((uint)ColliderType.Net);
            RigidBody.IsGravityAffected = true;

            maxVelocity = 20f;
            velocityMagnificationTreshold = 10f;
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
            else if (collisionInfo.Collider is Net)
            {
                if (gameLogic != null)
                    gameLogic.OnGoal((Net)collisionInfo.Collider);
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
            this.RigidBody.IsCollisionsAffected = true;
            if (this.HasPlayer)
                this.RemoveToPlayer();
        }

        public void Update(float deltaTime)
        {
            //Console.WriteLine(Id + " im at" + this.Position);

            //max velocity of the ball should be 20, over velocityMagnificationTreshold the ball should be in "air" 
            Magnification = RigidBody.Velocity.Length() - velocityMagnificationTreshold;
            //Console.WriteLine(Magnification);
            //Console.WriteLine(RigidBody.Velocity.Length());
            if (Magnification <= 0f)
                Magnification = 0f;
            else
                Magnification /= (maxVelocity - velocityMagnificationTreshold);

            if (PlayerWhoOwnsTheBall != null)
                this.Position = PlayerWhoOwnsTheBall.Position;
        }

        public Packet GetUpdatePacket()
        {
            return new Packet(PacketsCommands.Update, false,
                Id, X, Y, Magnification);//could send owner
        }

        public void AttachToPlayer(GameObject newOwner)
        {
            RigidBody.IsCollisionsAffected = false;
            Magnification = 0.0f;
            this.RigidBody.Velocity = Vector2.Zero;
            PlayerWhoOwnsTheBall = newOwner;
            if (newOwner is Player)
                ((Player)PlayerWhoOwnsTheBall).Ball = this;
            else
                ((Goalkeeper)PlayerWhoOwnsTheBall).Ball = this;
            if (gameLogic != null && newOwner != null)
                gameLogic.OnBallTaken(newOwner);
        }

        public void RemoveToPlayer()
        {
            if (PlayerWhoOwnsTheBall == null)
                return;

            if (PlayerWhoOwnsTheBall is Player)
                ((Player)PlayerWhoOwnsTheBall).Ball = null;
            else
                ((Goalkeeper)PlayerWhoOwnsTheBall).Ball = null;

            PlayerWhoOwnsTheBall = null;
            RigidBody.IsCollisionsAffected = true;
        }

        public void Shot(Vector2 startPos, Vector2 velocity)
        {
            Vector2 trueStartPos = startPos;

            if(gameLogic!=null)
            {
                Vector2 direction = Vector2.Normalize(velocity);

                PhysicsHandler.RaycastHitInfo hitInfo = gameLogic.PhysicsHandler.RayCast(this.Position,
                    direction, this.RigidBody, (uint)ColliderType.Obstacle);

                if (hitInfo.Collider != null)
                {
                    if (hitInfo.Distance < Player.ThrowOffset)
                    {
                        trueStartPos = hitInfo.Point - (direction * this.RigidBody.BoundingBox.HalfWidth);
                        Console.WriteLine("Adjusting shot");
                    }
                }
            }

            this.Position = trueStartPos;
            RemoveToPlayer();
            RigidBody.Velocity = velocity;
            //force update to update magnification
            this.Update(0f);
        }
    }
}
using System;
using System.Numerics;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer.Test.EngineTests
{
    public class PlayerTests
    {
        private Player myPlayer;

        [SetUp]
        public void SetUp()
        {
            myPlayer = new Player(null, 1, 1);
        }

        [Test]
        public void DefaultValues()
        {
            Assert.That(myPlayer.RigidBody.Type, Is.EqualTo((uint)ColliderType.Player));
            Assert.That(myPlayer.RigidBody.CollisionMask, Is.EqualTo(47));
            Assert.That(myPlayer.ObjectType, Is.EqualTo((uint)InternalObjectsId.Player));
            Assert.That(myPlayer.State, Is.EqualTo(PlayerState.Idle));
        }

        [Test]
        public void Reset()
        {
            Ball myBall = new Ball(null, 1, 1);

            myBall.AttachToPlayer(myPlayer);
            myPlayer.GetStunned();

            myPlayer.Reset();

            Assert.That(myPlayer.Ball, Is.EqualTo(null));
            Assert.That(myPlayer.State, Is.EqualTo(PlayerState.Idle));
            Assert.That(myBall.PlayerWhoOwnsTheBall, Is.EqualTo(null));
        }


        [Test]
        public void StateStartTackling()
        {
            myPlayer.StartTackling();
            Assert.That(myPlayer.State, Is.EqualTo(PlayerState.Tackling));
        }

        [Test]
        public void ResetStateOnTimerElapsed()
        {
            myPlayer.StartTackling();
            myPlayer.Update(Player.TacklingTime);
            Assert.That(myPlayer.State, Is.EqualTo(PlayerState.Idle));
        }

        [Test]
        public void StateGetStunned()
        {
            myPlayer.GetStunned();
            Assert.That(myPlayer.State, Is.EqualTo(PlayerState.Stunned));
        }

        [Test]
        public void StateAfterStopGetStunned()
        {
            myPlayer.GetStunned();
            myPlayer.Update(Player.StunnedTime);
            Assert.That(myPlayer.State, Is.EqualTo(PlayerState.Idle));
        }

        [Test]
        public void StateNotResettingOnTimerNotElapsed()
        {
            myPlayer.GetStunned();
            myPlayer.Update(Player.TacklingTime);
            Assert.That(myPlayer.State, Is.EqualTo(PlayerState.Stunned));
        }

        [Test]
        public void CheckUpdatePacket()
        {
            byte[] data = myPlayer.GetUpdatePacket().GetData();

            uint netId = BitConverter.ToUInt32(data, 5);
            float x = BitConverter.ToSingle(data, 9);
            float y = BitConverter.ToSingle(data, 13);
            uint state = BitConverter.ToUInt32(data, 17);

            Assert.That(netId, Is.EqualTo(myPlayer.Id));
            Assert.That(x, Is.EqualTo(myPlayer.X));
            Assert.That(y, Is.EqualTo(myPlayer.Y));
            Assert.That(state, Is.EqualTo((uint)myPlayer.State));
        }

        [Test]
        public void CheckSpawnPacket()
        {
            byte[] data = myPlayer.GetSpawnPacket().GetData();

            uint objectType = BitConverter.ToUInt32(data, 5);
            uint netId = BitConverter.ToUInt32(data, 9);
            float x = BitConverter.ToSingle(data, 13);
            float y = BitConverter.ToSingle(data, 17);
            float height = BitConverter.ToSingle(data, 21);
            float width = BitConverter.ToSingle(data, 25);
            float teamId = BitConverter.ToUInt32(data, 29);

            Assert.That(objectType, Is.EqualTo(myPlayer.ObjectType));
            Assert.That(x, Is.EqualTo(myPlayer.X));
            Assert.That(y, Is.EqualTo(myPlayer.Y));
            Assert.That(netId, Is.EqualTo(myPlayer.Id));
            Assert.That(teamId, Is.EqualTo(myPlayer.TeamId));
            Assert.That(width, Is.EqualTo(myPlayer.Width));
            Assert.That(height, Is.EqualTo(myPlayer.Height));
        }

        [Test]
        public void ThrowBall()
        {
            Ball myBall = new Ball(null, 1, 1);
            myBall.AttachToPlayer(myPlayer);

            myPlayer.ThrowBall(new Vector2(1f,0f),20);

            Assert.That(myBall.RigidBody.Velocity, Is.EqualTo(new Vector2(20f, 0f)));
            Assert.That(myBall.PlayerWhoOwnsTheBall,Is.EqualTo(null));
        }

        [Test]
        public void OnCollideWithBall()
        {
            Ball myBall = new Ball(null, 1, 1);
            PhysicsHandler physicsHandler = new PhysicsHandler();

            physicsHandler.AddItem(myPlayer.RigidBody);
            physicsHandler.AddItem(myBall.RigidBody);

            myPlayer.Position = new Vector2(50, 50);
            myBall.Position = new Vector2(50, 50);
            physicsHandler.CheckCollisions();

            Assert.That(myBall.PlayerWhoOwnsTheBall, Is.EqualTo(myPlayer));
            Assert.That(myBall, Is.EqualTo(myPlayer.Ball));
        }

        [Test]
        public void OnCollideWhileTacklingOpponentWithBall()
        {
            Player myOpponent;
            Ball myBall;
            PhysicsHandler physicsHandler;

            physicsHandler = new PhysicsHandler();

            myOpponent = new Player(null, 1, 1);
            myOpponent.TeamId = 1;

            myBall = new Ball(null, 1, 1);
            myBall.Position = new Vector2(200f, 200f);

            physicsHandler.AddItem(myPlayer.RigidBody);
            physicsHandler.AddItem(myOpponent.RigidBody);
            physicsHandler.AddItem(myBall.RigidBody);

            myOpponent.Position = new Vector2(50, 50);
            myBall.Position = new Vector2(50, 50);

            physicsHandler.CheckCollisions();

            myPlayer.Position = myOpponent.Position;
            myPlayer.StartTackling();
            physicsHandler.CheckCollisions();

            Assert.That(myOpponent.State, Is.EqualTo(PlayerState.Stunned));
            Assert.That(myPlayer.State, Is.EqualTo(PlayerState.Idle));
            Assert.That(myPlayer.HasBall, Is.EqualTo(true));
            Assert.That(myOpponent.HasBall, Is.EqualTo(false));
        }

        [Test]
        public void OnCollideWhileTacklingOpponent()
        {
            Player myOpponent;
            PhysicsHandler physicsHandler;

            physicsHandler = new PhysicsHandler();

            myOpponent = new Player(null, 1, 1);
            myOpponent.TeamId = 1;

            physicsHandler.AddItem(myPlayer.RigidBody);
            physicsHandler.AddItem(myOpponent.RigidBody);

            myOpponent.Position = new Vector2(50, 50);
            physicsHandler.CheckCollisions();

            myPlayer.Position = myOpponent.Position;
            myPlayer.StartTackling();
            physicsHandler.CheckCollisions();

            Assert.That(myOpponent.State, Is.EqualTo(PlayerState.Stunned));
            Assert.That(myPlayer.State, Is.EqualTo(PlayerState.Tackling));
        }

        [Test]
        public void OnCollideWhileTacklingTeammate()
        {
            Player myTeammate;
            Ball myBall;
            PhysicsHandler physicsHandler;

            physicsHandler = new PhysicsHandler();

            myTeammate = new Player(null, 1, 1);
            myTeammate.TeamId = 0;

            myBall = new Ball(null, 1, 1);
            myBall.Position = new Vector2(200f, 200f);

            physicsHandler.AddItem(myTeammate.RigidBody);
            physicsHandler.AddItem(myPlayer.RigidBody);
            physicsHandler.AddItem(myBall.RigidBody);

            myTeammate.Position = new Vector2(50, 50);
            myBall.Position = new Vector2(50, 50);
            physicsHandler.CheckCollisions();

            myPlayer.Position = myTeammate.Position;
            myPlayer.StartTackling();
            physicsHandler.CheckCollisions();

            Assert.That(myPlayer.State, Is.EqualTo(PlayerState.Tackling));
            Assert.That(myTeammate.State, Is.EqualTo(PlayerState.Idle));
            Assert.That(myPlayer.HasBall, Is.EqualTo(false));
            Assert.That(myTeammate.HasBall, Is.EqualTo(true));
        }

        [Test]
        public void OnCollideWithOpponentWhilePushing()
        {
            Player myOpponent;
            PhysicsHandler physicsHandler;

            physicsHandler = new PhysicsHandler();

            myOpponent = new Player(null, 1, 1);
            myOpponent.TeamId = 1;

            physicsHandler.AddItem(myPlayer.RigidBody);
            physicsHandler.AddItem(myOpponent.RigidBody);

            myPlayer.Position = new Vector2(10, 10);
            myPlayer.RigidBody.Velocity = new Vector2(10, 10);

            myOpponent.Position = new Vector2(10, 10);

            physicsHandler.CheckCollisions();

            Assert.That(myOpponent.Position, Is.EqualTo(new Vector2(10, 10)));
            Assert.That(myOpponent.RigidBody.Velocity, Is.EqualTo(Vector2.Zero));
            Assert.That(myPlayer.Position, Is.Not.EqualTo(new Vector2(10, 10)));
        }

        [Test]
        public void OnCollideWithTeammateWhilePushing()
        {
            Player myTeammate;
            PhysicsHandler physicsHandler;

            physicsHandler = new PhysicsHandler();

            myTeammate = new Player(null, 1, 1);
            myTeammate.TeamId = 0;

            physicsHandler.AddItem(myTeammate.RigidBody);
            physicsHandler.AddItem(myPlayer.RigidBody);

            myPlayer.Position = new Vector2(10, 10);
            myPlayer.RigidBody.Velocity = new Vector2(10, 10);

            myTeammate.Position = new Vector2(10, 10);

            physicsHandler.CheckCollisions();

            Assert.That(myTeammate.Position, Is.EqualTo(new Vector2(10, 10)));
            Assert.That(myTeammate.RigidBody.Velocity, Is.Not.EqualTo(Vector2.Zero));
            Assert.That(myPlayer.Position, Is.Not.EqualTo(new Vector2(10, 10)));
        }
    }
}

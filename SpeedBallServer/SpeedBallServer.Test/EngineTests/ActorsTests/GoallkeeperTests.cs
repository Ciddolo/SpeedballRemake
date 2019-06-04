using System;
using System.Numerics;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer.Test.EngineTests
{
    class GoallkeeperTests
    {
        private Goalkeeper myGoalkeeper;

        [SetUp]
        public void SetUp()
        {
            myGoalkeeper = new Goalkeeper(null, 1, 1);
        }

        [Test]
        public void DefaultValues()
        {
            Assert.That(myGoalkeeper.ObjectType, Is.EqualTo((uint)InternalObjectsId.Goalkeeper));
            Assert.That(myGoalkeeper.RigidBody.Type, Is.EqualTo((uint)ColliderType.Goalkeeper));
            Assert.That(myGoalkeeper.RigidBody.CollisionMask, Is.EqualTo(11));
        }

        [Test]
        public void CheckSpawnPacket()
        {
            byte[] data = myGoalkeeper.GetSpawnPacket().GetData();

            uint objectType = BitConverter.ToUInt32(data, 5);
            uint netId = BitConverter.ToUInt32(data, 9);
            float x = BitConverter.ToSingle(data, 13);
            float y = BitConverter.ToSingle(data, 17);
            float height = BitConverter.ToSingle(data, 21);
            float width = BitConverter.ToSingle(data, 25);
            float teamId = BitConverter.ToUInt32(data, 29);

            Assert.That(objectType, Is.EqualTo(myGoalkeeper.ObjectType));
            Assert.That(x, Is.EqualTo(myGoalkeeper.X));
            Assert.That(y, Is.EqualTo(myGoalkeeper.Y));
            Assert.That(netId, Is.EqualTo(myGoalkeeper.Id));
            Assert.That(teamId, Is.EqualTo(myGoalkeeper.TeamId));
            Assert.That(width, Is.EqualTo(myGoalkeeper.Width));
            Assert.That(height, Is.EqualTo(myGoalkeeper.Height));
        }

        [Test]
        public void CheckUpdatePacket()
        {
            byte[] data = myGoalkeeper.GetUpdatePacket().GetData();

            uint netId = BitConverter.ToUInt32(data, 5);
            float x = BitConverter.ToSingle(data, 9);
            float y = BitConverter.ToSingle(data, 13);
            uint state = BitConverter.ToUInt32(data, 17);

            Assert.That(netId, Is.EqualTo(myGoalkeeper.Id));
            Assert.That(x, Is.EqualTo(myGoalkeeper.X));
            Assert.That(y, Is.EqualTo(myGoalkeeper.Y));
            Assert.That(state, Is.EqualTo((uint)myGoalkeeper.State));
        }

        [Test]
        public void OnCollideWithPlayer()
        {
            Player myPlayer;
            PhysicsHandler physicsHandler;

            physicsHandler = new PhysicsHandler();

            myPlayer = new Player(null, 1, 1);

            physicsHandler.AddItem(myPlayer.RigidBody);
            physicsHandler.AddItem(myGoalkeeper.RigidBody);

            myPlayer.Position = new Vector2(10, 10);

            myGoalkeeper.Position = new Vector2(10, 10);

            physicsHandler.CheckCollisions();

            Assert.That(myGoalkeeper.Position, Is.EqualTo(new Vector2(10, 10)));
            Assert.That(myPlayer.Position, Is.Not.EqualTo(new Vector2(10, 10)));
        }

        [Test]
        public void OnCollideWithOpponentWithBall()
        {
            Player myPlayer = new Player(null, 1, 1);
            Ball myBall = new Ball(null, 1, 1);
            Goalkeeper myGoalkeeper = new Goalkeeper(null, 1, 1);
            myGoalkeeper.Ball = myBall;

            PhysicsHandler physicsHandler = new PhysicsHandler();

            physicsHandler.AddItem(myGoalkeeper.RigidBody);
            physicsHandler.AddItem(myPlayer.RigidBody);
            physicsHandler.AddItem(myBall.RigidBody);

            myPlayer.Position = new Vector2(100, 100);
            myGoalkeeper.TeamId = 1;
            myBall.Position = new Vector2(100, 100);

            physicsHandler.CheckCollisions();

            myGoalkeeper.Position = new Vector2(100, 100);

            physicsHandler.CheckCollisions();

            Assert.That(myBall.RigidBody.Velocity, Is.EqualTo(new Vector2(0f, -12.5f)));
            Assert.That(myBall.PlayerWhoOwnsTheBall, Is.EqualTo(null));
            Assert.That(myGoalkeeper.Position, Is.EqualTo(new Vector2(100, 100)));
        }

        [Test]
        public void OnCollideWithTeammateWithBall()
        {
            Player myPlayer = new Player(null, 1, 1);
            Ball myBall = new Ball(null, 1, 1);
            Goalkeeper myGoalkeeper = new Goalkeeper(null, 1, 1);
            myGoalkeeper.Ball = myBall;

            PhysicsHandler physicsHandler = new PhysicsHandler();

            physicsHandler.AddItem(myGoalkeeper.RigidBody);
            physicsHandler.AddItem(myPlayer.RigidBody);
            physicsHandler.AddItem(myBall.RigidBody);

            myPlayer.Position = new Vector2(100, 100);
            myGoalkeeper.TeamId = 0;
            myBall.Position = new Vector2(100, 100);

            physicsHandler.CheckCollisions();

            myGoalkeeper.Position = new Vector2(100, 100);

            physicsHandler.CheckCollisions();

            Assert.That(myBall.PlayerWhoOwnsTheBall, Is.EqualTo(myPlayer));
            Assert.That(myBall.RigidBody.Velocity, Is.EqualTo(new Vector2(0f, 0f)));
            Assert.That(myGoalkeeper.Position, Is.EqualTo(new Vector2(100, 100)));
        }

        [Test]
        public void OnCollideWithBall()
        {
            Ball myBall = new Ball(null, 1, 1);
            Goalkeeper myGoalkeeper = new Goalkeeper(null, 1, 1);
            myGoalkeeper.Ball = myBall;

            PhysicsHandler physicsHandler = new PhysicsHandler();

            physicsHandler.AddItem(myGoalkeeper.RigidBody);
            physicsHandler.AddItem(myBall.RigidBody);

            myBall.Position = new Vector2(100, 100);

            physicsHandler.CheckCollisions();

            myGoalkeeper.Position = new Vector2(100, 100);

            physicsHandler.CheckCollisions();

            Assert.That(myBall.RigidBody.Velocity, Is.EqualTo(new Vector2(0f, -12.5f)));
            Assert.That(myGoalkeeper.Position, Is.EqualTo(new Vector2(100, 100)));
        }
    }
}

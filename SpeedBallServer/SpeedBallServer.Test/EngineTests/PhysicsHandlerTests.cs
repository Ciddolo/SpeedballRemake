using System;
using System.Numerics;
using NUnit.Framework;

namespace SpeedBallServer.Test.EngineTests
{
    public class PhysicsHandlerTest
    {
        private Player myPlayer;
        private Net myNet;
        private Ball myBall;
        private Bumper myBumper;
        private Obstacle obstacle;
        private PhysicsHandler physicsHandler;

        [SetUp]
        public void SetUpTest()
        {
            myPlayer = new Player(null, 1, 1);
            myPlayer.TeamId = 0;

            obstacle = new Obstacle(null, 10, 10);
            myNet = new Net(null, 1, 1);
            myBall = new Ball(null, 1, 1);
            myBumper = new Bumper(null, 1, 1);
            myNet.Position = new Vector2(200f, 200f);
            myBall.Position = new Vector2(200f, 200f);

            physicsHandler = new PhysicsHandler();
            physicsHandler.AddItem(myPlayer.RigidBody);
            physicsHandler.AddItem(obstacle.RigidBody);

            physicsHandler.CheckCollisions();
        }

        [Test]
        public void PlayerCollidingObstacle()
        {
            Assert.That(myPlayer.Position, Is.Not.EqualTo(Vector2.Zero));
        }

        [Test]
        public void PlayerNotCollidingWithNet()
        {
            myPlayer.Position = new Vector2(200f, 200f);
            physicsHandler.CheckCollisions();
            Assert.That(myPlayer.Position, Is.EqualTo(new Vector2(200f, 200f)));
        }

        [Test]
        public void BallTakenNotCollidingWithBumper()
        {
            myPlayer.Position = new Vector2(100f, 100f);
            myBall.Position = new Vector2(100f, 100f);
            physicsHandler.CheckCollisions();
            myBumper.Position = new Vector2(100f, 100f);
            physicsHandler.CheckCollisions();

            Assert.That(myBall.Position, Is.EqualTo(new Vector2(100f, 100f)));
        }
    }
}

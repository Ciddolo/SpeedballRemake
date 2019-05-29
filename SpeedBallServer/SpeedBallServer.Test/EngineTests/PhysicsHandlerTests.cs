using System;
using System.Numerics;
using NUnit.Framework;

namespace SpeedBallServer.Test.EngineTests
{
    public class PhysicsHandlerTest
    {
        private Player myPlayer;
        private Ball myBall;
        private Obstacle obstacle;
        private PhysicsHandler physicsHandler;

        [SetUp]
        public void SetUpTest()
        {
            myPlayer = new Player(null, 1, 1);
            obstacle = new Obstacle(null, 10, 10);
            myBall = new Ball(null, 1, 1);

            physicsHandler = new PhysicsHandler();
            physicsHandler.AddItem(myPlayer.RigidBody);
            physicsHandler.AddItem(obstacle.RigidBody);
            physicsHandler.AddItem(myBall.RigidBody);

            physicsHandler.CheckCollisions();
        }

        [Test]
        public void PlayerCollidingObstacle()
        {
            Assert.That(myPlayer.Position, Is.Not.EqualTo(Vector2.Zero));
        }

        [Test]
        public void SuccessfulCollisionBallKnowsPlayer()
        {
            myPlayer.Position = new Vector2(50, 50);
            myBall.Position = new Vector2(50, 50);
            physicsHandler.CheckCollisions();
            Assert.That(myBall.PlayerWhoOwnsTheBall, Is.EqualTo(myPlayer));
        }

        [Test]
        public void SuccessfulCollisionPlayerKnowsBall()
        {
            myPlayer.Position = new Vector2(50, 50);
            myBall.Position = new Vector2(50, 50);
            physicsHandler.CheckCollisions();
            Assert.That(myBall, Is.EqualTo(myPlayer.Ball));
        }
    }
}

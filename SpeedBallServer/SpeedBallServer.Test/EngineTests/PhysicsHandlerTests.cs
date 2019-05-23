using System;
using System.Numerics;
using NUnit.Framework;

namespace SpeedBallServer.Test.EngineTests
{
    public class PhysicsHandlerTest
    {
        private Player myPlayer;
        private Obstacle obstacle;
        private PhysicsHandler physicsHandler;

        [SetUp]
        public void SetUpTest()
        {
            myPlayer = new Player(null,1,1);
            obstacle = new Obstacle(null, 10, 10);

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
    }
}

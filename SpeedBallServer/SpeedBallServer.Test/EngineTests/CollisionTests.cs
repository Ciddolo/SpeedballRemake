using System;
using System.Numerics;
using NUnit.Framework;

namespace SpeedBallServer.Test.EngineTests
{
    public class CollisionTests
    {
        private Player myPlayer;
        private Obstacle obstacle;

        [SetUp]
        public void SetUpTest()
        {
            myPlayer = new Player(null);
            obstacle = new Obstacle(null,1,10);
        }

        [Test]
        public void SuccessfulCollision()
        {
            Collision collisionInfo = new Collision();
            Assert.That(myPlayer.RigidBody.Collides(obstacle.RigidBody, ref collisionInfo), Is.EqualTo(true));
        }

        [Test]
        public void UnsuccessfulCollision()
        {
            myPlayer.Position = new Vector2(0, 1.01f);
            Collision collisionInfo = new Collision();
            Assert.That(myPlayer.RigidBody.Collides(obstacle.RigidBody, ref collisionInfo), Is.EqualTo(false));
        }

    }
}

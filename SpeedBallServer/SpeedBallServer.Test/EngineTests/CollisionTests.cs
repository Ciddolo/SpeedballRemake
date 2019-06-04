using System;
using System.Numerics;
using NUnit.Framework;

namespace SpeedBallServer.Test.EngineTests
{
    public class CollisionTests
    {
        private Player myPlayer;
        private Player myTeammate;
        private Ball myBall;
        private Obstacle obstacle;

        [SetUp]
        public void SetUpTest()
        {
            myPlayer = new Player(null, 1, 1);
            myTeammate = new Player(null, 1, 1);
            obstacle = new Obstacle(null, 1, 10);
            myBall = new Ball(null, 1, 1);
        }

        [Test]
        public void SuccessfulCollisionPlayerObstacle()
        {
            Collision collisionInfo = new Collision();
            Assert.That(myPlayer.RigidBody.Collides(obstacle.RigidBody, ref collisionInfo), Is.EqualTo(true));
        }

        [Test]
        public void UnsuccessfulCollisionPlayerObstacle()
        {
            myPlayer.Position = new Vector2(0, 1.01f);
            Collision collisionInfo = new Collision();
            Assert.That(myPlayer.RigidBody.Collides(obstacle.RigidBody, ref collisionInfo), Is.EqualTo(false));
        }

        [Test]
        public void CollisionBetweenPlayerWhoIsPushing()
        {
            myPlayer.Position = new Vector2(10, 10);
            myPlayer.RigidBody.Velocity = new Vector2(10, 10);
            myTeammate.Position = new Vector2(10, 10);
            Collision collisionInfo = new Collision();
            myPlayer.RigidBody.Collides(myTeammate.RigidBody, ref collisionInfo);
            Assert.That(collisionInfo.WhoIsPushing, Is.EqualTo(myPlayer));
        }

    }
}

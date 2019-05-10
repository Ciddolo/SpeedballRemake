using System.Numerics;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer.Test
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
        public void TestSuccessfulCollision()
        {
            Collision collisionInfo = new Collision();
            Assert.That(myPlayer.RigidBody.Collides(obstacle.RigidBody, ref collisionInfo), Is.EqualTo(true));
        }

        [Test]
        public void TestUnsuccessfulCollision()
        {
            myPlayer.Position = new Vector2(0, 1.01f);
            Collision collisionInfo = new Collision();
            Assert.That(myPlayer.RigidBody.Collides(obstacle.RigidBody, ref collisionInfo), Is.EqualTo(false));
        }

    }
}

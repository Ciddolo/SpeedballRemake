using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Numerics;

namespace SpeedBallServer.Test.EngineTests
{
    public class RigidBodyTests
    {
        private RigidBody myRigidbody;

        [SetUp]
        public void SetUp()
        {
            myRigidbody = new Ball(null,1, 1).RigidBody;
        }

        [Test]
        public void DefaultVelocity()
        {
            Assert.That(myRigidbody.Velocity, Is.EqualTo(Vector2.Zero));
        }

        [Test]
        public void AddVelocity()
        {
            myRigidbody.AddVelocity(25f, -25f);
            myRigidbody.AddVelocity(25f, 0f);
            Assert.That(myRigidbody.Velocity, Is.EqualTo(new Vector2(50f, -25f)));
        }

        [Test]
        public void UpdateNoGravity()
        {
            myRigidbody.Velocity = new Vector2(20f, 0f);
            myRigidbody.IsGravityAffected = false;
            myRigidbody.Update(1f);

            Assert.That(myRigidbody.Position, Is.EqualTo(new Vector2(20f, 0)));
            Assert.That(myRigidbody.Velocity, Is.EqualTo(new Vector2(20f, 0)));
        }

        [Test]
        public void UpdateWithGravity()
        {
            myRigidbody.Velocity = new Vector2(20f, 0f);
            myRigidbody.Update(1f);

            Assert.That(myRigidbody.Position, Is.EqualTo(new Vector2(20f, 0)));
            Assert.That(myRigidbody.Velocity, Is.EqualTo(new Vector2(20f-4f, 0)));
        }

        [Test]
        public void SetXVelocity()
        {
            myRigidbody.SetXVelocity(5f);

            Assert.That(myRigidbody.Velocity, Is.EqualTo(new Vector2(5f, 0f)));
        }

        [Test]
        public void SetYVelocity()
        {
            myRigidbody.SetYVelocity(5f);

            Assert.That(myRigidbody.Velocity, Is.EqualTo(new Vector2(0f, 5f)));
        }

        [Test]
        public void SetRigidbodyType()
        {
            myRigidbody.Type = (uint)ColliderType.Obstacle;

            Assert.That(myRigidbody.Type, Is.EqualTo((uint)ColliderType.Obstacle));
        }

        [Test]
        public void SetRigidbodyCollisionMask()
        {
            myRigidbody.ResetCollisionMask();

            myRigidbody.AddCollision(ColliderType.Net);
            myRigidbody.AddCollision(ColliderType.Player);

            Assert.That(myRigidbody.CollisionMask, Is.EqualTo(5));
        }

        [Test]
        public void SucessfullCheckCollisionMask()
        {
            Player player = new Player(null, 1, 1);

            Assert.That(player.RigidBody.CheckCollisionWith(myRigidbody), Is.EqualTo(true));
        }

        [Test]
        public void UnsucessfullCheckCollisionMask()
        {
            Player player = new Player(null,1,1);

            Assert.That(myRigidbody.CheckCollisionWith(player.RigidBody), Is.EqualTo(false));
        }

    }
}

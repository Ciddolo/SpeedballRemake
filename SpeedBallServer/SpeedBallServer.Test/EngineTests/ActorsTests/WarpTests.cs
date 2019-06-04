using System;
using System.Numerics;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer.Test.EngineTests
{
    public class WarpTests
    {
        private Warp myWarp;

        [SetUp]
        public void SetUp()
        {
            myWarp = new Warp(null, 1, 1);
        }

        [Test]
        public void DefaultValues()
        {
            Assert.That(myWarp.ObjectType, Is.EqualTo((uint)InternalObjectsId.Warp));
            Assert.That(myWarp.RigidBody.Type, Is.EqualTo((uint)ColliderType.Warp));
            Assert.That(myWarp.RigidBody.CollisionMask, Is.EqualTo((uint)ColliderType.Ball));
        }

        [Test]
        public void CheckSpawnPacket()
        {
            byte[] data = myWarp.GetSpawnPacket().GetData();

            uint objectType = BitConverter.ToUInt32(data, 5);
            uint netId = BitConverter.ToUInt32(data, 9);
            float x = BitConverter.ToSingle(data, 13);
            float y = BitConverter.ToSingle(data, 17);
            float height = BitConverter.ToSingle(data, 21);
            float width = BitConverter.ToSingle(data, 25);

            Assert.That(objectType, Is.EqualTo(myWarp.ObjectType));
            Assert.That(x, Is.EqualTo(myWarp.X));
            Assert.That(y, Is.EqualTo(myWarp.Y));
            Assert.That(netId, Is.EqualTo(myWarp.Id));
            Assert.That(width, Is.EqualTo(myWarp.Width));
            Assert.That(height, Is.EqualTo(myWarp.Height));
        }

        [Test]
        public void OnCollideWithBallFullEnter()
        {
            Ball myBall = new Ball(null, 1, 1);
            Warp aWarp = new Warp(null, 1, 1);

            myWarp.ConnectedWarp = aWarp;

            PhysicsHandler physicsHandler = new PhysicsHandler();

            physicsHandler.AddItem(aWarp.RigidBody);
            physicsHandler.AddItem(myWarp.RigidBody);
            physicsHandler.AddItem(myBall.RigidBody);

            myWarp.Position = new Vector2(100, 100);
            myBall.Position = new Vector2(100, 100);
            myBall.RigidBody.Velocity = new Vector2(1f, 0f);

            physicsHandler.CheckCollisions();

            Vector2 distanceBetweenWarps = aWarp.Position - myWarp.Position;
            int sign = -Math.Sign(distanceBetweenWarps.X);
            Vector2 predictedWarpPosition = new Vector2(aWarp.Position.X + (((aWarp.Width / 2f) + (myBall.Width / 1.9f)) * sign), myBall.Position.Y);

            Assert.That(myBall.Position, Is.EqualTo(predictedWarpPosition));
        }

        [Test]
        public void OnCollideWithBallPartialEnter()
        {
            Ball myBall = new Ball(null, 1, 1);
            Warp aWarp = new Warp(null, 1, 1);
            myWarp.ConnectedWarp = aWarp;

            PhysicsHandler physicsHandler = new PhysicsHandler();

            physicsHandler.AddItem(aWarp.RigidBody);
            physicsHandler.AddItem(myWarp.RigidBody);
            physicsHandler.AddItem(myBall.RigidBody);

            myWarp.Position = new Vector2(100, 100);
            myBall.Position = new Vector2(100, 100.8f);
            myBall.RigidBody.Velocity = new Vector2(1f, 0f);

            physicsHandler.CheckCollisions();

            Assert.That(myBall.Position, Is.EqualTo(new Vector2(100, 100.8f)));
        }

        [Test]
        public void OnCollideWithBallNoConnectedWarp()
        {
            Ball myBall = new Ball(null, 1, 1);

            PhysicsHandler physicsHandler = new PhysicsHandler();

            physicsHandler.AddItem(myWarp.RigidBody);
            physicsHandler.AddItem(myBall.RigidBody);

            myWarp.Position = new Vector2(100, 100);
            myBall.Position = new Vector2(100, 100);
            myBall.RigidBody.Velocity = new Vector2(1f, 0f);

            Assert.That(()=> physicsHandler.CheckCollisions(),Throws.InstanceOf<Exception>());
        }

    }
}

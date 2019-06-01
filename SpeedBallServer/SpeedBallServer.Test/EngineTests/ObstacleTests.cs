using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Numerics;
using System.Threading.Tasks;

namespace SpeedBallServer.Test.EngineTests
{
    public class ObstacleTests
    {
        private Obstacle myObstacle;

        [SetUp]
        public void SetUp()
        {
            myObstacle = new Obstacle(null, 1, 1);
        }

        [Test]
        public void InternalObjId()
        {
            Assert.That(myObstacle.ObjectType, Is.EqualTo((uint)InternalObjectsId.Obstacle));
        }

        [Test]
        public void RigidBodyType()
        {
            Assert.That(myObstacle.RigidBody.Type, Is.EqualTo((uint)ColliderType.Obstacle));
        }

        [Test]
        public void RigidBodyCollisionMask()
        {
            Assert.That(myObstacle.RigidBody.CollisionMask, Is.EqualTo(0));
        }

        [Test]
        public void CheckSpawnPacket()
        {
            byte[] data = myObstacle.GetSpawnPacket().GetData();

            uint objectType = BitConverter.ToUInt32(data, 5);
            uint netId = BitConverter.ToUInt32(data, 9);
            float x = BitConverter.ToSingle(data, 13);
            float y = BitConverter.ToSingle(data, 17);
            float height = BitConverter.ToSingle(data, 21);
            float width = BitConverter.ToSingle(data, 25);

            Assert.That(objectType, Is.EqualTo(myObstacle.ObjectType));
            Assert.That(x, Is.EqualTo(myObstacle.X));
            Assert.That(y, Is.EqualTo(myObstacle.Y));
            Assert.That(netId, Is.EqualTo(myObstacle.Id));
            Assert.That(width, Is.EqualTo(myObstacle.Width));
            Assert.That(height, Is.EqualTo(myObstacle.Height));
        }
    }
}

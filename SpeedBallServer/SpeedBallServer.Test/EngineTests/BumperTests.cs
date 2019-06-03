using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Numerics;
using System.Threading.Tasks;


namespace SpeedBallServer.Test.EngineTests
{
    public class BumperTests
    {
        private Bumper myBumper;

        [SetUp]
        public void SetUp()
        {
            myBumper = new Bumper(null, 1, 1);
        }

        [Test]
        public void InternalObjId()
        {
            Assert.That(myBumper.ObjectType, Is.EqualTo((uint)InternalObjectsId.Bumper));
        }

        [Test]
        public void RigidBodyType()
        {
            Assert.That(myBumper.RigidBody.Type, Is.EqualTo((uint)ColliderType.Obstacle));
        }

        [Test]
        public void RigidBodyCollisionMask()
        {
            Assert.That(myBumper.RigidBody.CollisionMask, Is.EqualTo(0));
        }

        [Test]
        public void CheckSpawnPacket()
        {
            byte[] data = myBumper.GetSpawnPacket().GetData();

            uint objectType = BitConverter.ToUInt32(data, 5);
            uint netId = BitConverter.ToUInt32(data, 9);
            float x = BitConverter.ToSingle(data, 13);
            float y = BitConverter.ToSingle(data, 17);
            float height = BitConverter.ToSingle(data, 21);
            float width = BitConverter.ToSingle(data, 25);

            Assert.That(objectType, Is.EqualTo(myBumper.ObjectType));
            Assert.That(x, Is.EqualTo(myBumper.X));
            Assert.That(y, Is.EqualTo(myBumper.Y));
            Assert.That(netId, Is.EqualTo(myBumper.Id));
            Assert.That(width, Is.EqualTo(myBumper.Width));
            Assert.That(height, Is.EqualTo(myBumper.Height));
        }
    }
}

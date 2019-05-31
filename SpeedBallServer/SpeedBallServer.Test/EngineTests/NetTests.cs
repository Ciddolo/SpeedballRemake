using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Numerics;
using System.Threading.Tasks;

namespace SpeedBallServer.Test.EngineTests
{
    public class NetTests
    {
        private Net myNet;

        [SetUp]
        public void SetUp()
        {
            myNet = new Net(null,1, 1);
        }

        [Test]
        public void InternalObjId()
        {
            Assert.That(myNet.ObjectType, Is.EqualTo((uint)InternalObjectsId.Net));
        }

        [Test]
        public void RigidBodyType()
        {
            Assert.That(myNet.RigidBody.Type, Is.EqualTo((uint)ColliderType.Net));
        }

        [Test]
        public void RigidBodyCollisionMask()
        {
            Assert.That(myNet.RigidBody.CollisionMask, Is.EqualTo(0));
        }

        [Test]
        public void CheckSpawnPacket()
        {
            byte[] data = myNet.GetSpawnPacket().GetData();

            uint objectType = BitConverter.ToUInt32(data, 5);
            uint netId = BitConverter.ToUInt32(data, 9);
            float x = BitConverter.ToSingle(data, 13);
            float y = BitConverter.ToSingle(data, 17);
            float height = BitConverter.ToSingle(data, 21);
            float width = BitConverter.ToSingle(data, 25);
            float teamId = BitConverter.ToUInt32(data, 29);

            Assert.That(objectType, Is.EqualTo(myNet.ObjectType));
            Assert.That(x, Is.EqualTo(myNet.X));
            Assert.That(y, Is.EqualTo(myNet.Y));
            Assert.That(netId, Is.EqualTo(myNet.Id));
            Assert.That(teamId, Is.EqualTo(myNet.TeamId));
            Assert.That(width, Is.EqualTo(myNet.Width));
            Assert.That(height, Is.EqualTo(myNet.Height));
        }
    }
}

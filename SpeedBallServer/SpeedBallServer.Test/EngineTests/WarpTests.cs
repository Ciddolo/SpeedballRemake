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
        public void InternalObjId()
        {
            Assert.That(myWarp.ObjectType, Is.EqualTo((uint)InternalObjectsId.Warp));
        }

        [Test]
        public void RigidBodyType()
        {
            Assert.That(myWarp.RigidBody.Type, Is.EqualTo((uint)ColliderType.Warp));
        }

        [Test]
        public void RigidBodyCollisionMask()
        {
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
    }
}

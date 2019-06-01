using System;
using System.Numerics;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer.Test.EngineTests
{
    class GoallkeeperTests
    {
        private Goalkeeper myGoalkeeper;

        [SetUp]
        public void SetUp()
        {
            myGoalkeeper = new Goalkeeper(null, 1, 1);
        }

        [Test]
        public void InternalObjId()
        {
            Assert.That(myGoalkeeper.ObjectType, Is.EqualTo((uint)InternalObjectsId.Goalkeeper));
        }

        [Test]
        public void RigidBodyType()
        {
            Assert.That(myGoalkeeper.RigidBody.Type, Is.EqualTo((uint)ColliderType.Goalkeeper));
        }

        [Test]
        public void RigidBodyCollisionMask()
        {
            Assert.That(myGoalkeeper.RigidBody.CollisionMask, Is.EqualTo(11));
        }

        [Test]
        public void CheckSpawnPacket()
        {
            byte[] data = myGoalkeeper.GetSpawnPacket().GetData();

            uint objectType = BitConverter.ToUInt32(data, 5);
            uint netId = BitConverter.ToUInt32(data, 9);
            float x = BitConverter.ToSingle(data, 13);
            float y = BitConverter.ToSingle(data, 17);
            float height = BitConverter.ToSingle(data, 21);
            float width = BitConverter.ToSingle(data, 25);
            float teamId = BitConverter.ToUInt32(data, 29);

            Assert.That(objectType, Is.EqualTo(myGoalkeeper.ObjectType));
            Assert.That(x, Is.EqualTo(myGoalkeeper.X));
            Assert.That(y, Is.EqualTo(myGoalkeeper.Y));
            Assert.That(netId, Is.EqualTo(myGoalkeeper.Id));
            Assert.That(teamId, Is.EqualTo(myGoalkeeper.TeamId));
            Assert.That(width, Is.EqualTo(myGoalkeeper.Width));
            Assert.That(height, Is.EqualTo(myGoalkeeper.Height));
        }

        [Test]
        public void CheckUpdatePacket()
        {
            byte[] data = myGoalkeeper.GetUpdatePacket().GetData();

            uint netId = BitConverter.ToUInt32(data, 5);
            float x = BitConverter.ToSingle(data, 9);
            float y = BitConverter.ToSingle(data, 13);
            uint state = BitConverter.ToUInt32(data, 17);

            Assert.That(netId, Is.EqualTo(myGoalkeeper.Id));
            Assert.That(x, Is.EqualTo(myGoalkeeper.X));
            Assert.That(y, Is.EqualTo(myGoalkeeper.Y));
            Assert.That(state, Is.EqualTo((uint)myGoalkeeper.State));
        }
    }
}

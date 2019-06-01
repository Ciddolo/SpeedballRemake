﻿using System;
using NUnit.Framework;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer.Test.EngineTests
{
    class BallTests
    {
        private Ball myBall;

        [SetUp]
        public void SetUp()
        {
            myBall = new Ball(null, 1, 1);
        }

        [Test]
        public void RigidBodyType()
        {
            Assert.That(myBall.RigidBody.Type, Is.EqualTo((uint)ColliderType.Ball));
        }

        [Test]
        public void InternalObjId()
        {
            Assert.That(myBall.ObjectType, Is.EqualTo((uint)InternalObjectsId.Ball));
        }

        [Test]
        public void RigidBodyCollisionMask()
        {
            Assert.That(myBall.RigidBody.CollisionMask, Is.EqualTo(12));
        }

        [Test]
        public void MaxMagnification()
        {
            myBall.Shot(new Vector2(1f, 1f), new Vector2(1f, 0f) * 20f);
            myBall.Update(0f);
            Assert.That(myBall.Magnification, Is.EqualTo(1f));
        }

        [Test]
        public void NoMagnification()
        {
            myBall.Shot(new Vector2(1f, 1f), new Vector2(1f, 0f) * 9f);
            myBall.Update(0f);
            Assert.That(myBall.Magnification, Is.EqualTo(0f));
        }

        [Test]
        public void AttachingBallToPlayer()
        {
            Player myPlayer = new Player(null, 1, 1);

            myBall.AttachToPlayer(myPlayer);

            Assert.That(myBall.PlayerWhoOwnsTheBall, Is.EqualTo(myPlayer));
            Assert.That(myPlayer.Ball, Is.EqualTo(myBall));
            Assert.That(myPlayer.Ball.RigidBody.Velocity, Is.EqualTo(Vector2.Zero));
            Assert.That(myPlayer.Ball.RigidBody.IsCollisionsAffected, Is.EqualTo(false));
        }

        [Test]
        public void BallFollowingPlayerWhoOwnsTheBall()
        {
            Player myPlayer = new Player(null, 1, 1);

            myPlayer.Position = new Vector2(100f,100f);
            myBall.AttachToPlayer(myPlayer);

            myBall.Update(0f);

            Assert.That(myPlayer.Position, Is.EqualTo(new Vector2(100f, 100f)));
        }

        [Test]
        public void DeattachingBallFromPlayer()
        {
            Player myPlayer = new Player(null, 1, 1);

            myBall.AttachToPlayer(myPlayer);

            myBall.RemoveToPlayer();

            Assert.That(myBall.PlayerWhoOwnsTheBall, Is.EqualTo(null));
            Assert.That(myPlayer.Ball, Is.EqualTo(null));
        }

        [Test]
        public void Shot()
        {
            Player myPlayer = new Player(null, 1, 1);

            myBall.AttachToPlayer(myPlayer);
            myBall.Shot(new Vector2(1f, 0f), new Vector2(1f, 0f) * 20f);

            myBall.RigidBody.Update(1f);
            myBall.Update(1f);

            Assert.That(myBall.RigidBody.IsGravityAffected, Is.EqualTo(true));
            Assert.That(myBall.PlayerWhoOwnsTheBall, Is.EqualTo(null));
            Assert.That(myBall.Position, Is.EqualTo(new Vector2(21f, 0f)));
            Assert.That(myPlayer.Ball, Is.EqualTo(null));
            Assert.That(myBall.Magnification, Is.EqualTo(6 / 10f));
        }

        [Test]
        public void Reset()
        {
            Player myPlayer = new Player(null, 1, 1);

            myBall.AttachToPlayer(myPlayer);
            myBall.Shot(Vector2.Zero,new Vector2(20f,20f));

            myBall.Reset();

            Assert.That(myPlayer.Ball, Is.EqualTo(null));
            Assert.That(myBall.PlayerWhoOwnsTheBall, Is.EqualTo(null));
            Assert.That(myBall.RigidBody.Velocity, Is.EqualTo(Vector2.Zero));
        }

        [Test]
        public void CheckUpdatePacket()
        {
            byte[] data = myBall.GetUpdatePacket().GetData();

            uint netId = BitConverter.ToUInt32(data, 5);
            float x = BitConverter.ToSingle(data, 9);
            float y = BitConverter.ToSingle(data, 13);
            float magnification = BitConverter.ToSingle(data, 17);

            Assert.That(netId, Is.EqualTo(myBall.Id));
            Assert.That(x, Is.EqualTo(myBall.X));
            Assert.That(y, Is.EqualTo(myBall.Y));
            Assert.That(magnification, Is.EqualTo(myBall.Magnification));
        }

        [Test]
        public void CheckSpawnPacket()
        {
            byte[] data = myBall.GetSpawnPacket().GetData();

            uint objectType = BitConverter.ToUInt32(data, 5);
            uint netId = BitConverter.ToUInt32(data, 9);
            float x = BitConverter.ToSingle(data, 13);
            float y = BitConverter.ToSingle(data, 17);
            float height = BitConverter.ToSingle(data, 21);
            float width = BitConverter.ToSingle(data, 25);

            Assert.That(objectType, Is.EqualTo(myBall.ObjectType));
            Assert.That(x, Is.EqualTo(myBall.X));
            Assert.That(y, Is.EqualTo(myBall.Y));
            Assert.That(netId, Is.EqualTo(myBall.Id));
            Assert.That(width, Is.EqualTo(myBall.Width));
            Assert.That(height, Is.EqualTo(myBall.Height));
        }
    }
}

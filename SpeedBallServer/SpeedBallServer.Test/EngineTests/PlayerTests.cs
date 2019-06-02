using System;
using System.Numerics;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer.Test.EngineTests
{
    public class PlayerTests
    {
        private Player myPlayer;

        [Test]
        public void InternalObjId()
        {
            Assert.That(myPlayer.ObjectType, Is.EqualTo((uint)InternalObjectsId.Player));
        }

        [Test]
        public void Reset()
        {
            Ball myBall = new Ball(null, 1, 1);

            myBall.AttachToPlayer(myPlayer);

            myPlayer.Reset();

            Assert.That(myPlayer.Ball, Is.EqualTo(null));
            Assert.That(myBall.PlayerWhoOwnsTheBall, Is.EqualTo(null));
        }

        [Test]
        public void RigidBodyType()
        {
            Assert.That(myPlayer.RigidBody.Type, Is.EqualTo((uint)ColliderType.Player));
        }

        [Test]
        public void RigidBodyCollisionMask()
        {
            Assert.That(myPlayer.RigidBody.CollisionMask, Is.EqualTo(15));
        }

        [SetUp]
        public void SetUp()
        {
            myPlayer = new Player(null, 1, 1);
        }

        [Test]
        public void DefaultState()
        {
            Assert.That(myPlayer.State, Is.EqualTo(PlayerState.Idle));
        }

        [Test]
        public void DefaultStateAfterUpdate()
        {
            myPlayer.Update(1f);
            Assert.That(myPlayer.State, Is.EqualTo(PlayerState.Idle));
        }

        [Test]
        public void StateAfterTackling()
        {
            myPlayer.StartTackling();
            Assert.That(myPlayer.State, Is.EqualTo(PlayerState.Tackling));
        }

        [Test]
        public void StateAfterStopTackling()
        {
            myPlayer.StartTackling();
            myPlayer.Update(Player.TacklingTime);
            Assert.That(myPlayer.State, Is.EqualTo(PlayerState.Idle));
        }

        [Test]
        public void StateAfterStunned()
        {
            myPlayer.GetStunned();
            Assert.That(myPlayer.State, Is.EqualTo(PlayerState.Stunned));
        }

        [Test]
        public void PlayerResetState()
        {
            myPlayer.GetStunned();
            myPlayer.Reset();
            Assert.That(myPlayer.State, Is.EqualTo(PlayerState.Idle));

        }

        [Test]
        public void StateAfterStopStunned()
        {
            myPlayer.GetStunned();
            myPlayer.Update(Player.StunnedTime);
            Assert.That(myPlayer.State, Is.EqualTo(PlayerState.Idle));
        }

        [Test]
        public void StateResettingAfterElapsedTime()
        {
            myPlayer.GetStunned();
            myPlayer.Update(Player.TacklingTime);
            Assert.That(myPlayer.State, Is.EqualTo(PlayerState.Stunned));
        }

        [Test]
        public void CheckUpdatePacket()
        {
            byte[] data = myPlayer.GetUpdatePacket().GetData();

            uint netId = BitConverter.ToUInt32(data, 5);
            float x = BitConverter.ToSingle(data, 9);
            float y = BitConverter.ToSingle(data, 13);
            uint state = BitConverter.ToUInt32(data, 17);

            Assert.That(netId, Is.EqualTo(myPlayer.Id));
            Assert.That(x, Is.EqualTo(myPlayer.X));
            Assert.That(y, Is.EqualTo(myPlayer.Y));
            Assert.That(state, Is.EqualTo((uint)myPlayer.State));
        }

        [Test]
        public void CheckSpawnPacket()
        {
            byte[] data = myPlayer.GetSpawnPacket().GetData();

            uint objectType = BitConverter.ToUInt32(data, 5);
            uint netId = BitConverter.ToUInt32(data, 9);
            float x = BitConverter.ToSingle(data, 13);
            float y = BitConverter.ToSingle(data, 17);
            float height = BitConverter.ToSingle(data, 21);
            float width = BitConverter.ToSingle(data, 25);
            float teamId = BitConverter.ToUInt32(data, 29);

            Assert.That(objectType, Is.EqualTo(myPlayer.ObjectType));
            Assert.That(x, Is.EqualTo(myPlayer.X));
            Assert.That(y, Is.EqualTo(myPlayer.Y));
            Assert.That(netId, Is.EqualTo(myPlayer.Id));
            Assert.That(teamId, Is.EqualTo(myPlayer.TeamId));
            Assert.That(width, Is.EqualTo(myPlayer.Width));
            Assert.That(height, Is.EqualTo(myPlayer.Height));
        }

        [Test]
        public void ThrowBall()
        {
            Ball myBall = new Ball(null, 1, 1);
            myBall.AttachToPlayer(myPlayer);

            myPlayer.ThrowBall(new Vector2(1f,0f),20);

            Assert.That(myBall.RigidBody.Velocity, Is.EqualTo(new Vector2(20f, 0f)));
            Assert.That(myBall.PlayerWhoOwnsTheBall,Is.EqualTo(null));
        }

    }
}

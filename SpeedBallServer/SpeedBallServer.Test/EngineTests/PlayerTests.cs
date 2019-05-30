using System;
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

    }
}

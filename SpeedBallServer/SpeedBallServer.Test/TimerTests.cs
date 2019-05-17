using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer.Test
{
    public class TimerTests
    {
        private FakeClock myClock;
        private Timer myTimer;
        private int callbackCount;
        private Action myCallback;
        private float currentNow;
        private float lastTickTimestamp;

        [SetUp]
        public void SetUpTest()
        {
            myClock = new FakeClock();
            currentNow = myClock.GetNow();
            lastTickTimestamp = currentNow;
            myCallback = () => { callbackCount++; };
            callbackCount = 0;
        }

        [Test]
        public void UnsucessfulCallback()
        {
            myTimer = new Timer(1, myCallback);
            myTimer.Start();

            float deltaTime = currentNow - lastTickTimestamp;
            myTimer.Update(deltaTime);

            Assert.That(callbackCount, Is.EqualTo(0));
        }

        [Test]
        public void SuccessfulCallback()
        {
            myTimer = new Timer(1, myCallback);
            myTimer.Start();

            myClock.IncreaseTimeStamp(2);
            currentNow = myClock.GetNow();
            float deltaTime = currentNow - lastTickTimestamp;
            myTimer.Update(deltaTime);

            Assert.That(callbackCount, Is.EqualTo(1));
        }


        [Test]
        public void MultipleSuccessfulCallbacks()
        {
            myTimer = new Timer(1, myCallback, true);
            myTimer.Start();

            myClock.IncreaseTimeStamp(1.3f);
            currentNow = myClock.GetNow();
            float deltaTime = currentNow - lastTickTimestamp;
            myTimer.Update(deltaTime);

            myClock.IncreaseTimeStamp(1.3f);
            currentNow = myClock.GetNow();
            deltaTime = currentNow - lastTickTimestamp;
            myTimer.Update(deltaTime);

            Assert.That(callbackCount, Is.EqualTo(2));
        }

        [Test]
        public void MultipleUnsuccessfulCallbacks()
        {
            myTimer = new Timer(1, myCallback);
            myTimer.Start();

            myClock.IncreaseTimeStamp(1.3f);
            currentNow = myClock.GetNow();
            float deltaTime = currentNow - lastTickTimestamp;
            myTimer.Update(deltaTime);

            myClock.IncreaseTimeStamp(1.3f);
            currentNow = myClock.GetNow();
            deltaTime = currentNow - lastTickTimestamp;
            myTimer.Update(deltaTime);

            Assert.That(callbackCount, Is.EqualTo(1));
        }


        [Test]
        public void SuccessfulCallbackOnNowGreaterThanInterval()
        {
            myTimer = new Timer(1, myCallback, true);
            myTimer.Start();

            myClock.IncreaseTimeStamp(1.3f);
            currentNow = myClock.GetNow();
            float deltaTime = currentNow - lastTickTimestamp;
            myTimer.Update(deltaTime);

            myClock.IncreaseTimeStamp(.8f);
            currentNow = myClock.GetNow();
            deltaTime = currentNow - lastTickTimestamp;
            myTimer.Update(deltaTime);

            myClock.IncreaseTimeStamp(.9f);
            currentNow = myClock.GetNow();
            deltaTime = currentNow - lastTickTimestamp;
            myTimer.Update(deltaTime);

            Assert.That(callbackCount, Is.EqualTo(3));
        }

        [Test]
        public void UnsucessfullCalbackTimerNotStarted()
        {
            myTimer = new Timer(1, myCallback);

            myClock.IncreaseTimeStamp(2);
            currentNow = myClock.GetNow();
            float deltaTime = currentNow - lastTickTimestamp;
            myTimer.Update(deltaTime);

            Assert.That(callbackCount, Is.EqualTo(0));
        }
    }
}

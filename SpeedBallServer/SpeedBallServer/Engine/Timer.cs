using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer
{
    public class Timer
    {
        /// <summary>
        /// If the timer is counting the passed time.
        /// </summary>
        private bool isStarted;

        /// <summary>
        /// After how much time the timer will call the given callback.
        /// </summary>
        public float Interval;

        /// <summary>
        /// If the timer will restart after the callback.
        /// Usefull for keeping track of the excess accumulated time for a always running timer.
        /// </summary>
        public bool AutomaticRestart;

        /// <summary>
        /// Function called after the interval is elapsed.
        /// </summary>
        private Action callbackToCall;

        /// <summary>
        /// How much time has passed since the re/start of the clock.
        /// </summary>
        private float accumulatedTime;

        /// <summary>
        /// Gets if the accumulated time is higher or equal of the interval.
        /// </summary>
        private bool isTimeElapsed
        {
            get
            {
                return accumulatedTime  >= Interval;
            }
        }

        public Timer(float interval, Action callback ,bool autoRestart=false)
        {
            AutomaticRestart = autoRestart;
            Interval = interval;
            isStarted = false;
            this.callbackToCall = callback;
        }

        /// <summary>
        /// Start counting the time.
        /// </summary>
        public void Start()
        {
            accumulatedTime = 0f;
            isStarted = true;
        }

        /// <summary>
        /// Stop counting the time.
        /// </summary>
        public void Stop()
        {
            isStarted = false;
        }

        /// <summary>
        /// Update accumulated time by the clock.
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Update(float deltaTime)
        {
            if(!isStarted)
                return;

            accumulatedTime += deltaTime;

            if(isTimeElapsed)
            {
                //Console.WriteLine("time elapsed");
                callbackToCall.Invoke();

                if(AutomaticRestart)
                {
                    accumulatedTime -= Interval;
                }
                else
                {
                    Stop();
                }
            }
        }
    }
}

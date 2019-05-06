using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer.Test
{
    public class FakeClock : IMonotonicClock
    {
        private float timeStamp;

        public FakeClock(float timeStamp = 0)
        {
            this.timeStamp = timeStamp;
        }

        public float GetNow()
        {
            return timeStamp;
        }

        public void IncreaseTimeStamp(float delta)
        {
            if (delta <= 0)
            {
                throw new Exception("invalid delta value");
            }
            timeStamp += delta;
        }
    }
}

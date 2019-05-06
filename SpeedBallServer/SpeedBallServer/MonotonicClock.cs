using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer
{
    class MonotonicClock : IMonotonicClock
    {
        public float GetNow()
        {
            return Stopwatch.GetTimestamp() / (float)Stopwatch.Frequency; ;
        }
    }
}

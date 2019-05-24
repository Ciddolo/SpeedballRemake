﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer
{
    class Program
    {
        static void Main(string[] args)
        {
            GameTransportIPv4 transport = new GameTransportIPv4();
            transport.Bind("127.0.0.1", 5000);

            MonotonicClock monotonicClock = new MonotonicClock();

            string serializedLevel = File.ReadAllText("Level.json");

            GameServer server = new GameServer(transport, monotonicClock,1,serializedLevel);

            server.Run();
        }
    }
}

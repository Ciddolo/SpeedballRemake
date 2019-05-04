using System;
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

            GameServer server = new GameServer(transport, monotonicClock);

            server.Run();
        }
    }
}

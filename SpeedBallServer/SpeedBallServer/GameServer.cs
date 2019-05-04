using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer
{
    public class GameServer
    {
        private IMonotonicClock clock;
        private IGameTransport transport;

        private Dictionary<byte, GameCommand> commandsTable;
        private delegate void GameCommand(byte[] data, EndPoint sender);

        private float updateFrequency;
        private float lastServerTickTimestamp;

        //cached time value
        private float currentNow;
        public float Now
        {
            get
            {
                return currentNow;
            }
        }

        public GameServer(IGameTransport gameTransport, IMonotonicClock clock, int ticksAmount = 1)
        {         
            this.transport = gameTransport;
            this.clock = clock;

            updateFrequency = 1f/ticksAmount;

            commandsTable = new Dictionary<byte, GameCommand>();
            commandsTable[1] = Join;
        }

        public void Run()
        {
            Console.WriteLine("server started");
            while (true)
            {
                SingleStep();
            }
        }

        public void SingleStep()
        {
            currentNow = clock.GetNow();

            EndPoint sender = transport.CreateEndPoint();
            byte[] dataReceived = transport.Recv(256, ref sender);

            //if the packet received is from my endpoint ignore it
            if (transport.BindedEndPoint.Equals(sender))
            {
                return;
            }

            if (dataReceived != null)
            {
                byte gameCommand = dataReceived[0];

                if (commandsTable.ContainsKey(gameCommand))
                {
                    commandsTable[gameCommand](dataReceived, sender);
                }
            }

            float timeSinceLasTick = currentNow - lastServerTickTimestamp;
            if (timeSinceLasTick >= updateFrequency)
            {
                lastServerTickTimestamp = currentNow;
                //to do
                //tick game objects
            }
        }

        /// <summary>
        /// Gives data and where to send it to the transport to send it
        /// </summary>
        /// <param name="data"></param>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public bool Send(byte[] data, EndPoint endPoint)
        {
            return transport.Send(data, endPoint);
        }

        private void Join(byte[] data, EndPoint sender)
        {
            Packet welcome = new Packet(1);
            Send(welcome.GetData(), sender);
        }
    }
}

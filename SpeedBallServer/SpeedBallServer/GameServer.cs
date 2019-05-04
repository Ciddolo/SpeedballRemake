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
        private IGameTransport transport;

        private Dictionary<byte, GameCommand> commandsTable;
        private delegate void GameCommand(byte[] data, EndPoint sender);
        //private IMonotonicClock clock;

        public GameServer(IGameTransport gameTransport)
        {         
            transport = gameTransport;
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
            EndPoint sender = transport.CreateEndPoint();
            byte[] dataReceived = transport.Recv(256, ref sender);

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
            //if the packet received is from my endpoint ignore it
            Packet welcome = new Packet(1);
            Send(welcome.GetData(), sender);
        }
    }
}

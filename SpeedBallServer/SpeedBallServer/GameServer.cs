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
        private delegate byte[] GameCommand(byte[] data, EndPoint sender);
        //private IMonotonicClock clock;

        public GameServer(IGameTransport gameTransport)
        {         
            transport = gameTransport;
            commandsTable = new Dictionary<byte, GameCommand>();
            commandsTable[0] = Join;
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

            if (dataReceived != null)
            {
                byte gameCommand = dataReceived[0];

                if (commandsTable.ContainsKey(gameCommand))
                {
                    byte[] dataToSend = commandsTable[gameCommand](dataReceived, sender);
                    Send(dataToSend, sender);
                }
            }
        }

        /// <summary>
        /// Gives data to send and where to send it to the transport to sendt
        /// </summary>
        /// <param name="data"></param>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public bool Send(byte[] data, EndPoint endPoint)
        {
            return transport.Send(data, endPoint);
        }

        private byte[] Join(byte[] data, EndPoint sender)
        {
            //if the packet received is from my endpoint ignore it
            if (!transport.BindedEndPoint.Equals(sender))
            {               
                return new byte[] { 0x1 };
            }
            else
            {
                return new byte[] { 0x2 };
            }
        }
    }
}

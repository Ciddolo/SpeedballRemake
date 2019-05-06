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
        private Dictionary<EndPoint, GameClient> clientsTable;

        private int maxPlayers;

        private float updateFrequency;
        private float lastServerTickTimestamp;
        private float maxTimeOfInactivity;

        public int ClientsAmount
        {
            get
            {
                return clientsTable.Count;
            }
        }

        private bool CheckIfClientJoined(EndPoint client)
        {
            return clientsTable.ContainsKey(client);
        }

        public int GetClientMalus(EndPoint client)
        {
            if (!CheckIfClientJoined(client))
                return -1;
            return (int)clientsTable[client].Malus;
        }

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
            clientsTable = new Dictionary<EndPoint, GameClient>();
            updateFrequency = 1f/ticksAmount;
            maxPlayers = 2;
            maxTimeOfInactivity = 30f;
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
                    if (CheckIfClientJoined(sender))
                    {
                        clientsTable[sender].LastPacketTimestamp = currentNow;
                    }
                }
            }

            float timeSinceLasTick = currentNow - lastServerTickTimestamp;
            if (timeSinceLasTick >= updateFrequency)
            {
                lastServerTickTimestamp = currentNow;
                //to do
                //tick game objects

                List<EndPoint> deadClients=new List<EndPoint>();

                foreach (GameClient client in clientsTable.Values)
                {
                    float timeSinceLastPacket = currentNow - client.LastPacketTimestamp;

                    if (timeSinceLastPacket>maxTimeOfInactivity)
                    {
                        deadClients.Add(client.EndPoint);
                    }
                    else
                    {
                        client.Process();
                    }
                }

                foreach (EndPoint deadClient in deadClients)
                {
                    clientsTable.Remove(deadClient);
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

        public bool Send(Packet data, EndPoint endPoint)
        {
            return this.Send(data.GetData(), endPoint);
        }

        private void Join(byte[] data, EndPoint sender)
        {
            // check if the client has already joined
            if (CheckIfClientJoined(sender))
            {
                GameClient badClient = clientsTable[sender];
                badClient.Malus++;
                return;
            }

            if (ClientsAmount >= maxPlayers)
                return;

            GameClient newClient = new GameClient(this, sender);
            clientsTable[sender] = newClient;

            Packet welcome = new Packet(1);
            newClient.Enqueue(welcome);
        }
    }
}

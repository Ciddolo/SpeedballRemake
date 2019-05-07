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
        private Dictionary<uint, GameObject> gameObjectsTable;
        private Dictionary<EndPoint, GameClient> clientsTable;

        private int maxPlayers;

        public float UpdateFrequency;
        public float LastServerTickTimestamp;
        public float MaxTimeOfInactivity;

        public float MaxAttemptsForPacket;
        public float SendAfterTimeForPackets;

        public int ClientsAmount
        {
            get
            {
                return clientsTable.Count;
            }
        }

        public int GameObjectsAmount
        {
            get
            {
                return gameObjectsTable.Count;
            }
        }

        private bool CheckIfClientJoined(EndPoint client)
        {
            return clientsTable.ContainsKey(client);
        }

        public List<uint> AcksIdsNeededFrom(EndPoint client)
        {
            if (!CheckIfClientJoined(client))
                return null;
            return clientsTable[client].AcksNedeedIds();
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
            gameObjectsTable = new Dictionary<uint, GameObject>();

            UpdateFrequency = 1f/ticksAmount;
            maxPlayers = 2;
            MaxTimeOfInactivity = 30f;
            MaxAttemptsForPacket = 3;
            SendAfterTimeForPackets = .1f;

            commandsTable = new Dictionary<byte, GameCommand>();
            commandsTable[0] = Join;
            commandsTable[255] = Ack;
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

            float timeSinceLasTick = currentNow - LastServerTickTimestamp;
            if (timeSinceLasTick >= UpdateFrequency)
            {
                LastServerTickTimestamp = currentNow;
                //to do
                //update game objects
                //physics update
                //tick game objects

                List<EndPoint> deadClients=new List<EndPoint>();

                foreach (GameClient client in clientsTable.Values)
                {
                    float timeSinceLastPacket = currentNow - client.LastPacketTimestamp;

                    if (timeSinceLastPacket>MaxTimeOfInactivity)
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
                badClient.Malus+=100;
                return;
            }

            if (ClientsAmount >= maxPlayers)
                return;

            GameClient newClient = new GameClient(this, sender);
            clientsTable[sender] = newClient;
            //to do spawn 5 players with one default and one gk
            //spawning one instead for now
            Player player = Spawn<Player>();
            player.SetOwner(newClient);
            Packet welcome = new Packet(1,true, player.ObjectType, player.Id);

            newClient.Enqueue(welcome);

        }

        private void Ack(byte[] data, EndPoint sender)
        {
            if (!CheckIfClientJoined(sender))
            {
                return;
            }

            GameClient client = clientsTable[sender];
            uint packetId = BitConverter.ToUInt32(data, 6);

            client.Ack(packetId);
        }

        public void RegisterGameObject(GameObject gameObject)
        {
            if (gameObjectsTable.ContainsKey(gameObject.Id))
                throw new Exception("GameObject already registered");
            gameObjectsTable[gameObject.Id] = gameObject;
        }

        public T Spawn<T>() where T : GameObject
        {
            object[] ctorParams = { this };
            T newGameObject = Activator.CreateInstance(typeof(T), ctorParams) as T;
            RegisterGameObject(newGameObject);
            return newGameObject;
        }
    }
}

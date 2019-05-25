using System;
using System.Net;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer
{
    public enum PacketsCommands
    {
        Join=0,
        Welcome=1,
        Spawn=2,
        Update=3,
        Input=4,
        GameInfo=5,
        Pong = 253,
        Ping = 254,
        Ack =255
    }

    public class GameServer
    {
        private Timer gameLogicTimer;

        private IMonotonicClock clock;
        private IGameTransport transport;

        private Dictionary<byte, GameCommand> commandsTable;
        private delegate void GameCommand(byte[] data, EndPoint sender);
        private Dictionary<EndPoint, GameClient> clientsTable;
        private GameLogic gameLogic;
        private Dictionary<uint, GameObject> gameObjectsTable;
        public Dictionary<uint, GameObject> GameObjectsTable
        {
            get
            {
                return gameObjectsTable;
            }
        }

        public GameState CurrentGameState()
        {
            return gameLogic.GameStatus;
        }

        private int maxPlayers;

        public int MaxPlayers
        {
            get
            {
                return maxPlayers;
            }
        }

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

        public float GetClientPing(EndPoint client)
        {
            if (!CheckIfClientJoined(client))
                return -1;
            return clientsTable[client].PingValue;
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

        public GameServer(IGameTransport gameTransport, IMonotonicClock clock, int ticksAmount = 1,string startingLevel=null)
        {         
            this.transport = gameTransport;
            this.clock = clock;
            currentNow = clock.GetNow();
            LastServerTickTimestamp = currentNow;

            clientsTable = new Dictionary<EndPoint, GameClient>();
            gameObjectsTable = new Dictionary<uint, GameObject>();

            UpdateFrequency = 1f/ticksAmount;
            maxPlayers = 2;
            MaxTimeOfInactivity = 30f;
            MaxAttemptsForPacket = 3;
            SendAfterTimeForPackets = .1f;

            commandsTable = new Dictionary<byte, GameCommand>();
            commandsTable[(byte)PacketsCommands.Join] = Join;
            commandsTable[(byte)PacketsCommands.Ack] = Ack;
            commandsTable[(byte)PacketsCommands.Update] = Update;
            commandsTable[(byte)PacketsCommands.Ping] = Ping;
            commandsTable[(byte)PacketsCommands.Pong] = Pong;

            gameLogic = new GameLogic(this);
            gameLogic.SpawnLevel(startingLevel);

            gameLogicTimer = new Timer(UpdateFrequency, GameLogicTick, true);
            gameLogicTimer.Start();

        }

        public bool CheckGameObjectOwner(uint id,EndPoint owner)
        {
            if (gameObjectsTable.ContainsKey(id))
            {
                if(gameObjectsTable[id].Owner!=null)
                    return gameObjectsTable[id].Owner.EndPoint == owner;
            }

            return false;
        }

        public void Run()
        {
            Console.WriteLine("server started");

            while (true)
            {
                SingleStep();
            }
        }

        private void GameLogicTick()
        {
            //Console.WriteLine("tick :" + currentNow);
            gameLogic.Update();

            List<EndPoint> deadClients = new List<EndPoint>();

            foreach (GameClient client in clientsTable.Values)
            {
                if (client.Malus > 5000)
                {
                    //Console.WriteLine("kicking");
                    deadClients.Add(client.EndPoint);
                }
                else
                {
                    client.Process(gameLogicTimer.Interval);
                }
            }

            foreach (EndPoint deadClient in deadClients)
            {
                gameLogic.RemovePlayer(clientsTable[deadClient]);
                clientsTable.Remove(deadClient);
            }

        }

        public void SingleStep()
        {
            currentNow = clock.GetNow();

            EndPoint sender = transport.CreateEndPoint();
            byte[] dataReceived = transport.Recv(256, ref sender);

            if (dataReceived != null)
            {
                //if the packet received is from my endpoint ignore it
                if (!transport.BindedEndPoint.Equals(sender))
                {
                    byte gameCommand = dataReceived[0];

                    if (commandsTable.ContainsKey(gameCommand))
                    {
                        commandsTable[gameCommand](dataReceived, sender);
                    }
                }
            }

            float deltaTime = currentNow - LastServerTickTimestamp;
            gameLogicTimer.Update(deltaTime);
            LastServerTickTimestamp = currentNow;
        }

        /// <summary>
        /// Gives data and where to send it to the transport to send it
        /// </summary>
        /// <param name="data"></param>
        /// <param name="endPoint"></param>
        public bool Send(byte[] data, EndPoint endPoint)
        {
            return transport.Send(data, endPoint);
        }

        /// <summary>
        /// Gives data and where to send it to the transport to send it
        /// </summary>
        /// <param name="data"></param>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public bool Send(Packet data, EndPoint endPoint)
        {
            return this.Send(data.GetData(), endPoint);
        }

        private void Ping(byte[] data, EndPoint sender)
        {
            uint packetId = BitConverter.ToUInt32(data, 1);
            Packet pongPacket = new Packet((byte)PacketsCommands.Pong, false, packetId);
            Send(pongPacket.GetData(), sender);
        }

        private void Pong(byte[] data, EndPoint sender)
        {
            //Console.WriteLine("pong");
            if (!CheckIfClientJoined(sender))
            {
                return;
            }

            GameClient client = clientsTable[sender];
            uint packetId = BitConverter.ToUInt32(data, 5);

            client.Pong(packetId);
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

            uint controlledPlayerId;
            uint clientId = gameLogic.AddClient(newClient,out controlledPlayerId);

            Packet welcome = new Packet((byte)PacketsCommands.Welcome, true,clientId, controlledPlayerId);
            newClient.Enqueue(welcome);
            newClient.Ping();

            foreach (GameObject gameObject in gameObjectsTable.Values)
            {
                newClient.Enqueue(gameObject.GetSpawnPacket());
            }
        }

        private void Update(byte[] data, EndPoint sender)
        {

            if (!clientsTable.ContainsKey(sender))
            {
                //Console.WriteLine("unknown client");
                return;
            }

            GameClient client = clientsTable[sender];

            uint netId = BitConverter.ToUInt32(data, 5);
            if (gameObjectsTable.ContainsKey(netId))
            {
                //Console.WriteLine("object found");
                gameLogic.ClientUpdate(data, client);
            }
            else
            {
                client.Malus++;
            }
        }

        private void Ack(byte[] data, EndPoint sender)
        {
            if (!CheckIfClientJoined(sender))
            {
                return;
            }

            GameClient client = clientsTable[sender];
            uint packetId = BitConverter.ToUInt32(data, 5);

            client.Ack(packetId);
        }

        public void RegisterGameObject(GameObject gameObject)
        {
            if (gameObjectsTable.ContainsKey(gameObject.Id))
                throw new Exception("GameObject already registered");
            gameObjectsTable[gameObject.Id] = gameObject;
        }

        public T Spawn<T>(params object[] additionalParams) where T : GameObject
        {
            List<object> ctorParamsList = new List<object>();

            ctorParamsList.Add(this);

            foreach (object param in additionalParams)
            {
                ctorParamsList.Add(param);
            }

            T newGameObject = Activator.CreateInstance(typeof(T), ctorParamsList.ToArray()) as T;
            RegisterGameObject(newGameObject);
            return newGameObject;
        }

        public void SendToAllClients(Packet packet)
        {
            foreach (GameClient client in clientsTable.Values)
            {
                client.Enqueue(packet);
            }
        }
    }
}

using System;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer
{
    public struct ClientInfo
    {
        public uint TeamId;
        public uint ControlledPlayerId;
        //public byte[] InputData;
        //public PlayerState Sate;
    }

    //public enum PlayerState
    //{
    //    WithoutInput,
    //    HasInputReady,
    //}

    public enum GameState
    {
        WaitingForPlayers,
        ResettingPlayersPositions,
        Playing,
        Ended
    }

    public class GameLogic
    {
        private GameServer server;
        public GameState GameStatus;
        public Dictionary<GameClient, ClientInfo> Clients;
        private float startTimestamp;

        public uint[] Score;

        private List<IUpdatable> updatableItems;
        private PhysicsHandler physicsHandler;
        private List<Player>[] teams;
        private List<Player> teamOneControllablePlayers;
        private List<Player> teamTwoControllablePlayers;

        private uint defaultPlayerTeamOneId,defaultPlayerTeamTwoId;

        public uint AddClient(GameClient client,out uint controlledPlayerId)
        {
            ClientInfo newClientInfo = new ClientInfo();

            newClientInfo.TeamId = 0;
            newClientInfo.ControlledPlayerId = teamOneControllablePlayers[(int)defaultPlayerTeamOneId].Id;

            if (!Clients.ContainsKey(client))
            {
                if (Clients.Count == 1 )
                {
                    if(Clients.First().Value.TeamId==0)
                    {
                        newClientInfo.TeamId = 1;
                        newClientInfo.ControlledPlayerId = teamTwoControllablePlayers[(int)defaultPlayerTeamTwoId].Id;
                    }

                    GameStatus = GameState.Playing;
                    startTimestamp = server.Now;
                }

                Clients.Add(client,newClientInfo);
            }

            foreach (Player player in teams[newClientInfo.TeamId])
            {
                player.SetOwner(client);
            }

            if (Clients.Count >= server.MaxPlayers)
            {
                GameStatus = GameState.Playing;
            }

            controlledPlayerId = newClientInfo.ControlledPlayerId;

            return newClientInfo.TeamId;
        }

        public void RemovePlayer(GameClient clientToRemove)
        {
            foreach (Player player in teams[Clients[clientToRemove].TeamId])
            {
                player.SetOwner(null);
            }

            Clients.Remove(clientToRemove);
            if(GameStatus==GameState.Playing)
                GameStatus = GameState.Ended;
        }

        private void SpawnTestingLevel()
        {
            Obstacle myObstacle = server.Spawn<Obstacle>(1, 10);
            physicsHandler.AddItem(myObstacle.RigidBody);
            myObstacle.SetPosition(0f, 3f);

            Player defPlayerTeamOne = server.Spawn<Player>(1,1);
            defaultPlayerTeamOneId = 0;
            defPlayerTeamOne.SetStartingPosition(-1f, 0f);
            defPlayerTeamOne.TeamId = 0;
            teamOneControllablePlayers.Add(defPlayerTeamOne);
            updatableItems.Add(defPlayerTeamOne);
            physicsHandler.AddItem(defPlayerTeamOne.RigidBody);

            Player defAnotherPlayerTeamOne = server.Spawn<Player>(1,1);
            defAnotherPlayerTeamOne.SetStartingPosition(3f, 0f);
            defAnotherPlayerTeamOne.TeamId = 0;
            updatableItems.Add(defAnotherPlayerTeamOne);
            physicsHandler.AddItem(defAnotherPlayerTeamOne.RigidBody);

            teamOneControllablePlayers.Add(defAnotherPlayerTeamOne);

            Player defPlayerTeamTwo = server.Spawn<Player>(1,1);
            defaultPlayerTeamTwoId = 0;
            defPlayerTeamTwo.SetStartingPosition(1f, 0f);
            defPlayerTeamTwo.TeamId = 1;
            teamTwoControllablePlayers.Add(defPlayerTeamTwo);
            physicsHandler.AddItem(defPlayerTeamTwo.RigidBody);
            updatableItems.Add(defPlayerTeamTwo);

            ResetPositions();
        }

        private void SpawnSerializedLevel(string serializedLevel)
        {
            Level levelData = JsonConvert.DeserializeObject<Level>(serializedLevel);

            PlayersInfo playerInfo = levelData.PlayerInfo;

            defaultPlayerTeamTwoId = (uint)playerInfo.DefaultPlayerIndex;
            defaultPlayerTeamOneId = (uint)playerInfo.DefaultPlayerIndex;

            foreach (var obstacleInfo in levelData.Walls)
            {
                Obstacle myObstacle = server.Spawn<Obstacle>(obstacleInfo.Height, obstacleInfo.Width);
                physicsHandler.AddItem(myObstacle.RigidBody);
                myObstacle.SetPosition(obstacleInfo.Position);

                myObstacle.Name = obstacleInfo.Name;
            }

            for (int i = 0; i < levelData.TeamOneSpawnPositions.Count; i++)
            {
                SimpleLevelObject data = levelData.TeamOneSpawnPositions[i];

                Player player = server.Spawn<Player>(playerInfo.Height, playerInfo.Width);
                player.SetStartingPosition(data.Position);

                player.TeamId = 0;
                teamOneControllablePlayers.Add(player);

                updatableItems.Add(player);
                physicsHandler.AddItem(player.RigidBody);

                player.Name = levelData.TeamOneSpawnPositions[i].Name;
            }

            for (int i = 0; i < levelData.TeamTwoSpawnPositions.Count; i++)
            {
                SimpleLevelObject data = levelData.TeamOneSpawnPositions[i];
                Player player = server.Spawn<Player>(playerInfo.Height, playerInfo.Width);
                player.SetStartingPosition(data.Position);

                player.TeamId = 1;
                teamTwoControllablePlayers.Add(player);

                updatableItems.Add(player);
                physicsHandler.AddItem(player.RigidBody);

                player.Name = levelData.TeamOneSpawnPositions[i].Name;
            }

            Net TeamOneNet = server.Spawn<Net>(levelData.NetTeamOne.Height, levelData.NetTeamOne.Width);
            TeamOneNet.Name = levelData.NetTeamOne.Name;
            physicsHandler.AddItem(TeamOneNet.RigidBody);

            Net TeamTwoNet = server.Spawn<Net>(levelData.NetTeamTwo.Height, levelData.NetTeamTwo.Width);
            TeamTwoNet.Name = levelData.NetTeamTwo.Name;
            physicsHandler.AddItem(TeamTwoNet.RigidBody);

            Ball Ball = server.Spawn<Ball>(levelData.Ball.Height, levelData.Ball.Width);
            Ball.Name = levelData.Ball.Name;
            physicsHandler.AddItem(Ball.RigidBody);
            updatableItems.Add(Ball);

        }

        public void SpawnLevel(string serializedLevel=null)
        {
            if (serializedLevel == null)
                SpawnTestingLevel();
            else
                SpawnSerializedLevel(serializedLevel);

            Console.WriteLine("Loaded Level");
            teams[0] = teamOneControllablePlayers;
            teams[1] = teamTwoControllablePlayers;

            Score = new uint[server.MaxPlayers];
        }

        public GameLogic(GameServer server)
        {
            this.server = server;
            Clients = new Dictionary<GameClient, ClientInfo>();
            GameStatus = GameState.WaitingForPlayers;
            teams = new List<Player>[server.MaxPlayers];
            teamOneControllablePlayers = new List<Player>();
            teamTwoControllablePlayers = new List<Player>();
            updatableItems = new List<IUpdatable>();
            physicsHandler = new PhysicsHandler();
        }

        public void ClientUpdate(byte[] packetData,GameClient client)
        {
            if (GameStatus != GameState.Playing)
            {
                client.Malus++;
                return;
            }

            uint netId = BitConverter.ToUInt32(packetData, 5);
            GameObject gameObject = server.GameObjectsTable[netId];

            if (gameObject.IsOwnedBy(client) && Clients[client].ControlledPlayerId==netId)
            {
                Player playerToMove = (Player)gameObject;

                float posX, posY, dirX, dirY;

                posX = BitConverter.ToSingle(packetData, 9);
                posY = BitConverter.ToSingle(packetData, 13);
                dirX = BitConverter.ToSingle(packetData, 17);
                dirY = BitConverter.ToSingle(packetData, 21);

                playerToMove.SetPosition(posX,posY);
                playerToMove.SetLookingRotation(dirX,dirY);
            }
            else
            {
                client.Malus += 10;
            }
        }

        public void Update()
        {
            physicsHandler.Update();
            foreach (IUpdatable item in updatableItems)
            {
               item.Update();
            }
            physicsHandler.CheckCollisions();
        }

        public void ResetPositions()
        {
            foreach (IUpdatable item in updatableItems)
            {
                item.Reset();
            }
        }

        public Packet GetGameInfoPacket()
        {
            return new Packet((int)PacketsCommands.GameInfo, false, Score[0], Score[1], (uint)GameStatus);
        }
    }
}

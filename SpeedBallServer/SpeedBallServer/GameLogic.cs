using System;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer
{
    public class ClientInfo
    {
        public uint TeamId;
        public uint ControlledPlayerId;
    }

    public enum InputType
    {
        SelectPlayer,
        Shot,
        Tackle,
        Movement
    }

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
                SimpleLevelObject data = levelData.TeamTwoSpawnPositions[i];
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
            TeamOneNet.TeamId = 0;
            TeamOneNet.Position = levelData.NetTeamOne.Position;
            physicsHandler.AddItem(TeamOneNet.RigidBody);

            Net TeamTwoNet = server.Spawn<Net>(levelData.NetTeamTwo.Height, levelData.NetTeamTwo.Width);
            TeamTwoNet.Name = levelData.NetTeamTwo.Name;
            TeamTwoNet.TeamId = 1;
            TeamTwoNet.Position = levelData.NetTeamTwo.Position;
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

            inputCommandsTable = new Dictionary<byte, GameCommand>();
            inputCommandsTable[(byte)InputType.SelectPlayer] = SelectPlayer;
            inputCommandsTable[(byte)InputType.Movement] = MovementDir;
        }

        private void SelectPlayer(byte[] data, GameClient sender)
        {
            uint playerId = BitConverter.ToUInt32(data, 6);
            GameObject playerToControl = server.GameObjectsTable[playerId];
            //Console.WriteLine("selecting "+playerId+" selected"+ Clients[sender].ControlledPlayerId);

            if (playerToControl is Player)
            {
                if (playerToControl.Owner == sender)
                {
                    Clients[sender].ControlledPlayerId = playerId;
                }
                else
                {
                    sender.Malus += 1;
                }
            }
            else
            {
                sender.Malus += 10;
            }

        }

        private void MovementDir(byte[] data, GameClient sender)
        {
            uint playerId = Clients[sender].ControlledPlayerId;
            Player playerToMove = (Player)server.GameObjectsTable[playerId];

            if (playerToMove.Owner == sender)
            {
                float x = BitConverter.ToSingle(data,6);
                float y = BitConverter.ToSingle(data,10);

                playerToMove.SetMovingDirection(new System.Numerics.Vector2(x,y));
            }
            else
            {
                sender.Malus += 1;
            }

        }

        private Dictionary<byte, GameCommand> inputCommandsTable;
        private delegate void GameCommand(byte[] data, GameClient sender);

        public void GetPlayerInput(byte[] data,GameClient client)
        {

            //Console.WriteLine("taking input");
            if (GameStatus != GameState.Playing)
            {
                //Console.WriteLine("not playing");
                client.Malus++;
                return;
            }

            byte inputCommand = data[5];

            if (inputCommandsTable.ContainsKey(inputCommand))
            {
                inputCommandsTable[inputCommand](data, client);
            }
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

        public uint GetClientControlledPlayerId(GameClient client)
        {
            if (Clients.ContainsKey(client))
                return Clients[client].ControlledPlayerId;
            return 0;
        }

        public Packet GetGameInfoPacket()
        {
            return new Packet((int)PacketsCommands.GameInfo, false, Score[0], Score[1], (uint)GameStatus);
        }
    }
}

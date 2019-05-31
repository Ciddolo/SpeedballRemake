using System;
using System.Numerics;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer
{
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
        public Dictionary<GameClient, Team> Clients;
        private float startTimestamp;
        public Ball Ball { get; protected set; }

        public uint[] Score;

        private List<IUpdatable> updatableItems;
        private PhysicsHandler physicsHandler;
        public Team[] Teams { get; protected set; }

        public uint AddClient(GameClient client, out uint controlledPlayerId)
        {
            if (!Clients.ContainsKey(client))
            {
                Team teamToAssign = null;

                foreach (Team item in Teams)
                {
                    if (!item.HasOwner)
                    {
                        teamToAssign = item;
                        break;
                    }
                }

                if (teamToAssign == null)
                    throw new Exception("Third client should nover join and try to get a team.");

                teamToAssign.SetTeamOwner(client);

                Clients.Add(client, teamToAssign);

                if (Clients.Count >= server.MaxPlayers)
                {
                    startTimestamp = server.Now;
                    GameStatus = GameState.Playing;
                }

                controlledPlayerId = teamToAssign.ControlledPlayerId;

                return teamToAssign.TeamId;
            }

            controlledPlayerId = 0;
            return 0;
        }

        public void RemovePlayer(GameClient clientToRemove)
        {
            Clients[clientToRemove].Reset();
            Clients.Remove(clientToRemove);

            if (GameStatus == GameState.Playing)
                GameStatus = GameState.Ended;
        }

        private void SpawnTestingLevel()
        {
            Obstacle myObstacle = server.Spawn<Obstacle>(1, 10);
            physicsHandler.AddItem(myObstacle.RigidBody);
            myObstacle.SetPosition(0f, 3f);

            Player defPlayerTeamOne = server.Spawn<Player>(1, 1);
            defPlayerTeamOne.SetStartingPosition(-1f, 0f);
            defPlayerTeamOne.TeamId = 0;
            updatableItems.Add(defPlayerTeamOne);
            physicsHandler.AddItem(defPlayerTeamOne.RigidBody);

            Teams[0].AddPlayer(defPlayerTeamOne);
            Teams[0].DefaultControlledPlayerId = defPlayerTeamOne.Id;


            Player defAnotherPlayerTeamOne = server.Spawn<Player>(1, 1);
            defAnotherPlayerTeamOne.SetStartingPosition(3f, 0f);
            defAnotherPlayerTeamOne.TeamId = 0;
            updatableItems.Add(defAnotherPlayerTeamOne);
            physicsHandler.AddItem(defAnotherPlayerTeamOne.RigidBody);

            Teams[0].AddPlayer(defAnotherPlayerTeamOne);

            Player defPlayerTeamTwo = server.Spawn<Player>(1, 1);
            defPlayerTeamTwo.SetStartingPosition(1f, 0f);
            defPlayerTeamTwo.TeamId = 1;

            Teams[1].AddPlayer(defPlayerTeamTwo);
            Teams[1].DefaultControlledPlayerId = defPlayerTeamTwo.Id;
            physicsHandler.AddItem(defPlayerTeamTwo.RigidBody);

            updatableItems.Add(defPlayerTeamTwo);

            Ball = new Ball(this.server, 1, 1);
            updatableItems.Add(Ball);
            Ball.gameLogic = this;
            physicsHandler.AddItem(Ball.RigidBody);
            Ball.SetStartingPosition(30f, 30f);

            Net teamTwoNet = new Net(this.server, 4, 4);
            teamTwoNet.Position = new Vector2(20, 20);
            physicsHandler.AddItem(teamTwoNet.RigidBody);

            ResetPositions();
        }

        private void SpawnSerializedLevel(string serializedLevel)
        {
            Level levelData = JsonConvert.DeserializeObject<Level>(serializedLevel);

            PlayersInfo playerInfo = levelData.PlayerInfo;

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
                Teams[0].AddPlayer(player);

                updatableItems.Add(player);
                physicsHandler.AddItem(player.RigidBody);

                player.Name = levelData.TeamOneSpawnPositions[i].Name;

                if ((uint)playerInfo.DefaultPlayerIndex == i)
                    (Teams[0]).DefaultControlledPlayerId = player.Id;
            }

            for (int i = 0; i < levelData.TeamTwoSpawnPositions.Count; i++)
            {
                SimpleLevelObject data = levelData.TeamTwoSpawnPositions[i];
                Player player = server.Spawn<Player>(playerInfo.Height, playerInfo.Width);
                player.SetStartingPosition(data.Position);

                player.TeamId = 1;
                Teams[1].AddPlayer(player);

                updatableItems.Add(player);
                physicsHandler.AddItem(player.RigidBody);

                player.Name = levelData.TeamOneSpawnPositions[i].Name;

                if ((uint)playerInfo.DefaultPlayerIndex == i)
                    (Teams[1]).DefaultControlledPlayerId = player.Id;
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

            Ball = server.Spawn<Ball>(levelData.Ball.Height, levelData.Ball.Width);
            Ball.Name = levelData.Ball.Name;
            Ball.gameLogic = this;
            Ball.SetStartingPosition(levelData.Ball.Position);
            physicsHandler.AddItem(Ball.RigidBody);
            updatableItems.Add(Ball);
        }

        public void OnBallTaken(Player playerTakingBall)
        {
            Console.WriteLine("BALL POSSESSION CHANGED! NEW TEAM ID:[" + playerTakingBall.TeamId + "] NEW PLAYER ID:[" + playerTakingBall.Id + "]");
            uint playerTeamId = playerTakingBall.TeamId;
            Teams[playerTeamId].ControlledPlayer.SetMovingDirection(Vector2.Zero);
            Teams[playerTeamId].ControlledPlayerId = playerTakingBall.Id;
        }

        public void OnGoal(Net collidedNet)
        {
            int scoreToIncrement = ((int)collidedNet.TeamId + 1) % server.MaxPlayers;
            Score[scoreToIncrement]++;

            this.ResetPositions();
        }

        public void SpawnLevel(string serializedLevel = null)
        {
            if (serializedLevel == null)
                SpawnTestingLevel();
            else
                SpawnSerializedLevel(serializedLevel);

            Console.WriteLine("Loaded Level");

            Score = new uint[server.MaxPlayers];
        }

        public GameLogic(GameServer server)
        {
            this.server = server;
            Clients = new Dictionary<GameClient, Team>();
            GameStatus = GameState.WaitingForPlayers;
            Teams = new Team[server.MaxPlayers];
            Teams[0] = new Team(0);
            Teams[1] = new Team(1);

            //teamOneControllablePlayers = new List<Player>();
            //teamTwoControllablePlayers = new List<Player>();
            updatableItems = new List<IUpdatable>();
            physicsHandler = new PhysicsHandler();

            inputCommandsTable = new Dictionary<byte, GameCommand>();
            inputCommandsTable[(byte)InputType.SelectPlayer] = SelectPlayer;
            inputCommandsTable[(byte)InputType.Movement] = MovementDir;
            inputCommandsTable[(byte)InputType.Shot] = Shot;
            inputCommandsTable[(byte)InputType.Tackle] = Tackle;
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
                    Player playerToStop = (Player)server.GameObjectsTable[Clients[sender].ControlledPlayerId];
                    playerToStop.SetMovingDirection(new Vector2(0.0f, 0.0f));

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
                float x = BitConverter.ToSingle(data, 6);
                float y = BitConverter.ToSingle(data, 10);
                playerToMove.SetMovingDirection(new Vector2(x, y));
            }
            else
            {
                sender.Malus += 1;
            }
        }

        private void Shot(byte[] data, GameClient sender)
        {
            Console.WriteLine("SHOT");
            uint playerId = Clients[sender].ControlledPlayerId;
            Player currentPlayer = (Player)server.GameObjectsTable[playerId];

            if (currentPlayer.HasBall)
            {
                float directionX = BitConverter.ToSingle(data, 6);
                float directionY = BitConverter.ToSingle(data, 10);
                float force = BitConverter.ToSingle(data, 14);

                currentPlayer.ThrowBall(new Vector2(directionX, directionY), force);              
            }
            else
            {
                sender.Malus++;
            }
        }

        private void Tackle(byte[] data, GameClient sender)
        {
            Console.WriteLine("TACKLE");
            //Console.WriteLine("tackle input");
            uint playerId = Clients[sender].ControlledPlayerId;
            Player currentPlayer = (Player)server.GameObjectsTable[playerId];

            if (currentPlayer.HasBall || currentPlayer.State != PlayerState.Idle)
            {
                //Console.WriteLine("cant tackle if u have the ball or u are stunned/already tackling");
                return;
            }
            else
            {
                currentPlayer.StartTackling();
            }
        }

        private Dictionary<byte, GameCommand> inputCommandsTable;
        private delegate void GameCommand(byte[] data, GameClient sender);

        public void GetPlayerInput(byte[] data, GameClient client)
        {
            //Console.WriteLine("taking input");
            //if (GameStatus != GameState.Playing)
            //{
            //    //Console.WriteLine("not playing");
            //    client.Malus++;
            //    return;
            //}

            byte inputCommand = data[5];

            if (inputCommandsTable.ContainsKey(inputCommand))
            {
                inputCommandsTable[inputCommand](data, client);
            }
        }

        public void ClientUpdate(byte[] packetData, GameClient client)
        {
            if (GameStatus != GameState.Playing)
            {
                client.Malus++;
                return;
            }

            uint netId = BitConverter.ToUInt32(packetData, 5);
            GameObject gameObject = server.GameObjectsTable[netId];

            if (gameObject.IsOwnedBy(client) && Clients[client].ControlledPlayerId == netId)
            {
                Player playerToMove = (Player)gameObject;

                float posX, posY;

                posX = BitConverter.ToSingle(packetData, 9);
                posY = BitConverter.ToSingle(packetData, 13);

                playerToMove.SetPosition(posX, posY);
            }
            else
            {
                client.Malus += 10;
            }
        }

        public void Update()
        {
            physicsHandler.Update(server.UpdateFrequency);
            physicsHandler.CheckCollisions();

            foreach (IUpdatable item in updatableItems)
            {
                item.Update(server.UpdateFrequency);
                server.SendToAllClients(item.GetUpdatePacket());
            }

            server.SendToAllClients(this.GetGameInfoPacket());
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
            uint controlledPlayerIdTeamOne = Teams[0].ControlledPlayerId;
            uint controlledPlayerIdTeamTwo = Teams[1].ControlledPlayerId;

            return new Packet(PacketsCommands.GameInfo, false, Score[0], Score[1],
                controlledPlayerIdTeamOne, controlledPlayerIdTeamTwo, (uint)GameStatus);
        }
    }
}

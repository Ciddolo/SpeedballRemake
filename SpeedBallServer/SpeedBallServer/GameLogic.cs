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
        public PhysicsHandler PhysicsHandler { get; protected set; }
        public Timer GameTime;
        private float maxPlayers;

        public uint[] Score;

        private List<IUpdatable> updatableItems;

        public Team[] Teams { get; protected set; }

        public uint? AddClient(GameClient client, out uint controlledPlayerId)
        {
            controlledPlayerId = 0;

            if (Clients.Count >= maxPlayers)
            {
                return null;
            }

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

                if (Clients.Count >= maxPlayers)
                {
                    startTimestamp = server.Now;
                    GameStatus = GameState.Playing;
                }

                controlledPlayerId = teamToAssign.ControlledPlayerId;

                return teamToAssign.TeamId;
            }

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
            PhysicsHandler.AddItem(myObstacle.RigidBody);
            myObstacle.SetPosition(0f, 3f);

            Player defPlayerTeamOne = server.Spawn<Player>(1, 1);
            defPlayerTeamOne.SetStartingPosition(-1f, 0f);
            defPlayerTeamOne.TeamId = 0;
            updatableItems.Add(defPlayerTeamOne);
            PhysicsHandler.AddItem(defPlayerTeamOne.RigidBody);

            Teams[0].AddPlayer(defPlayerTeamOne);
            Teams[0].DefaultControlledPlayerId = defPlayerTeamOne.Id;


            Player defAnotherPlayerTeamOne = server.Spawn<Player>(1, 1);
            defAnotherPlayerTeamOne.SetStartingPosition(3f, 0f);
            defAnotherPlayerTeamOne.TeamId = 0;
            updatableItems.Add(defAnotherPlayerTeamOne);
            PhysicsHandler.AddItem(defAnotherPlayerTeamOne.RigidBody);

            Teams[0].AddPlayer(defAnotherPlayerTeamOne);

            Player defPlayerTeamTwo = server.Spawn<Player>(1, 1);
            defPlayerTeamTwo.SetStartingPosition(1f, 0f);
            defPlayerTeamTwo.TeamId = 1;

            Teams[1].AddPlayer(defPlayerTeamTwo);
            Teams[1].DefaultControlledPlayerId = defPlayerTeamTwo.Id;
            PhysicsHandler.AddItem(defPlayerTeamTwo.RigidBody);

            updatableItems.Add(defPlayerTeamTwo);

            Ball = new Ball(this.server, 1, 1);
            updatableItems.Add(Ball);
            Ball.gameLogic = this;
            PhysicsHandler.AddItem(Ball.RigidBody);
            Ball.SetStartingPosition(30f, 30f);

            Net teamTwoNet = new Net(this.server, 4, 4);
            teamTwoNet.Position = new Vector2(20, 20);
            PhysicsHandler.AddItem(teamTwoNet.RigidBody);

            ResetPositions();
        }

        private void SpawnSerializedLevel(string serializedLevel)
        {
            Level levelData = JsonConvert.DeserializeObject<Level>(serializedLevel);

            PlayersInfo playerInfo = levelData.PlayerInfo;

            Ball = server.Spawn<Ball>(levelData.Ball.Height, levelData.Ball.Width);
            Ball.Name = levelData.Ball.Name;
            Ball.gameLogic = this;
            Ball.SetStartingPosition(levelData.Ball.Position);
            PhysicsHandler.AddItem(Ball.RigidBody);
            updatableItems.Add(Ball);

            foreach (var obstacleInfo in levelData.Walls)
            {
                Obstacle myObstacle = server.Spawn<Obstacle>(obstacleInfo.Height, obstacleInfo.Width);
                PhysicsHandler.AddItem(myObstacle.RigidBody);
                myObstacle.SetPosition(obstacleInfo.Position);

                myObstacle.Name = obstacleInfo.Name;
            }

            foreach (var bumperInfo in levelData.Bumpers)
            {
                Bumper myBumper = server.Spawn<Bumper>(bumperInfo.Height, bumperInfo.Width);
                PhysicsHandler.AddItem(myBumper.RigidBody);
                myBumper.SetPosition(bumperInfo.Position);

                myBumper.Name = bumperInfo.Name;
            }

            Warp warpRight = server.Spawn<Warp>(levelData.WarpRight.Height, levelData.WarpRight.Width);
            PhysicsHandler.AddItem(warpRight.RigidBody);
            warpRight.SetPosition(levelData.WarpRight.Position);
            warpRight.Name = levelData.WarpRight.Name;

            Warp warpLeft = server.Spawn<Warp>(levelData.WarpLeft.Height, levelData.WarpLeft.Width);
            PhysicsHandler.AddItem(warpLeft.RigidBody);
            warpLeft.SetPosition(levelData.WarpLeft.Position);
            warpLeft.Name = levelData.WarpLeft.Name;

            warpRight.ConnectedWarp = warpLeft;
            warpLeft.ConnectedWarp = warpRight;

            for (int i = 0; i < levelData.TeamOneSpawnPositions.Count; i++)
            {
                SimpleLevelObject data = levelData.TeamOneSpawnPositions[i];

                if (i == (uint)playerInfo.GoalkeeperIndex)
                {
                    Goalkeeper goalkeeper = server.Spawn<Goalkeeper>(playerInfo.Height, playerInfo.Width);
                    goalkeeper.SetStartingPosition(data.Position);

                    goalkeeper.TeamId = 0;

                    updatableItems.Add(goalkeeper);
                    PhysicsHandler.AddItem(goalkeeper.RigidBody);

                    goalkeeper.Name = levelData.TeamOneSpawnPositions[i].Name;
                    goalkeeper.Ball = Ball;
                    Teams[0].Goalkeeper = goalkeeper;
                }
                else
                {
                    Player player = server.Spawn<Player>(playerInfo.Height, playerInfo.Width);
                    player.SetStartingPosition(data.Position);

                    player.TeamId = 0;
                    Teams[0].AddPlayer(player);

                    updatableItems.Add(player);
                    PhysicsHandler.AddItem(player.RigidBody);

                    player.Name = levelData.TeamOneSpawnPositions[i].Name;

                    if ((uint)playerInfo.DefaultPlayerIndex == i)
                        (Teams[0]).DefaultControlledPlayerId = player.Id;
                }
            }

            for (int i = 0; i < levelData.TeamTwoSpawnPositions.Count; i++)
            {
                SimpleLevelObject data = levelData.TeamTwoSpawnPositions[i];

                if (i == (uint)playerInfo.GoalkeeperIndex)
                {
                    Goalkeeper goalkeeper = server.Spawn<Goalkeeper>(playerInfo.Height, playerInfo.Width);
                    goalkeeper.SetStartingPosition(data.Position);

                    goalkeeper.TeamId = 1;

                    updatableItems.Add(goalkeeper);
                    PhysicsHandler.AddItem(goalkeeper.RigidBody);

                    goalkeeper.Name = levelData.TeamOneSpawnPositions[i].Name;
                    goalkeeper.Ball = Ball;
                    Teams[1].Goalkeeper = goalkeeper;
                }
                else
                {
                    Player player = server.Spawn<Player>(playerInfo.Height, playerInfo.Width);
                    player.SetStartingPosition(data.Position);

                    player.TeamId = 1;
                    Teams[1].AddPlayer(player);

                    updatableItems.Add(player);
                    PhysicsHandler.AddItem(player.RigidBody);

                    player.Name = levelData.TeamOneSpawnPositions[i].Name;

                    if ((uint)playerInfo.DefaultPlayerIndex == i)
                        (Teams[1]).DefaultControlledPlayerId = player.Id;
                }
            }

            Net TeamOneNet = server.Spawn<Net>(levelData.NetTeamOne.Height, levelData.NetTeamOne.Width);
            TeamOneNet.Name = levelData.NetTeamOne.Name;
            TeamOneNet.TeamId = 0;
            TeamOneNet.Position = levelData.NetTeamOne.Position;
            PhysicsHandler.AddItem(TeamOneNet.RigidBody);

            Net TeamTwoNet = server.Spawn<Net>(levelData.NetTeamTwo.Height, levelData.NetTeamTwo.Width);
            TeamTwoNet.Name = levelData.NetTeamTwo.Name;
            TeamTwoNet.TeamId = 1;
            TeamTwoNet.Position = levelData.NetTeamTwo.Position;
            PhysicsHandler.AddItem(TeamTwoNet.RigidBody);
        }

        public void OnBallTaken(GameObject playerTakingBall)
        {
            uint playerTeamId;
            if (playerTakingBall is Player)
            {
                Console.WriteLine("BALL POSSESSION CHANGED! NEW TEAM ID:[" + ((Player)playerTakingBall).TeamId + "] NEW PLAYER ID:[" + playerTakingBall.Id + "]");
                playerTeamId = ((Player)playerTakingBall).TeamId;
                Teams[playerTeamId].ControlledPlayer.SetMovingDirection(Vector2.Zero);
                Teams[playerTeamId].ControlledPlayerId = playerTakingBall.Id;
            }
            else
            {
                Console.WriteLine("BALL POSSESSION CHANGED! NEW TEAM ID:[" + ((Goalkeeper)playerTakingBall).TeamId + "] NEW PLAYER ID:[" + playerTakingBall.Id + "]");
                playerTeamId = ((Goalkeeper)playerTakingBall).TeamId;
            }
        }

        public void OnGoal(Net collidedNet)
        {
            int scoreToIncrement = ((int)collidedNet.TeamId + 1) % 2;
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

            Score = new uint[2];
        }

        public GameLogic(GameServer server,string level=null)
        {
            this.server = server;
            maxPlayers = 2;
            Clients = new Dictionary<GameClient, Team>();
            GameStatus = GameState.WaitingForPlayers;
            Teams = new Team[2];
            Teams[0] = new Team(0);
            Teams[1] = new Team(1);

            GameTime = new Timer(200.0f, SetGameStatusToEnded);

            updatableItems = new List<IUpdatable>();
            PhysicsHandler = new PhysicsHandler();

            SpawnLevel(level);

            inputCommandsTable = new Dictionary<byte, GameCommand>();
            inputCommandsTable[(byte)InputType.SelectPlayer] = SelectPlayer;
            inputCommandsTable[(byte)InputType.Movement] = MovementDir;
            inputCommandsTable[(byte)InputType.Shot] = Shot;
            inputCommandsTable[(byte)InputType.Tackle] = Tackle;
        }

        private void SetGameStatusToEnded()
        {
            GameStatus = GameState.Ended;
        }

        private void SelectPlayer(byte[] data, GameClient sender)
        {
            if (Clients[sender].ControlledPlayer.HasBall)
            {
                sender.Malus++;
                return;
            }

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
            if (GameStatus == GameState.Playing && !GameTime.IsStarted)
                GameTime.Start();

            if (GameStatus == GameState.Playing && GameTime.IsStarted)
                GameTime.Update(server.UpdateFrequency);

            PhysicsHandler.Update(server.UpdateFrequency);
            PhysicsHandler.CheckCollisions();

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

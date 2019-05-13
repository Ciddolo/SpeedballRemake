using System;
using System.Net;
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
        public Dictionary<GameClient, ClientInfo> clients;
        private float startTimestamp;

        public uint[] Score;

        private List<IUpdatable> UpdatableItems;
        private PhysicsHandler physicsHandler;
        private List<Player>[] Teams;
        private List<Player> TeamOneControllablePlayers;
        private List<Player> TeamTwoControllablePlayers;

        private uint defaultPlayerTeamOneId,defaultPlayerTeamTwoId;

        public uint AddClient(GameClient client,out uint controlledPlayerId)
        {
            ClientInfo newClientInfo = new ClientInfo();

            newClientInfo.TeamId = 0;
            newClientInfo.ControlledPlayerId = defaultPlayerTeamOneId;

            if (!clients.ContainsKey(client))
            {
                if (clients.Count == 1 )
                {
                    if(clients.First().Value.TeamId==0)
                    {
                        newClientInfo.TeamId = 1;
                        newClientInfo.ControlledPlayerId = defaultPlayerTeamTwoId;
                    }

                    GameStatus = GameState.Playing;
                    startTimestamp = server.Now;
                }

                clients.Add(client,newClientInfo);
            }

            foreach (Player player in Teams[newClientInfo.TeamId])
            {
                player.SetOwner(client);
            }

            if (clients.Count >= server.MaxPlayers)
            {
                GameStatus = GameState.Playing;
            }

            controlledPlayerId = newClientInfo.ControlledPlayerId;
            return newClientInfo.TeamId;
        }

        public void RemovePlayer(GameClient clientToRemove)
        {
            foreach (Player player in Teams[clients[clientToRemove].TeamId])
            {
                player.SetOwner(null);
            }

            clients.Remove(clientToRemove);
            if(GameStatus==GameState.Playing)
                GameStatus = GameState.Ended;
        }

        private void SpawnTestingLevel()
        {
            Obstacle myObstacle = server.Spawn<Obstacle>(1, 10);
            physicsHandler.AddItem(myObstacle.RigidBody);
            myObstacle.SetPosition(0f, 3f);

            Player defPlayerTeamOne = server.Spawn<Player>();
            defaultPlayerTeamOneId = defPlayerTeamOne.Id;
            defPlayerTeamOne.SetStartingPosition(-1f, 0f);
            defPlayerTeamOne.TeamId = 0;
            TeamOneControllablePlayers.Add(defPlayerTeamOne);
            UpdatableItems.Add(defPlayerTeamOne);
            physicsHandler.AddItem(defPlayerTeamOne.RigidBody);

            Player defAnotherPlayerTeamOne = server.Spawn<Player>();
            defAnotherPlayerTeamOne.SetStartingPosition(3f, 0f);
            defAnotherPlayerTeamOne.TeamId = 0;
            UpdatableItems.Add(defAnotherPlayerTeamOne);
            physicsHandler.AddItem(defAnotherPlayerTeamOne.RigidBody);

            TeamOneControllablePlayers.Add(defAnotherPlayerTeamOne);

            Player defPlayerTeamTwo = server.Spawn<Player>();
            defPlayerTeamTwo.SetStartingPosition(1f, 0f);
            defaultPlayerTeamTwoId = defPlayerTeamTwo.Id;
            defPlayerTeamTwo.TeamId = 1;
            TeamTwoControllablePlayers.Add(defPlayerTeamTwo);
            physicsHandler.AddItem(defPlayerTeamTwo.RigidBody);
            UpdatableItems.Add(defPlayerTeamTwo);

            ResetPositions();
        }

        public void SpawnLevel(string serializedLevel=null)
        {
            if (serializedLevel == null)
                SpawnTestingLevel();
            else
                throw new NotImplementedException();

            Console.WriteLine("Loaded Level");
            Teams[0] = TeamOneControllablePlayers;
            Teams[1] = TeamTwoControllablePlayers;

            Score = new uint[server.MaxPlayers];


        }

        public GameLogic(GameServer server)
        {
            this.server = server;
            clients = new Dictionary<GameClient, ClientInfo>();
            GameStatus = GameState.WaitingForPlayers;
            Teams = new List<Player>[server.MaxPlayers];
            TeamOneControllablePlayers = new List<Player>();
            TeamTwoControllablePlayers = new List<Player>();
            UpdatableItems = new List<IUpdatable>();
            physicsHandler = new PhysicsHandler();
        }

        public void ClientUpdate(byte[] packetData,GameClient client)
        {
            if (GameStatus != GameState.Playing)
            {
                client.Malus++;
                return;
            }

            uint netId = BitConverter.ToUInt32(packetData, 6);
            GameObject gameObject = server.GameObjectsTable[netId];

            if (gameObject.IsOwnedBy(client) && clients[client].ControlledPlayerId==netId)
            {
                Player playerToMove = (Player)gameObject;

                float posX, posY, dirX, dirY;

                posX = BitConverter.ToSingle(packetData, 10);
                posY = BitConverter.ToSingle(packetData, 14);
                dirX = BitConverter.ToSingle(packetData, 18);
                dirY = BitConverter.ToSingle(packetData, 22);

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
            foreach (IUpdatable item in UpdatableItems)
            {
               item.Update();
            }
            physicsHandler.CheckCollisions();
        }

        public void ResetPositions()
        {
            foreach (IUpdatable item in UpdatableItems)
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

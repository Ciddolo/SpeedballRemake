using System;
using System.Net;
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
        public uint Score;
    }

    public enum GameState
    {
        WaitingForPlayers,
        Playing,
        Ended
    }

    public class GameLogic
    {
        private GameServer server;
        public GameState GameStatus;
        public Dictionary<GameClient, ClientInfo> clients;
        private float startTimestamp;

        private List<Player>[] Teams;

        private List<Player> TeamOnePlayers;
        private List<Player> TeamTwoPlayers;

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
            myObstacle.SetPosition(0, 3);

            Player defPlayerTeamOne = server.Spawn<Player>();
            defaultPlayerTeamOneId = defPlayerTeamOne.Id;
            defPlayerTeamOne.SetStartingPosition(-1, 0);
            defPlayerTeamOne.TeamId = 0;
            TeamOnePlayers.Add(defPlayerTeamOne);

            Player defAnotherPlayerTeamOne = server.Spawn<Player>();
            defAnotherPlayerTeamOne.SetStartingPosition(3, 0);
            defAnotherPlayerTeamOne.TeamId = 0;

            TeamOnePlayers.Add(defAnotherPlayerTeamOne);

            Player defPlayerTeamTwo = server.Spawn<Player>();
            defPlayerTeamTwo.SetStartingPosition(1, 0);
            defaultPlayerTeamTwoId = defPlayerTeamTwo.Id;
            defPlayerTeamTwo.TeamId = 1;
            TeamTwoPlayers.Add(defPlayerTeamTwo);
        }

        public void SpawnLevel(string serializedLevel=null)
        {
            //to do
            //tell the server to spawn objects of the level from serialized level

            if (serializedLevel == null)
                SpawnTestingLevel();
            else
                throw new NotImplementedException();

            Console.WriteLine("Loaded Level");
            Teams[0] = TeamOnePlayers;
            Teams[1] = TeamTwoPlayers;
        }

        public GameLogic(GameServer server)
        {
            this.server = server;
            clients = new Dictionary<GameClient, ClientInfo>();
            GameStatus = GameState.WaitingForPlayers;
            Teams = new List<Player>[server.MaxPlayers];
            TeamOnePlayers = new List<Player>();
            TeamTwoPlayers = new List<Player>();
        }
    }
}

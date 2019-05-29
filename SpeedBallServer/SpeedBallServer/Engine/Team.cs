using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer
{
    public class Team
    {
        public uint TeamId { get; private set; }
        public uint ControlledPlayerId;
        public Player ControlledPlayer
        {
            get
            {
                return ControllablePlayers[ControlledPlayerId];
            }
        }
        private uint defaultControlledPlayerId;
        public uint DefaultControlledPlayerId
        {
            get
            {
                return defaultControlledPlayerId;
            }
            set
            {
                defaultControlledPlayerId = value;
                ControlledPlayerId = value;
            }
        }
        public GameClient Owner { get; private set; }
        public bool HasOwner { get { return Owner != null; } }

        public Dictionary<uint,Player> ControllablePlayers;
        public Goalkeeper Goalkeeper;

        public Team(uint teamId)
        {
            TeamId = teamId;
            ControllablePlayers = new Dictionary<uint, Player>();
        }

        public void AddPlayer(Player newPlayer)
        {
            ControllablePlayers.Add(newPlayer.Id,newPlayer);
        }

        public void SetTeamOwner(GameClient newOnwer)
        {
            this.Owner = newOnwer;
            foreach (Player player in ControllablePlayers.Values)
            {
                player.SetOwner(newOnwer);
            }
        }

        public void Reset()
        {
            this.ControlledPlayerId = defaultControlledPlayerId;
            this.SetTeamOwner(null);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer
{
    public class Player : GameObject
    {
        public Player(GameServer server)
            : base((int)InternalObjectsId.Player, server, 1, 1)
        {

        }
    }
}

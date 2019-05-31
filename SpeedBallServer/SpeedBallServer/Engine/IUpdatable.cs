using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer
{
    public interface IUpdatable
    {
        Packet GetUpdatePacket();
        void Update(float deltaTime);
        void Reset();
    }
}

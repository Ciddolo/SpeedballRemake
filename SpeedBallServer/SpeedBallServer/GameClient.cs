using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer
{
    public class GameClient
    {
        public uint Malus;
        public float LastPacketTimestamp;

        private Queue<Packet> sendQueue;
        private GameServer server;

        private EndPoint endPoint;

        public EndPoint EndPoint
        {
            get { return endPoint; }
        }

        public GameClient(GameServer server, EndPoint endPoint)
        {
            this.server = server;
            this.endPoint = endPoint;
            sendQueue = new Queue<Packet>();
            Malus = 0;
        }

        public void Process()
        {
            int packetsInQueue = sendQueue.Count;
            for (int i = 0; i < packetsInQueue; i++)
            {
                Packet packet = sendQueue.Dequeue();
                server.Send(packet, endPoint);
            }
        }

        public void Enqueue(Packet packet)
        {
            sendQueue.Enqueue(packet);
        }

        public override string ToString()
        {
            return endPoint.ToString();
        }
    }
}

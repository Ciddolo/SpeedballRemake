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
        private Dictionary<uint, Packet> ackTable;

        public EndPoint EndPoint
        {
            get { return endPoint; }
        }

        public List<uint> AcksNedeedIds()
        {
            List<uint> acksIds = new List<uint>();
            foreach (KeyValuePair<uint,Packet> packet in ackTable)
            {
                acksIds.Add(packet.Key);
            }
            return acksIds;
        }

        public GameClient(GameServer server, EndPoint endPoint)
        {
            this.server = server;
            this.endPoint = endPoint;
            sendQueue = new Queue<Packet>();
            ackTable = new Dictionary<uint, Packet>();
            Malus = 0;
        }

        public void Process()
        {
            int packetsInQueue = sendQueue.Count;
            for (int i = 0; i < packetsInQueue; i++)
            {
                Packet packet = sendQueue.Dequeue();
                if (server.Now >= packet.SendAfter)
                {
                    packet.IncreaseAttempts();
                    if (server.Send(packet, endPoint))
                    {
                        // when the packet gets sendend to the endpoint
                        if (packet.NeedAck)
                        {
                            ackTable[packet.Id] = packet;
                        }
                    }
                    // on error, retry sending only if NOT OneShot
                    else if (!packet.OneShot)
                    {
                        if (packet.Attempts < server.MaxAttemptsForPacket)
                        {
                            // retry sending after sendingaftertimeforpackts
                            packet.SendAfter = server.Now + server.SendAfterTimeForPackets;
                            sendQueue.Enqueue(packet);
                        }
                    }
                }
                else
                {
                    // it is too early, re-enqueue the packet
                    sendQueue.Enqueue(packet);
                }
            }

            // check ack table
            List<uint> deadPackets = new List<uint>();
            foreach (uint id in ackTable.Keys)
            {
                Packet packet = ackTable[id];
                if (packet.IsExpired(server.Now))
                {
                    if (packet.Attempts < server.MaxAttemptsForPacket)
                    {
                        sendQueue.Enqueue(packet);
                    }
                    else
                    {
                        deadPackets.Add(id);
                    }
                }
            }

            foreach (uint id in deadPackets)
            {
                ackTable.Remove(id);
            }
        }

        public void Enqueue(Packet packet)
        {
            sendQueue.Enqueue(packet);
        }

        public void Ack(uint packetId)
        {
            if (ackTable.ContainsKey(packetId))
            {
                ackTable.Remove(packetId);
            }
            else
            {
                //increase malus
                Malus += 5;
            }
        }

        public override string ToString()
        {
            return endPoint.ToString();
        }
    }
}

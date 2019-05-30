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

        private Queue<Packet> sendQueue;
        private GameServer server;

        private EndPoint endPoint;
        private Dictionary<uint, Packet> ackTable;

        private Timer pingTimer;
        private Packet pingPacketSended;
        private bool waitingForPong;
        private uint pingPacketId;
        private float sendedPingTimestamp;
        public float LastPingValue;

        public float PingAverage { get; private set;}
        private Queue<float> lastTenPingValues;
        private float pingTotal;
        private int pingCount;
        private int maxPingCount;

        private void PingReceived(float newPing)
        {
            lastTenPingValues.Enqueue(newPing);
            pingTotal += newPing;
            pingCount++;
            if(lastTenPingValues .Count> maxPingCount)
            {
                float pingToRemove = lastTenPingValues.Dequeue();
                pingTotal -= pingToRemove;
                pingCount--;
            }
            //Console.WriteLine("PING AVERAGE:[" + PingAverage + "] ADD:[" + newPing + "] COUNT:[" + pingCount + "]");
            PingAverage = pingTotal / pingCount;
        }

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
            pingTimer = new Timer(1,AttemptPing,true);
            LastPingValue = -1;
            lastTenPingValues = new Queue<float>();
            maxPingCount = 10;
            pingTimer.Start();
        }

        private void AttemptPing()
        {
            if(waitingForPong)
            {
                Malus += 1000;
                sendedPingTimestamp = server.Now;
                server.Send(pingPacketSended, this.endPoint);
            }
            else
            {
                //Console.WriteLine("sending ping");
                Ping();
            }
        }

        public void Process(float deltaTime)
        {
            pingTimer.Update(deltaTime);

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
                //Console.WriteLine("client not responding ack for packet " + id);
            }

            //Console.WriteLine(PingValue);
            //Console.WriteLine(Malus);

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
        }

        public void Enqueue(Packet packet)
        {
            sendQueue.Enqueue(packet);
        }

        public void Ack(uint packetId)
        {
            if (ackTable.ContainsKey(packetId))
            {
                //Console.WriteLine("received ack of packet "+ packetId);
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

        public void Ping()
        {
            waitingForPong = true;
            Packet pingPacket = new Packet((byte)PacketsCommands.Ping);
            this.pingPacketId = pingPacket.Id;
            sendedPingTimestamp = server.Now;
            server.Send(pingPacket, this.endPoint);
            pingPacketSended = pingPacket;
        }

        public void Pong(uint packetId)
        {
            //Console.WriteLine("received pong "+packetId+" expected "+pingPacketId);
            if (pingPacketId==packetId)
            {
                LastPingValue = server.Now - sendedPingTimestamp;
                PingReceived(LastPingValue);
                pingPacketId = 0;
                waitingForPong = false;
            }
            else
            {
                //increase malus
                Malus += 50;
            }
        }


    }
}

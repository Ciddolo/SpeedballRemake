﻿using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer.Test
{
    public class PinPongTests
    {
        private GameServer server;
        private FakeClock clock;
        private FakeTransport transport;
        private FakeEndPoint FirstClient;

        [SetUp]
        public void SetUpTest()
        {
            //initializing objects needed for the server
            transport = new FakeTransport();
            clock = new FakeClock();
            transport.Bind("127.0.0.1", 5000);

            //initializing server
            server = new GameServer(transport, clock,2);

            //initializinng client
            FirstClient = new FakeEndPoint("192.168.1.1", 5001);

            //initializing join packet
            FakeData packet = new FakeData();
            packet.data = new Packet((byte)PacketsCommands.Join).GetData();
            packet.endPoint = FirstClient;

            //sending packet
            transport.ClientEnqueue(packet);

            //server reads join
            server.SingleStep();

        }

        [Test]
        public void ReceivePingPacket()
        {
            byte[] pingPacket = (transport.ClientDequeue()).data;

            Assert.That(pingPacket[0], Is.EqualTo((byte)PacketsCommands.Ping));
        }

        [Test]
        public void DefaultClientPing()
        {
            Assert.That(server.GetClientPing(FirstClient),Is.EqualTo(-1f));
        }

        [Test]
        public void SendPongPacket()
        {
            byte[] pingPacket = (transport.ClientDequeue()).data;

            uint pingPacketId = BitConverter.ToUInt32(pingPacket,1);

            FakeData pongPacket=new FakeData();
            pongPacket.endPoint = FirstClient;
            pongPacket.data = (new Packet((byte)PacketsCommands.Pong,false,pingPacketId)).GetData();

            clock.IncreaseTimeStamp(.5f);
            transport.ClientEnqueue(pongPacket);
            clock.IncreaseTimeStamp(.4f);
            server.SingleStep();

            Assert.That(server.GetClientPing(FirstClient), Is.EqualTo(.9f));
        }

        [Test]
        public void CheckPingPacketAfterPong()
        {
            byte[] pingPacket = (transport.ClientDequeue()).data;

            uint pingPacketId = BitConverter.ToUInt32(pingPacket, 1);

            FakeData pongPacket = new FakeData();
            pongPacket.endPoint = FirstClient;
            pongPacket.data = (new Packet((byte)PacketsCommands.Pong, false, pingPacketId)).GetData();

            transport.ClientEnqueue(pongPacket);
            clock.IncreaseTimeStamp(.5f);
            server.SingleStep();
            clock.IncreaseTimeStamp(1.5f);
            server.SingleStep();

            //dequeueing welcome
            transport.ClientDequeue();

            //dequeueing obstacle spawn packet
            transport.ClientDequeue();

            //dequeueing first player spawn
            transport.ClientDequeue();

            //dequeueing second player spawn
            transport.ClientDequeue();

            //dequeueing first player spawn of the second team
            transport.ClientDequeue();

            pingPacket = (transport.ClientDequeue()).data;

            Assert.That(pingPacket[0], Is.EqualTo((byte)PacketsCommands.Ping));

        }

        [Test]
        public void ClientSendingPingPacket()
        {
            FakeData pingPacket = new FakeData();
            pingPacket.endPoint = FirstClient;
            pingPacket.data = (new Packet((byte)PacketsCommands.Ping)).GetData();

            transport.ClientEnqueue(pingPacket);

            server.SingleStep();

            //dequeue ping packet
            transport.ClientDequeue();

            byte[] pongPacket = (transport.ClientDequeue()).data;

            Assert.That(pongPacket[0], Is.EqualTo((byte)PacketsCommands.Pong));
        }

        [Test]
        public void ClientSendingPingPacketId()
        {
            FakeData pingPacket = new FakeData();
            pingPacket.endPoint = FirstClient;
            pingPacket.data = (new Packet((byte)PacketsCommands.Ping)).GetData();
            uint pingPacketId = BitConverter.ToUInt32(pingPacket.data, 1);

            transport.ClientEnqueue(pingPacket);

            server.SingleStep();

            //dequeue ping packet
            transport.ClientDequeue();

            byte[] pongPacket = (transport.ClientDequeue()).data;
            uint pongResponseId = BitConverter.ToUInt32(pingPacket.data, 1);

            Assert.That(pingPacketId, Is.EqualTo(pongResponseId));
        }
    }
}

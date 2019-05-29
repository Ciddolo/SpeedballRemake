﻿using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer.Test.ServerTests
{
    class AckTests
    {
        private GameServer server;
        private FakeClock clock;
        private FakeTransport transport;
        private FakeEndPoint firstClient;

        [SetUp]
        public void SetUpTest()
        {
            transport = new FakeTransport();
            clock = new FakeClock();
            server = new GameServer(transport, clock);

            transport.Bind("127.0.0.1", 5000);

            firstClient = new FakeEndPoint("192.168.1.1", 5001);

            FakeData packet = new FakeData();
            packet.data = new Packet(PacketsCommands.Join).GetData();
            packet.endPoint = firstClient;

            transport.ClientEnqueue(packet);
            server.SingleStep();

            //dequeue ping packet
            transport.ClientDequeue();

            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            //dequeue ping packet
            transport.ClientDequeue();
            //update packets
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
        }

        [Test]
        public void SendAckAttempts()
        {
            transport.ClientDequeue();

            //gameinfo
            transport.ClientDequeue();

            //on join received 5 packets that needs ack
            //every increaseTimestamp/singleStep the client receive the copy of the 5 packets
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            //dequeue ping packet
            transport.ClientDequeue();
            //update packets
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();

            //gameinfo
            transport.ClientDequeue();

            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            //dequeue ping packet
            transport.ClientDequeue();
            //update packets
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();

            //gameinfo
            transport.ClientDequeue();

            //on this increaseTimestamp/singleStep the client will not receive the copy of the 5 packets, out of attempts
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            //dequeue ping packet
            transport.ClientDequeue();
            //update packets
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();

            //gameinfo
            transport.ClientDequeue();

            Assert.That(transport.GetSendQueueCount(), Is.EqualTo(15));
        }

        [Test]
        public void AckNeeded()
        {
            //dequeue welcome packet
            byte[] welcomePacket = transport.ClientDequeue().data;
            uint packetId = BitConverter.ToUInt32(welcomePacket, 1);

            Assert.That(server.AcksIdsNeededFrom(firstClient).Contains(packetId), Is.EqualTo(true));
        }

        [Test]
        public void SuccessfullAck()
        {
            byte[] welcomePacket = transport.ClientDequeue().data;
            uint packetId = BitConverter.ToUInt32(welcomePacket, 1);

            FakeData packet = new FakeData();
            packet.endPoint = firstClient;
            packet.data = new Packet(PacketsCommands.Ack, false, packetId).GetData();

            transport.ClientEnqueue(packet);

            server.SingleStep();

            Assert.That(server.AcksIdsNeededFrom(firstClient).Contains(packetId), Is.EqualTo(false));
        }

        [Test]
        public void CheckSuccessfullAckAfterSingleStep()
        {
            byte[] welcomePacket = transport.ClientDequeue().data;
            uint packetId = BitConverter.ToUInt32(welcomePacket, 1);

            FakeData packet = new FakeData();
            packet.endPoint = firstClient;
            packet.data = new Packet(PacketsCommands.Ack, false, packetId).GetData();

            transport.ClientEnqueue(packet);
            server.SingleStep();

            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            Assert.That(server.AcksIdsNeededFrom(firstClient).Contains(packetId), Is.EqualTo(false));
        }

        [Test]
        public void UnsuccessfullAck()
        {
            byte[] welcomePacket = transport.ClientDequeue().data;
            uint packetId = BitConverter.ToUInt32(welcomePacket, 1);

            FakeData packet = new FakeData();
            packet.endPoint = firstClient;
            packet.data = new Packet(PacketsCommands.Ack, false, 0).GetData();

            transport.ClientEnqueue(packet);

            server.SingleStep();

            Assert.That(server.AcksIdsNeededFrom(firstClient).Contains(packetId), Is.EqualTo(true));
        }
    }
}

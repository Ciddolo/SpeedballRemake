using System;
using NUnit.Framework;

namespace SpeedBallServer.Test.ServerTests
{
    class WelcomeTests
    {
        private GameServer server;
        private FakeClock clock;
        private FakeTransport transport;
        private FakeEndPoint firstClient, secondClient;

        [SetUp]
        public void SetUpTest()
        {
            transport = new FakeTransport();
            clock = new FakeClock();
            server = new GameServer(transport, clock);

            transport.Bind("127.0.0.1", 5000);

            firstClient = new FakeEndPoint("192.168.1.1", 5001);
            secondClient = new FakeEndPoint("192.168.1.2", 5002);

            FakeData packet = new FakeData();
            packet.data = new Packet(PacketsCommands.Join).GetData();
            packet.endPoint = firstClient;

            transport.ClientEnqueue(packet);
            server.SingleStep();

            //dequeue ping packet
            transport.ClientDequeue();
        }

        [Test]
        public void SuccessfullJoinResponse()
        {
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();
            //dequeue ping packet
            transport.ClientDequeue();

            byte[] welcomePacketData = transport.ClientDequeue().data;

            Assert.That(welcomePacketData[0], Is.EqualTo(1));
        }

        [Test]
        public void FirstClientIdFromWelcome()
        {
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();
            //dequeue ping packet
            transport.ClientDequeue();

            byte[] welcomePacketData = transport.ClientDequeue().data;

            uint clientId = BitConverter.ToUInt32(welcomePacketData, 5);

            Assert.That(clientId, Is.EqualTo(0));
        }

        [Test]
        public void SecondClientIdFromWelcome()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet(PacketsCommands.Join).GetData();
            packet.endPoint = secondClient;

            transport.ClientEnqueue(packet);
            server.SingleStep();

            //dequeueing ping packet for second client
            transport.ClientDequeue();

            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            //dequeue ping packet
            transport.ClientDequeue();

            //dequeue ping packet
            transport.ClientDequeue();

            //dequeue of the packets for the first client (1 welcome 4 spawn)
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();

            //update packets
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();

            //gameinfo
            transport.ClientDequeue();

            //can finally dequeue the welcome packet for the second client
            byte[] welcomePacket = transport.ClientDequeue().data;

            uint clientId = BitConverter.ToUInt32(welcomePacket, 5);

            Assert.That(clientId, Is.EqualTo(1));
        }

        [Test]
        public void ClientOwnsDefaultPlayer()
        {
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            //dequeue ping packet
            transport.ClientDequeue();

            byte[] output = transport.ClientDequeue().data;

            uint playerId = BitConverter.ToUInt32(output, 9);

            Assert.That(server.CheckGameObjectOwner(playerId, firstClient), Is.EqualTo(true));
        }
    }
}

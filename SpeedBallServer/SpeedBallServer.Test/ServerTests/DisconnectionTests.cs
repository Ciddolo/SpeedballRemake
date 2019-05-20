using System;
using NUnit.Framework;

namespace SpeedBallServer.Test.ServerTests
{
    class DisconnectionTests
    {
        private GameServer server;
        private FakeClock clock;
        private FakeTransport transport;
        private FakeEndPoint firstClient, secondClient, thirdClient;

        [SetUp]
        public void SetUpTest()
        {
            transport = new FakeTransport();
            clock = new FakeClock();
            server = new GameServer(transport, clock);
            transport.Bind("127.0.0.1", 5000);
            firstClient = new FakeEndPoint("192.168.1.1", 5001);
            secondClient = new FakeEndPoint("192.168.1.2", 5002);
            thirdClient = new FakeEndPoint("192.168.1.3", 5003);
        }

        [Test]
        public void ClientDisconnection()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet(PacketsCommands.Join).GetData();
            packet.endPoint = firstClient;

            transport.ClientEnqueue(packet);

            server.SingleStep();

            clock.IncreaseTimeStamp(50f);

            server.SingleStep();

            Assert.That(server.ClientsAmount, Is.EqualTo(0));
        }

        [Test]
        public void JoinAfterDisconnection()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet(PacketsCommands.Join).GetData();
            packet.endPoint = firstClient;

            transport.ClientEnqueue(packet);
            server.SingleStep();

            clock.IncreaseTimeStamp(20f);
            packet.endPoint = secondClient;
            transport.ClientEnqueue(packet);
            server.SingleStep();

            clock.IncreaseTimeStamp(20f);
            server.SingleStep();

            packet.endPoint = thirdClient;
            transport.ClientEnqueue(packet);
            server.SingleStep();

            Assert.That(server.ClientsAmount, Is.EqualTo(2));
        }
    }
}

using System;
using NUnit.Framework;

namespace SpeedBallServer.Test.ServerTests
{
    class JoinTests
    {
        private GameServer server;
        private FakeClock clock;
        private FakeTransport transport;
        private FakeEndPoint maliciusClient,firstClient,secondClient,thirdClient;

        [SetUp]
        public void SetUpTest()
        {
            transport = new FakeTransport();
            clock = new FakeClock();
            server = new GameServer(transport, clock);

            transport.Bind("127.0.0.1", 5000);
            maliciusClient = new FakeEndPoint("127.0.0.1", 5000);

            firstClient = new FakeEndPoint("192.168.1.1", 5001);
            secondClient = new FakeEndPoint("192.168.1.2", 5002);
            thirdClient = new FakeEndPoint("192.168.1.3", 5003);

            FakeData packet = new FakeData();
            packet.data = new Packet(PacketsCommands.Join).GetData();
            packet.endPoint = firstClient;

            transport.ClientEnqueue(packet);
            server.SingleStep();

            //dequeue ping packet
            transport.ClientDequeue();
        }

        [Test]
        public void JoinMaliciusClient()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet(PacketsCommands.Join).GetData();
            packet.endPoint = maliciusClient;

            transport.ClientEnqueue(packet);
            server.SingleStep();

            Assert.That(transport.GetSendQueueCount(), Is.EqualTo(0));
        }

        [Test]
        public void SuccessfullJoinClientAmount()
        {
            Assert.That(server.ClientsAmount, Is.EqualTo(1));
        }

        [Test]
        public void UnsuccessfullJoin()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet(50).GetData();
            packet.endPoint = secondClient;

            transport.ClientEnqueue(packet);
            server.SingleStep();

            Assert.That(transport.GetSendQueueCount(), Is.EqualTo(0));
        }

        [Test]
        public void DefaultMalusAfterJoin()
        {
            Assert.That(server.GetClientMalus(firstClient), Is.EqualTo(0));
        }

        [Test]
        public void MalusAfterMultipleJoin()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet(PacketsCommands.Join).GetData();
            packet.endPoint = firstClient;

            transport.ClientEnqueue(packet);
            server.SingleStep();

            Assert.That(server.GetClientMalus(firstClient), Is.EqualTo(100));
        }

        [Test]
        public void ClientsAmountAfterMultipleJoins()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet(PacketsCommands.Join).GetData();
            packet.endPoint = secondClient;
            transport.ClientEnqueue(packet);
            packet.endPoint = thirdClient;
            transport.ClientEnqueue(packet);

            server.SingleStep();
            server.SingleStep();
            server.SingleStep();

            Assert.That(server.ClientsAmount, Is.EqualTo(2));
        }

        [Test]
        public void PacketsReceivedAfterJoin()
        {
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            Assert.That(transport.GetSendQueueCount(), Is.EqualTo(11));
        }
    }
}

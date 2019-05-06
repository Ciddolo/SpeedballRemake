using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit;
using System.Threading.Tasks;

namespace SpeedBallServer.Test
{
    class TestFromDifferentEndPoint
    {
        private GameServer server;
        private FakeClock clock;
        private FakeTransport transport;
        private FakeEndPoint FirstClient, SecondClient, ThirdClient;

        [SetUp]
        public void SetUpTest()
        {
            transport = new FakeTransport();
            clock = new FakeClock();
            server = new GameServer(transport, clock);
            transport.Bind("127.0.0.1", 5000);
            FirstClient = new FakeEndPoint("192.168.1.1", 5001);
            SecondClient = new FakeEndPoint("192.168.1.2", 5002);
            ThirdClient = new FakeEndPoint("192.168.1.3", 5003);
        }

        [Test]
        public void TestSuccessfullJoinResponse()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet(1).GetData();
            packet.endPoint = FirstClient;

            transport.ClientEnqueue(packet);
            server.SingleStep();
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();
            byte[] output = transport.ClientDequeue().data;

            Assert.That(output[0], Is.EqualTo(1));
        }

        [Test]
        public void TestSuccessfullJoinClientAmount()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet(1).GetData();
            packet.endPoint = FirstClient;

            transport.ClientEnqueue(packet);
            server.SingleStep();

            Assert.That(server.ClientsAmount, Is.EqualTo(1));
        }

        [Test]
        public void TestUnsuccessfullJoin()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet(0).GetData();
            packet.endPoint = FirstClient;

            transport.ClientEnqueue(packet);
            server.SingleStep();

            Assert.That(transport.GetSendQueueCount(), Is.EqualTo(0));
        }

        [Test]
        public void CheckClientMalusAfterJoin()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet(1).GetData();
            packet.endPoint = FirstClient;

            transport.ClientEnqueue(packet);
            server.SingleStep();

            Assert.That(server.GetClientMalus(FirstClient), Is.EqualTo(0));
        }

        [Test]
        public void CheckClientMalusAfterMultipleJoin()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet(1).GetData();
            packet.endPoint = FirstClient;

            transport.ClientEnqueue(packet);
            transport.ClientEnqueue(packet);

            server.SingleStep();
            server.SingleStep();

            Assert.That(server.GetClientMalus(FirstClient), Is.EqualTo(1));
        }

        [Test]
        public void TestSuccessfullJoinClientsAmount()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet(1).GetData();
            packet.endPoint = FirstClient;

            transport.ClientEnqueue(packet);
            packet.endPoint = SecondClient;
            transport.ClientEnqueue(packet);
            packet.endPoint = ThirdClient;
            transport.ClientEnqueue(packet);

            server.SingleStep();
            server.SingleStep();
            server.SingleStep();

            Assert.That(server.ClientsAmount, Is.EqualTo(2));
        }

        [Test]
        public void TestClientDisconnection()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet(1).GetData();
            packet.endPoint = FirstClient;

            transport.ClientEnqueue(packet);

            server.SingleStep();

            clock.IncreaseTimeStamp(50f);

            server.SingleStep();

            Assert.That(server.ClientsAmount, Is.EqualTo(0));
        }

        [Test]
        public void TestClientJoinAfterDisconnection()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet(1).GetData();
            packet.endPoint = FirstClient;

            transport.ClientEnqueue(packet);
            server.SingleStep();


            clock.IncreaseTimeStamp(20f);
            packet.endPoint = SecondClient;
            transport.ClientEnqueue(packet);
            server.SingleStep();

            clock.IncreaseTimeStamp(20f);
            server.SingleStep();

            packet.endPoint = ThirdClient;
            transport.ClientEnqueue(packet);
            server.SingleStep();

            Assert.That(server.ClientsAmount, Is.EqualTo(2));
        }
    }
}

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
        private FakeTransport transport;
        private FakeEndPoint client;

        [SetUp]
        public void SetUpTest()
        {
            transport = new FakeTransport();
            server = new GameServer(transport);
            transport.Bind("127.0.0.1", 5000);
            client = new FakeEndPoint("client", 0);
        }

        [Test]
        public void TestSuccessfullJoin()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet(1).GetData();
            packet.endPoint = client;

            transport.ClientEnqueue(packet);
            server.SingleStep();

            byte[] output = transport.ClientDequeue().data;

            Assert.That(output[0], Is.EqualTo(0x1));
        }

        [Test]
        public void TestUnsuccessfullJoin()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet(0).GetData();
            packet.endPoint = client;

            transport.ClientEnqueue(packet);
            server.SingleStep();

            Assert.That(transport.GetSendQueueCount(), Is.EqualTo(0));
        }

    }
}

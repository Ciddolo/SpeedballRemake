using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer.Test
{
    class TestFromSameEndpoint
    {
        private GameServer server;
        private FakeTransport transport;
        private FakeEndPoint client;

        [SetUp]
        public void SetUpTest()
        {
            transport = new FakeTransport();
            server = new GameServer(transport);
            transport.Bind("client", 0);
            client = new FakeEndPoint("client", 0);
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

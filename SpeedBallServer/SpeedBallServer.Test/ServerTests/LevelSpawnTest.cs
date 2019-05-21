using System;
using System.Numerics;
using NUnit.Framework;
using System.IO;

namespace SpeedBallServer.Test.ServerTests
{
    class LevelSpawnTest
    {
        private GameServer server;
        private FakeClock clock;
        private FakeTransport transport;

        [SetUp]
        public void SetUpTest()
        {
            transport = new FakeTransport();
            clock = new FakeClock();
            transport.Bind("127.0.0.1", 5000);

            string level = File.ReadAllText(TestContext.CurrentContext.TestDirectory + "\\TestData\\TestLevel.json");
            server = new GameServer(transport, clock, 1, level);
        }

        [Test]
        public void CheckSpawnedGameObjects()
        {
            Assert.That(server.GameObjectsAmount,Is.EqualTo(21));
        }

        [Test]
        public void FirstClientMovingControlledPlayer()
        {
            //initializinng clients
            FakeEndPoint firstClient = new FakeEndPoint("192.168.1.1", 5001);
            FakeEndPoint secondClient = new FakeEndPoint("192.168.1.2", 5002);

            //initializing join packet
            FakeData packet = new FakeData();
            packet.data = new Packet(PacketsCommands.Join).GetData();
            packet.endPoint = firstClient;

            //sending packet
            transport.ClientEnqueue(packet);

            //server reads join
            server.SingleStep();
            clock.IncreaseTimeStamp(1f);

            //dequeue ping packet
            transport.ClientDequeue();

            //server responds after 1 second (default tick frequency)
            server.SingleStep();

            //dequeue ping packet
            transport.ClientDequeue();

            packet = new FakeData();
            packet.data = new Packet(PacketsCommands.Join).GetData();
            packet.endPoint = secondClient;
            transport.ClientEnqueue(packet);

            //read second client join
            server.SingleStep();

            //get controlled object id of the first client
            byte[] welcomeData = transport.ClientDequeue().data;
            uint playerObjectId = BitConverter.ToUInt32(welcomeData, 9);

            Console.WriteLine(playerObjectId);
            //initializing update packet
            packet.data = new Packet(PacketsCommands.Update, false, playerObjectId, 10f, 10f, 5f, 5f).GetData();
            packet.endPoint = firstClient;
            transport.ClientEnqueue(packet);

            //reading update packet
            server.SingleStep();

            Assert.That(server.GameObjectsTable[playerObjectId].Position, Is.EqualTo(new Vector2(10f, 10f)));
            Assert.That(((Player)server.GameObjectsTable[playerObjectId]).LookingDirection, Is.EqualTo(new Vector2(5f, 5f)));
        }
    }
}

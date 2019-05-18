using System;
using NUnit.Framework;
using System.Numerics;

namespace SpeedBallServer.Test.ServerTests
{
    public class UpdateTests
    {
        private GameServer server;
        private FakeClock clock;
        private FakeTransport transport;
        private FakeEndPoint firstClient, secondClient;

        [SetUp]
        public void SetUpTest()
        {
            //initializing objects needed fot the server
            transport = new FakeTransport();
            clock = new FakeClock();
            transport.Bind("127.0.0.1", 5000);

            //initializing server
            server = new GameServer(transport, clock);

            //initializinng clients
            firstClient = new FakeEndPoint("192.168.1.1", 5001);
            secondClient = new FakeEndPoint("192.168.1.2", 5002);

            //initializing join packet
            FakeData packet = new FakeData();
            packet.data = new Packet((byte)PacketsCommands.Join).GetData();
            packet.endPoint = firstClient;

            //sending packet
            transport.ClientEnqueue(packet);

            //server reads join
            server.SingleStep();
            clock.IncreaseTimeStamp(1f);

            //server responds after 1 second (default tick frequency)
            server.SingleStep();

            //dequeue ping packet
            transport.ClientDequeue();
        }

        [Test]
        public void FirstClientMovingControlledPlayer()
        {
            //get controlled object id
            byte[] welcomeData = transport.ClientDequeue().data;
            uint playerObjectId = BitConverter.ToUInt32(welcomeData, 10);

            //dequeueing obstacle spawn packet
            transport.ClientDequeue();

            //initializing update packet
            FakeData packet = new FakeData();

            packet.data = new Packet((byte)PacketsCommands.Update,false,playerObjectId,10f,10f,10f,10f).GetData();
            packet.endPoint = firstClient;
            transport.ClientEnqueue(packet);

            //reading update packet
            server.SingleStep();
            
            //object should not be move since the game is in the state "WaitingForPlayers"
            Assert.That(server.GameObjectsTable[playerObjectId].Position, Is.Not.EqualTo(new Vector2(10f,10f)));
        }

        [Test]
        public void FirstClientMovingControlledPlayerAfterSecondClientJoin()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet((byte)PacketsCommands.Join).GetData();
            packet.endPoint = secondClient;
            transport.ClientEnqueue(packet);

            //read second client join
            server.SingleStep();

            //get controlled object id of the first client
            byte[] welcomeData = transport.ClientDequeue().data;
            uint playerObjectId = BitConverter.ToUInt32(welcomeData, 10);

            //initializing update packet
            packet.data = new Packet((byte)PacketsCommands.Update, false, playerObjectId, 10f, 10f, 5f, 5f).GetData();
            packet.endPoint = firstClient;
            transport.ClientEnqueue(packet);

            //reading update packet
            server.SingleStep();

            Assert.That(server.GameObjectsTable[playerObjectId].Position, Is.EqualTo(new Vector2(10f, 10f)));
            Assert.That(((Player)server.GameObjectsTable[playerObjectId]).LookingDirection, Is.EqualTo(new Vector2(5f, 5f)));
        }

        [Test]
        public void SecondClientMovingControlledPlayer()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet((byte)PacketsCommands.Join).GetData();
            packet.endPoint = secondClient;
            transport.ClientEnqueue(packet);

            //increasing time to make server responds to the secon player
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            //first client welcome
            transport.ClientDequeue();

            //dequeueing obstacle spawn packet
            transport.ClientDequeue();

            //dequeueing first player spawn
            transport.ClientDequeue();

            //dequeueing second player spawn
            transport.ClientDequeue();

            //dequeueing first player spawn of the second team
            transport.ClientDequeue();

            //dequeue ping packet second client
            transport.ClientDequeue();

            //dequeing packet for first client
            //welcome
            transport.ClientDequeue();
            //obstacle spawn
            transport.ClientDequeue();
            //spawn playerOne  of the first team
            transport.ClientDequeue();
            //spawn playerTwo of the first team
            transport.ClientDequeue();

            //spawn playerTwo of the second team
            transport.ClientDequeue();

            //welcome for secondClient
            byte[] welcomeData = transport.ClientDequeue().data;
            uint playerObjectId = BitConverter.ToUInt32(welcomeData, 10);
            //initializing update packet
            packet.data = new Packet((byte)PacketsCommands.Update, false, playerObjectId, 10f, 10f, 10f, 10f).GetData();
            packet.endPoint = secondClient;
            transport.ClientEnqueue(packet);

            //reading update packet
            server.SingleStep();

            Assert.That(server.GameObjectsTable[playerObjectId].Position, Is.EqualTo(new Vector2(10f, 10f)));
        }

        [Test]
        public void FirstClientMovingPlayerNotControlled()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet((byte)PacketsCommands.Join).GetData();
            packet.endPoint = secondClient;
            transport.ClientEnqueue(packet);

            //read second client join
            server.SingleStep();

            //dequeue welcome packet
            transport.ClientDequeue();

            //dequeueing obstacle spawn packet
            transport.ClientDequeue();

            //dequeueing first player spawn packet
            transport.ClientDequeue();

            //get controlled object position
            byte[] SpawnData = transport.ClientDequeue().data;

            uint playerObjectId = BitConverter.ToUInt32(SpawnData,10);

            //initializing update packet
            packet.data = new Packet((byte)PacketsCommands.Update, false, playerObjectId, 10f, 10f, 10f, 10f).GetData();
            packet.endPoint = firstClient;
            transport.ClientEnqueue(packet);

            //reading update packet
            server.SingleStep();

            Assert.That(server.GameObjectsTable[playerObjectId].Position, Is.Not.EqualTo(new Vector2(10f, 10f)));
        }

        [Test]
        public void SecondClientMovingPlayerNonOwned()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet((byte)PacketsCommands.Join).GetData();
            packet.endPoint = secondClient;
            transport.ClientEnqueue(packet);

            //increasing time to make server responds to the secon player
            clock.IncreaseTimeStamp(1f);

            //read second client join
            server.SingleStep();

            //get controlled object id of the first client
            byte[] welcomeData = transport.ClientDequeue().data;
            uint playerObjectId = BitConverter.ToUInt32(welcomeData, 10);

            //initializing update packet
            packet.data = new Packet((byte)PacketsCommands.Update, false, playerObjectId, 10f, 10f, 10f, 10f).GetData();
            packet.endPoint = secondClient;
            transport.ClientEnqueue(packet);

            //reading update packet
            server.SingleStep();

            Assert.That(server.GameObjectsTable[playerObjectId].Position, Is.Not.EqualTo(new Vector2(10f, 10f)));
        }
    }
}

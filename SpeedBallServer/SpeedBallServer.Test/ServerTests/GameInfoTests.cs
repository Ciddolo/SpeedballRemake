using System;
using System.Numerics;
using NUnit.Framework;

namespace SpeedBallServer.Test.ServerTests
{
    class GameInfoTests
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
        public void ControlledPlayerFromGameInfo()
        {
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            //while(true)
            //{
            //    byte[] outp = transport.ClientDequeue().data;
            //    Console.WriteLine(outp[0]);
            //}

            //dequeue ping
            transport.ClientDequeue();

            byte[] welcomePacket = (transport.ClientDequeue()).data;

            uint playerObjectId = BitConverter.ToUInt32(welcomePacket, 9);

            Console.WriteLine(playerObjectId);

            //spawn updates and welcome
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();

            //cane dequeue gameinfo
            byte[] gameInfoPacket = (transport.ClientDequeue()).data;

            //reading controlled id object from game info packet
            uint playerObjectIdFromGameInfo = BitConverter.ToUInt32(gameInfoPacket, 13);

            Assert.That(playerObjectIdFromGameInfo, Is.EqualTo(playerObjectId));
        }

        [Test]
        public void ControlledPlayereChangingOnTakingBallReadedFromGameInfo()
        {
            server.GetBall().SetPosition(3f, 0f);


            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            //while(true)
            //{
            //    byte[] outp = transport.ClientDequeue().data;
            //    Console.WriteLine(outp[0]);
            //}

            //dequeue ping
            transport.ClientDequeue();

            byte[] welcomePacket = (transport.ClientDequeue()).data;

            uint playerObjectId = BitConverter.ToUInt32(welcomePacket, 9);

            Console.WriteLine(playerObjectId);

            //spawn updates and welcome
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();

            //cane dequeue gameinfo
            byte[] gameInfoPacket = (transport.ClientDequeue()).data;

            //reading controlled id object from game info packet
            uint playerObjectIdFromGameInfo = BitConverter.ToUInt32(gameInfoPacket, 13);

            Assert.That(playerObjectIdFromGameInfo, Is.Not.EqualTo(playerObjectId));
        }

        [Test]
        public void TestAfterSelect()
        {
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            //dequeue ping
            transport.ClientDequeue();

            byte[] welcomePacket = (transport.ClientDequeue()).data;

            uint playerObjectId = BitConverter.ToUInt32(welcomePacket, 9);

            Packet SelectPlayerPacket = new Packet(PacketsCommands.Input, false, (byte)0, playerObjectId + 1);
            FakeData fakePack = new FakeData();
            fakePack.data = SelectPlayerPacket.GetData();
            fakePack.endPoint = firstClient;
            transport.ClientEnqueue(fakePack);
            server.SingleStep();

            //Console.WriteLine(playerObjectId);

            //spawn updates and welcome
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
            //dequeue old gameinfo
            transport.ClientDequeue();

            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();

            //can dequeue gameinfo
            byte[] gameInfoPacket = (transport.ClientDequeue()).data;

            //reading controlled id object from game info packet
            uint playerObjectIdFromGameInfo = BitConverter.ToUInt32(gameInfoPacket, 13);

            Assert.That(playerObjectIdFromGameInfo, Is.Not.EqualTo(playerObjectId));
            Assert.That(playerObjectIdFromGameInfo, Is.EqualTo(playerObjectId + 1));
        }

        [Test]
        public void ScoreUpdate()
        {
            server.GetBall().SetPosition(20f, 20f);

            uint myObjId = server.GetClientControlledPlayer(firstClient);
            server.GameObjectsTable[myObjId].Position = new Vector2(100f, 100f);

            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            //dequeue ping
            transport.ClientDequeue();

            byte[] welcomePacket = (transport.ClientDequeue()).data;

            uint playerObjectId = BitConverter.ToUInt32(welcomePacket, 9);

            Console.WriteLine(playerObjectId);

            //spawn updates and welcome
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();

            //cane dequeue gameinfo
            byte[] gameInfoPacket = (transport.ClientDequeue()).data;

            //reading controlled id object from game info packet
            uint teamOneScore = BitConverter.ToUInt32(gameInfoPacket, 5);
            uint teamTwoScore = BitConverter.ToUInt32(gameInfoPacket, 9);

            Assert.That(teamOneScore, Is.Not.EqualTo(1));
            Assert.That(teamTwoScore, Is.Not.EqualTo(0));
        }

        [Test]
        public void ObjectsResetsAfterGoal()
        {
            server.GetBall().SetPosition(20f, 20f);

            uint myObjId = server.GetClientControlledPlayer(firstClient);
            server.GameObjectsTable[myObjId].Position = new Vector2(100f, 100f);

            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            //dequeue ping
            transport.ClientDequeue();

            byte[] welcomePacket = (transport.ClientDequeue()).data;

            uint playerObjectId = BitConverter.ToUInt32(welcomePacket, 9);

            //spawn updates and welcome
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();
            transport.ClientDequeue();

            //can dequeue gameinfo
            byte[] gameInfoPacket = (transport.ClientDequeue()).data;

            //reading controlled id object from game info packet
            uint teamOneScore = BitConverter.ToUInt32(gameInfoPacket, 5);
            uint teamTwoScore = BitConverter.ToUInt32(gameInfoPacket, 9);

            Assert.That(server.GetBall().Position, Is.EqualTo(new Vector2(30f, 30f)));
            Assert.That(server.GameObjectsTable[playerObjectId].Position, Is.EqualTo(new Vector2(-1, 0f)));
        }
    }
}

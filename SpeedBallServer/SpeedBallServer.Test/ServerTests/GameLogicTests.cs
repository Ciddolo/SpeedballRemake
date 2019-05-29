using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer.Test.ServerTests
{
    class GameLogicTests
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

            FakeData packet = new FakeData();
            packet.data = new Packet(PacketsCommands.Join).GetData();
            packet.endPoint = firstClient;

            transport.ClientEnqueue(packet);
            server.SingleStep();

            //dequeue ping packet
            transport.ClientDequeue();
        }

        [Test]
        public void GameStateAfterMultipleJoin()
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

            Assert.That(server.CurrentGameState, Is.EqualTo(GameState.Playing));
        }

        [Test]
        public void GameStateAfterClientDisconnection()
        {

            clock.IncreaseTimeStamp(50f);

            server.SingleStep();

            Assert.That(server.CurrentGameState, Is.EqualTo(GameState.WaitingForPlayers));
        }

        [Test]
        public void GameStateAfterMultipleJoinAndOneDisconnection()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet(PacketsCommands.Join).GetData();

            packet.endPoint = secondClient;

            //second client join
            transport.ClientEnqueue(packet);
            server.SingleStep();

            //every increaseTimestamp and single step increase the malus for clients not responding
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            byte[] pingPacket = (transport.ClientDequeue()).data;

            uint pingPacketId = BitConverter.ToUInt32(pingPacket, 1);

            FakeData pongPacket = new FakeData();
            pongPacket.endPoint = firstClient;
            pongPacket.data = (new Packet(PacketsCommands.Pong, false, pingPacketId)).GetData();

            transport.ClientEnqueue(pongPacket);

            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            Assert.That(server.CurrentGameState, Is.EqualTo(GameState.Ended));
        }

        [Test]
        public void GameStateAfterOneJoin()
        {

            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            Assert.That(server.CurrentGameState, Is.EqualTo(GameState.WaitingForPlayers));
        }

        [Test]
        public void ControlledPlayereChangingOnTakingBall()
        {
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            //dequeue ping
            transport.ClientDequeue();

            byte[] welcomePacket = (transport.ClientDequeue()).data;

            uint playerObjectId = BitConverter.ToUInt32(welcomePacket, 9);

            Console.WriteLine(playerObjectId);

            ////moving ball on second player of the first client
            server.GetBall().SetPosition(3f, 0f);

            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            Assert.That(server.GetBall().gameLogic, Is.Not.EqualTo(null));
            Assert.That(server.GetClientControlledPlayer(firstClient), Is.Not.EqualTo(playerObjectId));
        }
    }
}

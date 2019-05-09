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
            packet.data = new Packet((byte)PacketsCommands.Join).GetData();
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
            packet.data = new Packet((byte)PacketsCommands.Join).GetData();
            packet.endPoint = FirstClient;

            transport.ClientEnqueue(packet);
            server.SingleStep();
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            Assert.That(server.ClientsAmount, Is.EqualTo(1));
        }

        [Test]
        public void TestUnsuccessfullJoin()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet(50).GetData();
            packet.endPoint = FirstClient;

            transport.ClientEnqueue(packet);
            server.SingleStep();
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            Assert.That(transport.GetSendQueueCount(), Is.EqualTo(0));
        }

        [Test]
        public void CheckClientMalusAfterJoin()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet((byte)PacketsCommands.Join).GetData();
            packet.endPoint = FirstClient;

            transport.ClientEnqueue(packet);
            server.SingleStep();

            Assert.That(server.GetClientMalus(FirstClient), Is.EqualTo(0));
        }

        [Test]
        public void CheckClientMalusAfterMultipleJoin()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet((byte)PacketsCommands.Join).GetData();
            packet.endPoint = FirstClient;

            transport.ClientEnqueue(packet);
            transport.ClientEnqueue(packet);

            server.SingleStep();
            server.SingleStep();

            Assert.That(server.GetClientMalus(FirstClient), Is.EqualTo(100));
        }

        [Test]
        public void TestSuccessfullJoinClientsAmount()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet((byte)PacketsCommands.Join).GetData();
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
            packet.data = new Packet((byte)PacketsCommands.Join).GetData();
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
            packet.data = new Packet((byte)PacketsCommands.Join).GetData();
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

        [Test]
        public void TestSuccessfullJoinAcksAmount()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet((byte)PacketsCommands.Join).GetData();
            packet.endPoint = FirstClient;

            transport.ClientEnqueue(packet);

            server.SingleStep();
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();
            clock.IncreaseTimeStamp(1f);

            Assert.That(transport.GetSendQueueCount(), Is.EqualTo(15));
        }

        [Test]
        public void TestAcksPacektsAttempts()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet((byte)PacketsCommands.Join).GetData();
            packet.endPoint = FirstClient;

            transport.ClientEnqueue(packet);

            server.SingleStep();
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            byte[] output = transport.ClientDequeue().data;

            uint packetId = BitConverter.ToUInt32(output,1);

            Assert.That(server.AcksIdsNeededFrom(FirstClient).Contains(packetId), Is.EqualTo(true));
        }

        [Test]
        public void TestSuccessfullJoinAckResponds()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet((byte)PacketsCommands.Join).GetData();
            packet.endPoint = FirstClient;

            transport.ClientEnqueue(packet);

            server.SingleStep();
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            byte[] output = transport.ClientDequeue().data;

            uint packetId = BitConverter.ToUInt32(output, 1);

            packet.data = new Packet((byte)PacketsCommands.Ack, false, packetId).GetData();
            Console.WriteLine(packetId);

            transport.ClientEnqueue(packet);
            server.SingleStep();
            server.SingleStep();

            Assert.That(server.AcksIdsNeededFrom(FirstClient).Contains(packetId), Is.EqualTo(false));
        }

        [Test]
        public void TestWelcomePacketSpawn()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet((byte)PacketsCommands.Join).GetData();
            packet.endPoint = FirstClient;

            transport.ClientEnqueue(packet);

            server.SingleStep();
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();


            Assert.That(server.GameObjectsAmount, Is.EqualTo(4));
        }

        [Test]
        public void TestPacketsReceivedAfterWelcome()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet((byte)PacketsCommands.Join).GetData();
            packet.endPoint = FirstClient;

            transport.ClientEnqueue(packet);

            server.SingleStep();
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            Assert.That(transport.GetSendQueueCount(), Is.EqualTo(5));
        }

        [Test]
        public void TestFirstClientAfterJoin()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet((byte)PacketsCommands.Join).GetData();
            packet.endPoint = FirstClient;

            transport.ClientEnqueue(packet);

            server.SingleStep();
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            byte[] output = transport.ClientDequeue().data;

            uint clientId = BitConverter.ToUInt32(output, 6);

            Assert.That(clientId, Is.EqualTo(0));
        }

        [Test]
        public void TestSecondClientIdAfterJoin()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet((byte)PacketsCommands.Join).GetData();
            packet.endPoint = FirstClient;

            transport.ClientEnqueue(packet);

            server.SingleStep();

            packet.endPoint = SecondClient;
            transport.ClientEnqueue(packet);

            server.SingleStep();
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            //dequeue of the packets for the first client (1 welcome 4 spawn)
            byte[] output = transport.ClientDequeue().data;
            output = transport.ClientDequeue().data;
            output = transport.ClientDequeue().data;
            output = transport.ClientDequeue().data;
            output = transport.ClientDequeue().data;
            //dequeue of the same 5 packets this packets need ack 
            output = transport.ClientDequeue().data;
            output = transport.ClientDequeue().data;
            output = transport.ClientDequeue().data;
            output = transport.ClientDequeue().data;
            //can finally dequeue the welcome packet for the second client
            output = transport.ClientDequeue().data;

            uint clientId = BitConverter.ToUInt32(output, 6);

            Assert.That(clientId, Is.EqualTo(1));
        }

        [Test]
        public void TestFirstClientDefaultPlayerOwner()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet((byte)PacketsCommands.Join).GetData();
            packet.endPoint = FirstClient;

            transport.ClientEnqueue(packet);

            server.SingleStep();
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            byte[] output = transport.ClientDequeue().data;

            uint playerId = BitConverter.ToUInt32(output, 10);

            Assert.That(server.CheckGameObjectOwner(playerId, FirstClient), Is.EqualTo(true));
        }

        [Test]
        public void TestFirstClientNonDefaultPlayerOwner()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet((byte)PacketsCommands.Join).GetData();
            packet.endPoint = FirstClient;

            transport.ClientEnqueue(packet);

            server.SingleStep();
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            //welcome packet
            byte[] output = transport.ClientDequeue().data;
            //spawn obstacle packet
            output = transport.ClientDequeue().data;
            //spawn player (default)
            output = transport.ClientDequeue().data;
            //spawn second player
            output = transport.ClientDequeue().data;

            uint playerId = BitConverter.ToUInt32(output, 10);

            Console.WriteLine(playerId);
            //Console.WriteLine(controlledPlayerId);

            Assert.That(server.CheckGameObjectOwner(playerId, FirstClient), Is.EqualTo(true));
        }

        [Test]
        public void TestIfFirstClientDoesNotOwnOtherTeamPlayers()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet((byte)PacketsCommands.Join).GetData();
            packet.endPoint = FirstClient;

            transport.ClientEnqueue(packet);

            server.SingleStep();
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            //welcome packet
            byte[] output = transport.ClientDequeue().data;
            //spawn obstacle packet
            output = transport.ClientDequeue().data;
            //spawn player (default)
            output = transport.ClientDequeue().data;
            //spawn second player
            output = transport.ClientDequeue().data;
            //spawn first player of the second team (default)
            output = transport.ClientDequeue().data;

            uint playerId = BitConverter.ToUInt32(output, 10);

            Assert.That(server.CheckGameObjectOwner(playerId, FirstClient), Is.EqualTo(false));
        }

        [Test]
        public void TestFirstClientTeamIdWithPlayerTeamOneId()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet((byte)PacketsCommands.Join).GetData();
            packet.endPoint = FirstClient;

            transport.ClientEnqueue(packet);

            server.SingleStep();
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            //welcome packet
            byte[] output = transport.ClientDequeue().data;
            uint clientId = BitConverter.ToUInt32(output, 6);
            //spawn obstacle packet
            output = transport.ClientDequeue().data;
            //spawn player (default)
            output = transport.ClientDequeue().data;
            //spawn second player
            output = transport.ClientDequeue().data;

            uint playerClientId = BitConverter.ToUInt32(output, 30);

            Assert.That(clientId, Is.EqualTo(playerClientId));
        }

        public void TestFirstClientTeamIdWithPlayerTeamTwoId()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet((byte)PacketsCommands.Join).GetData();
            packet.endPoint = FirstClient;

            transport.ClientEnqueue(packet);

            server.SingleStep();
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            //welcome packet
            byte[] output = transport.ClientDequeue().data;
            uint clientId = BitConverter.ToUInt32(output, 6);
            //spawn obstacle packet
            output = transport.ClientDequeue().data;
            //spawn player (default)
            output = transport.ClientDequeue().data;
            //spawn second player
            output = transport.ClientDequeue().data;
            //spawn first player of the second team (default)
            output = transport.ClientDequeue().data;

            uint playerClientId = BitConverter.ToUInt32(output, 30);

            Assert.That(clientId, Is.Not.EqualTo(playerClientId));
        }

        public void TestGameStateAfterOneJoin()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet((byte)PacketsCommands.Join).GetData();
            packet.endPoint = FirstClient;

            transport.ClientEnqueue(packet);

            server.SingleStep();
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            Assert.That(server.CurrentGameState, Is.EqualTo(GameState.WaitingForPlayers));
        }

        [Test]
        public void TestGameStateAfterMultipleJoin()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet((byte)PacketsCommands.Join).GetData();
            packet.endPoint = FirstClient;

            transport.ClientEnqueue(packet);
            packet.endPoint = SecondClient;
            transport.ClientEnqueue(packet);
            packet.endPoint = ThirdClient;
            transport.ClientEnqueue(packet);

            server.SingleStep();
            server.SingleStep();
            server.SingleStep();

            Assert.That(server.CurrentGameState, Is.EqualTo(GameState.Playing));
        }

        [Test]
        public void TestClientDisconnectionGameState()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet((byte)PacketsCommands.Join).GetData();
            packet.endPoint = FirstClient;

            transport.ClientEnqueue(packet);

            server.SingleStep();

            clock.IncreaseTimeStamp(50f);

            server.SingleStep();

            Assert.That(server.CurrentGameState, Is.EqualTo(GameState.WaitingForPlayers));
        }



        [Test]
        public void TestGameStateAfterMultipleJoinAndOneDisconnection()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet((byte)PacketsCommands.Join).GetData();
            packet.endPoint = FirstClient;

            transport.ClientEnqueue(packet);
            packet.endPoint = SecondClient;

            transport.ClientEnqueue(packet);

            server.SingleStep();
            clock.IncreaseTimeStamp(10f);
            server.SingleStep();
            clock.IncreaseTimeStamp(30f);
            server.SingleStep();

            Assert.That(server.CurrentGameState, Is.EqualTo(GameState.Ended));
        }
    }
}

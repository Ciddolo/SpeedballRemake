using System;
using NUnit.Framework;

namespace SpeedBallServer.Test.ServerTests
{
    class SpawnTests
    {
        private GameServer server;
        private FakeClock clock;
        private FakeTransport transport;
        private FakeEndPoint firstClient;

        [SetUp]
        public void SetUpTest()
        {
            transport = new FakeTransport();
            clock = new FakeClock();
            server = new GameServer(transport, clock);

            transport.Bind("127.0.0.1", 5000);
            firstClient = new FakeEndPoint("192.168.1.1", 5001);

            FakeData packet = new FakeData();
            packet.data = new Packet(PacketsCommands.Join).GetData();
            packet.endPoint = firstClient;

            transport.ClientEnqueue(packet);

            server.SingleStep();

            //dequeue ping packet
            transport.ClientDequeue();
        }

        [Test]
        public void SpawnedObjects()
        {
            Assert.That(server.GameObjectsAmount, Is.EqualTo(4));
        }

        [Test]
        public void TestFirstClientNonDefaultPlayerOwner()
        {
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            //welcome packet
            transport.ClientDequeue();
            //spawn obstacle packet
            transport.ClientDequeue();
            //spawn player (default)(first client)
            transport.ClientDequeue();
            //spawn second player (first client)
            byte[] spawnPacket = transport.ClientDequeue().data;

            uint playerId = BitConverter.ToUInt32(spawnPacket, 9);

            Assert.That(server.CheckGameObjectOwner(playerId, firstClient), Is.EqualTo(true));
        }

        [Test]
        public void FirstClientDoesNotOwnOtherTeamPlayers()
        {
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            //dequeue ping packet
            transport.ClientDequeue();

            //welcome packet
            transport.ClientDequeue();
            //spawn obstacle packet
            transport.ClientDequeue();
            //spawn player (default)
            transport.ClientDequeue();
            //spawn second player
            transport.ClientDequeue();
            //spawn first player of the second team (default)
            byte[] spawnPacket = transport.ClientDequeue().data;

            uint playerId = BitConverter.ToUInt32(spawnPacket, 9);

            Assert.That(server.CheckGameObjectOwner(playerId, firstClient), Is.EqualTo(false));
        }

        [Test]
        public void ClientOwnsPlayer()
        {
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            //dequeue ping packet
            transport.ClientDequeue();

            //welcome packet
            byte[] output = transport.ClientDequeue().data;
            uint clientId = BitConverter.ToUInt32(output, 5);
            //spawn obstacle packet
            output = transport.ClientDequeue().data;
            //spawn player (default)
            output = transport.ClientDequeue().data;
            //spawn second player
            output = transport.ClientDequeue().data;

            uint playerClientId = BitConverter.ToUInt32(output, 29);

            Assert.That(clientId, Is.EqualTo(playerClientId));
        }

        public void ClientDoesNotOwnPlayer()
        {
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            //welcome packet
            byte[] output = transport.ClientDequeue().data;
            uint clientId = BitConverter.ToUInt32(output, 5);
            //spawn obstacle packet
            output = transport.ClientDequeue().data;
            //spawn player (default)
            output = transport.ClientDequeue().data;
            //spawn second player
            output = transport.ClientDequeue().data;
            //spawn first player of the second team (default)
            output = transport.ClientDequeue().data;

            uint playerClientId = BitConverter.ToUInt32(output, 29);

            Assert.That(clientId, Is.Not.EqualTo(playerClientId));
        }
    }
}

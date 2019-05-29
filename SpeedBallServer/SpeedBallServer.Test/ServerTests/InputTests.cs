using System;
using System.Numerics;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer.Test.ServerTests
{
    public class InputTests
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
        }

        [Test]
        public void DefaultControlledPlayer()
        {
            byte[] welcomePacketData = transport.ClientDequeue().data;
            uint playerId = BitConverter.ToUInt32(welcomePacketData, 9);
            Assert.That(playerId, Is.EqualTo(server.GetClientControlledPlayer(firstClient)));
        }

        [Test]
        public void ControlledPlayerAfterSelectPlayer()
        {
            byte[] welcomePacketData = transport.ClientDequeue().data;
            uint playerId = BitConverter.ToUInt32(welcomePacketData, 9);

            Packet SelectPlayerPacket = new Packet(PacketsCommands.Input, false, (byte)0, playerId + 1);
            FakeData fakePack = new FakeData();
            fakePack.data = SelectPlayerPacket.GetData();
            fakePack.endPoint = firstClient;
            transport.ClientEnqueue(fakePack);
            server.SingleStep();

            Assert.That(playerId, Is.EqualTo(server.GetClientControlledPlayer(firstClient)));
        }

        [Test]
        public void ControlledPlayerAfterSelectPlayerGameStarted()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet(PacketsCommands.Join).GetData();
            packet.endPoint = secondClient;
            transport.ClientEnqueue(packet);

            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            byte[] welcomePacketData = transport.ClientDequeue().data;
            uint playerId = BitConverter.ToUInt32(welcomePacketData, 9);

            Packet SelectPlayerPacket = new Packet(PacketsCommands.Input, false, (byte)0, playerId + 1);
            FakeData fakePack = new FakeData();
            fakePack.data = SelectPlayerPacket.GetData();
            fakePack.endPoint = firstClient;
            transport.ClientEnqueue(fakePack);
            server.SingleStep();

            Assert.That(playerId, Is.Not.EqualTo(server.GetClientControlledPlayer(firstClient)));
        }

        [Test]
        public void ControlledPlayerAfterMaliciusSelectPlayerGameStarted()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet(PacketsCommands.Join).GetData();
            packet.endPoint = secondClient;
            transport.ClientEnqueue(packet);

            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            byte[] welcomePacketData = transport.ClientDequeue().data;
            uint playerId = BitConverter.ToUInt32(welcomePacketData, 9);

            Packet SelectPlayerPacket = new Packet(PacketsCommands.Input, false, (byte)InputType.SelectPlayer, playerId + 2);
            FakeData fakePack = new FakeData();
            fakePack.data = SelectPlayerPacket.GetData();
            fakePack.endPoint = firstClient;

            transport.ClientEnqueue(fakePack);
            server.SingleStep();

            Assert.That(playerId, Is.EqualTo(server.GetClientControlledPlayer(firstClient)));
        }

        [Test]
        public void MovePlayerGameStarted()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet(PacketsCommands.Join).GetData();
            packet.endPoint = secondClient;
            transport.ClientEnqueue(packet);

            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            byte[] welcomePacketData = transport.ClientDequeue().data;
            uint playerId = BitConverter.ToUInt32(welcomePacketData, 9);
            Vector2 startPos = (server.GameObjectsTable[playerId].Position);

            Packet SelectPlayerPacket = new Packet(PacketsCommands.Input, false, (byte)InputType.Movement, 1f, 0f);
            FakeData fakePack = new FakeData();
            fakePack.data = SelectPlayerPacket.GetData();
            fakePack.endPoint = firstClient;

            transport.ClientEnqueue(fakePack);
            server.SingleStep();

            float updateTime = 1f;

            clock.IncreaseTimeStamp(updateTime);
            server.SingleStep();

            Assert.That(startPos + (new Vector2(1f, 0f) * updateTime * 5), Is.EqualTo(server.GameObjectsTable[playerId].Position));
        }

        [Test]
        public void VelocityResetAfterChangePlayer()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet(PacketsCommands.Join).GetData();
            packet.endPoint = secondClient;
            transport.ClientEnqueue(packet);

            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            byte[] welcomePacketData = transport.ClientDequeue().data;
            uint playerId = BitConverter.ToUInt32(welcomePacketData, 9);
            Vector2 startPos = (server.GameObjectsTable[playerId].Position);

            Packet MovePlayerPacket = new Packet(PacketsCommands.Input, false, (byte)InputType.Movement, 1f, 0f);
            FakeData fakePack = new FakeData();
            fakePack.data = MovePlayerPacket.GetData();
            fakePack.endPoint = firstClient;

            transport.ClientEnqueue(fakePack);
            server.SingleStep();

            float updateTime = 1f;

            clock.IncreaseTimeStamp(updateTime);
            server.SingleStep();

            Assert.That(startPos + (new Vector2(1f, 0f) * updateTime * 5), Is.EqualTo(server.GameObjectsTable[playerId].Position));

            Packet SelectPlayerPacket = new Packet(PacketsCommands.Input, false, (byte)InputType.SelectPlayer, playerId + 1);
            fakePack = new FakeData();
            fakePack.data = SelectPlayerPacket.GetData();
            fakePack.endPoint = firstClient;
            transport.ClientEnqueue(fakePack);
            server.SingleStep();

            clock.IncreaseTimeStamp(updateTime);
            server.SingleStep();

            clock.IncreaseTimeStamp(updateTime);
            server.SingleStep();

            clock.IncreaseTimeStamp(updateTime);
            server.SingleStep();

            Assert.That(playerId, Is.Not.EqualTo(server.GetClientControlledPlayer(firstClient)));
            Assert.That(startPos + (new Vector2(1f, 0f) * updateTime * 5), Is.EqualTo(server.GameObjectsTable[playerId].Position));
        }
    }
}

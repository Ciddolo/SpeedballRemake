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
        public void MaliciusInput()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet(PacketsCommands.Input,false, (byte)InputType.Tackle).GetData();
            packet.endPoint = secondClient;

            transport.ClientEnqueue(packet);

            Assert.That(() => server.SingleStep(), Throws.Nothing);
        }

        [Test]
        public void DefaultControlledPlayer()
        {
            byte[] welcomePacketData = transport.ClientDequeue().data;
            uint playerId = BitConverter.ToUInt32(welcomePacketData, 9);
            Assert.That(playerId, Is.EqualTo(server.GetClientControlledPlayer(firstClient)));
        }

        [Test]
        public void ControlledPlayerAfterSelectPlayerGameNotStarted()
        {
            byte[] welcomePacketData = transport.ClientDequeue().data;
            uint playerId = BitConverter.ToUInt32(welcomePacketData, 9);

            Packet SelectPlayerPacket = new Packet(PacketsCommands.Input, false, (byte)InputType.SelectPlayer, playerId + 1);
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

            Packet SelectPlayerPacket = new Packet(PacketsCommands.Input, false, (byte)InputType.SelectPlayer, playerId + 1);
            FakeData fakePack = new FakeData();
            fakePack.data = SelectPlayerPacket.GetData();
            fakePack.endPoint = firstClient;
            transport.ClientEnqueue(fakePack);
            server.SingleStep();

            Assert.That(playerId, Is.Not.EqualTo(server.GetClientControlledPlayer(firstClient)));
        }

        [Test]
        public void ControlledPlayerAfterSelectPlayerGameStartedBallTaken()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet(PacketsCommands.Join).GetData();
            packet.endPoint = secondClient;
            transport.ClientEnqueue(packet);


            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            byte[] welcomePacketData = transport.ClientDequeue().data;
            uint playerId = BitConverter.ToUInt32(welcomePacketData, 9);
            server.GameLogic.Ball.Position = server.GameObjectsTable[playerId].Position;

            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            Assert.That(server.GameLogic.Ball.PlayerWhoOwnsTheBall.Id, Is.EqualTo(playerId));

            Packet SelectPlayerPacket = new Packet(PacketsCommands.Input, false, (byte)InputType.SelectPlayer, playerId + 1);
            FakeData fakePack = new FakeData();
            fakePack.data = SelectPlayerPacket.GetData();
            fakePack.endPoint = firstClient;
            transport.ClientEnqueue(fakePack);
            server.SingleStep();

            Assert.That(playerId, Is.EqualTo(server.GetClientControlledPlayer(firstClient)));
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

            Assert.That(startPos + (new Vector2(updateTime* 6.5f, 0f)), Is.EqualTo(server.GameObjectsTable[playerId].Position));
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
            Assert.That(Vector2.Zero, Is.EqualTo(server.GameObjectsTable[playerId].RigidBody.Velocity));
        }

        [Test]
        public void ShotBallGameStarted()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet(PacketsCommands.Join).GetData();
            packet.endPoint = secondClient;
            transport.ClientEnqueue(packet);

            server.GetBall().Position = new Vector2(-1,0);
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            byte[] welcomePacketData = transport.ClientDequeue().data;
            uint playerId = BitConverter.ToUInt32(welcomePacketData, 9);
            Vector2 startPos = (server.GameObjectsTable[playerId].Position);

            Console.WriteLine(server.GetBall().Position);

            Packet shotPacket = new Packet(PacketsCommands.Input, false, (byte)InputType.Shot, -1f, 0f,10f);
            FakeData fakePack = new FakeData();
            fakePack.data = shotPacket.GetData();
            fakePack.endPoint = firstClient;

            transport.ClientEnqueue(fakePack);
            server.SingleStep();

            float updateTime = 1f;

            clock.IncreaseTimeStamp(updateTime);
            server.SingleStep();

            Assert.That(server.GetBall().Position, Is.EqualTo(new Vector2(-11-3,0)));
        }

        [Test]
        public void ShotBallMagnificationGameStarted()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet(PacketsCommands.Join).GetData();
            packet.endPoint = secondClient;
            transport.ClientEnqueue(packet);

            server.GetBall().Position = new Vector2(-1, 0);
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            byte[] welcomePacketData = transport.ClientDequeue().data;
            uint playerId = BitConverter.ToUInt32(welcomePacketData, 9);
            Vector2 startPos = (server.GameObjectsTable[playerId].Position);

            Console.WriteLine(server.GetBall().Position);

            Packet shotPacket = new Packet(PacketsCommands.Input, false, (byte)InputType.Shot, -1f, 0f, 10f);
            FakeData fakePack = new FakeData();
            fakePack.data = shotPacket.GetData();
            fakePack.endPoint = firstClient;

            transport.ClientEnqueue(fakePack);
            server.SingleStep();

            float updateTime = 1f;

            clock.IncreaseTimeStamp(updateTime);
            server.SingleStep();

            Assert.That(server.GetBall().Magnification, Is.EqualTo(0f));
        }

        [Test]
        public void ShotBallMagnificationInAirGameStarted()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet(PacketsCommands.Join).GetData();
            packet.endPoint = secondClient;
            transport.ClientEnqueue(packet);

            server.GetBall().Position = new Vector2(-1, 0);
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            byte[] welcomePacketData = transport.ClientDequeue().data;
            uint playerId = BitConverter.ToUInt32(welcomePacketData, 9);
            Vector2 startPos = (server.GameObjectsTable[playerId].Position);

            Console.WriteLine(server.GetBall().Position);

            Packet shotPacket = new Packet(PacketsCommands.Input, false, (byte)InputType.Shot, -1f, 0f, 20f);
            FakeData fakePack = new FakeData();
            fakePack.data = shotPacket.GetData();
            fakePack.endPoint = firstClient;

            transport.ClientEnqueue(fakePack);
            server.SingleStep();

            float updateTime = 1f;

            clock.IncreaseTimeStamp(updateTime);
            server.SingleStep();

            Assert.That(server.GetBall().Magnification, Is.EqualTo(6/10f));
        }

        [Test]
        public void BallInAirNotCatchable()
        {
            FakeData packet = new FakeData();
            packet.data = new Packet(PacketsCommands.Join).GetData();
            packet.endPoint = secondClient;
            transport.ClientEnqueue(packet);

            server.GetBall().Position = new Vector2(-1, 0);
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            byte[] welcomePacketData = transport.ClientDequeue().data;
            uint playerId = BitConverter.ToUInt32(welcomePacketData, 9);
            Vector2 startPos = (server.GameObjectsTable[playerId].Position);

            Console.WriteLine(server.GetBall().Position);

            Packet shotPacket = new Packet(PacketsCommands.Input, false, (byte)InputType.Shot, -1f, 0f, 20f);
            FakeData fakePack = new FakeData();
            fakePack.data = shotPacket.GetData();
            fakePack.endPoint = firstClient;

            transport.ClientEnqueue(fakePack);
            server.SingleStep();

            server.GetBall().Position = new Vector2(-1, 0);
            float updateTime = 1f;

            clock.IncreaseTimeStamp(updateTime);
            server.SingleStep();

            Assert.That(server.GetBall().PlayerWhoOwnsTheBall, Is.EqualTo(null));
        }

        [Test]
        public void UnsuccessfullTackle()
        {
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            byte[] welcomePacket = (transport.ClientDequeue()).data;

            uint playerObjectId = BitConverter.ToUInt32(welcomePacket, 9);

            Player myPlayer = (Player)server.GameObjectsTable[playerObjectId];
            Player opponent = (Player)server.GameObjectsTable[playerObjectId + 2];
            myPlayer.Position = new Vector2(1, 1);

            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            Assert.That(server.GetBall().PlayerWhoOwnsTheBall, Is.EqualTo(null));

            myPlayer.Position = new Vector2(500f, 500f);
            server.GetBall().SetPosition(1f, 1f);

            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            Assert.That(server.GetBall().PlayerWhoOwnsTheBall, Is.EqualTo(opponent));

            FakeData packet = new FakeData();
            packet.data = new Packet(PacketsCommands.Input, false, (byte)InputType.Tackle).GetData();
            packet.endPoint = firstClient;

            transport.ClientEnqueue(packet);
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            Assert.That(server.GetBall().PlayerWhoOwnsTheBall, Is.EqualTo(opponent));
        }

        [Test]
        public void SuccessfullTackle()
        {
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            byte[] welcomePacket = (transport.ClientDequeue()).data;

            uint playerObjectId = BitConverter.ToUInt32(welcomePacket, 9);

            Player myPlayer = (Player)server.GameObjectsTable[playerObjectId];
            Player opponent = (Player)server.GameObjectsTable[playerObjectId + 2];

            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            Assert.That(server.GetBall().PlayerWhoOwnsTheBall, Is.EqualTo(null));
            //Assert.That(myPlayer.ColliderPlayer, Is.EqualTo(opponent));

            server.GetBall().SetPosition(opponent.Position);

            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            Assert.That(server.GetBall().PlayerWhoOwnsTheBall, Is.EqualTo(opponent));

            FakeData packet = new FakeData();
            packet.data = new Packet(PacketsCommands.Input, false, (byte)InputType.Tackle).GetData();
            packet.endPoint = firstClient;

            myPlayer.Position = new Vector2(1f, 1f);

            transport.ClientEnqueue(packet);
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            Assert.That(server.GetBall().PlayerWhoOwnsTheBall, Is.EqualTo(myPlayer));
        }

        [Test]
        public void SuccessfullTackleNotStartingIdle()
        {
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            byte[] welcomePacket = (transport.ClientDequeue()).data;

            uint playerObjectId = BitConverter.ToUInt32(welcomePacket, 9);

            Player myPlayer = (Player)server.GameObjectsTable[playerObjectId];
            Player opponent = (Player)server.GameObjectsTable[playerObjectId + 2];

            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            Assert.That(server.GetBall().PlayerWhoOwnsTheBall, Is.EqualTo(null));
            //Assert.That(myPlayer.ColliderPlayer, Is.EqualTo(opponent));

            server.GetBall().SetPosition(opponent.Position);

            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            Assert.That(server.GetBall().PlayerWhoOwnsTheBall, Is.EqualTo(opponent));

            myPlayer.GetStunned();

            FakeData packet = new FakeData();
            packet.data = new Packet(PacketsCommands.Input, false, (byte)InputType.Tackle).GetData();
            packet.endPoint = firstClient;

            myPlayer.Position = new Vector2(1f, 1f);

            transport.ClientEnqueue(packet);
            clock.IncreaseTimeStamp(1f);
            server.SingleStep();

            Assert.That(server.GetBall().PlayerWhoOwnsTheBall, Is.EqualTo(opponent));
        }
    }
}
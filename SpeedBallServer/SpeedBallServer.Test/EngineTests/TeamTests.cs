using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using NUnit.Framework;

namespace SpeedBallServer.Test.EngineTests
{
    class TeamTests
    {
        private GameServer server;
        private FakeClock clock;
        private FakeTransport transport;
        private FakeEndPoint firstClientEndPoint, secondClientEndPoint;

        [SetUp]
        public void SetUpTest()
        {
            transport = new FakeTransport();
            clock = new FakeClock();
            server = new GameServer(transport, clock);

            transport.Bind("127.0.0.1", 5000);
            firstClientEndPoint = new FakeEndPoint("192.168.1.1", 5001);
            secondClientEndPoint = new FakeEndPoint("192.168.1.2", 5002);

            FakeData packet = new FakeData();
            packet.data = new Packet(PacketsCommands.Join).GetData();
            packet.endPoint = firstClientEndPoint;

            transport.ClientEnqueue(packet);

            server.SingleStep();

            packet = new FakeData();
            packet.data = new Packet(PacketsCommands.Join).GetData();
            packet.endPoint = secondClientEndPoint;
            transport.ClientEnqueue(packet);

            //read second client join
            server.SingleStep();
        }

        [Test]
        public void ClientOwnsTeam()
        {
            GameLogic gameLogic = server.GameLogic;

            Assert.That(gameLogic.Teams[0].HasOwner, Is.EqualTo(true));
            Assert.That(gameLogic.Teams[0].Owner.EndPoint, Is.EqualTo(firstClientEndPoint));
            Assert.That(gameLogic.Teams[1].Owner.EndPoint, Is.EqualTo(secondClientEndPoint));
        }

        [Test]
        public void AddPlayer()
        {
            GameLogic gameLogic = server.GameLogic;
            Player newPlayer = new Player(null,1,1);
            Team firstTeam = gameLogic.Teams[0];
            firstTeam.AddPlayer(newPlayer);

            Assert.That(firstTeam.ControllablePlayers.Count(), Is.EqualTo(3));          
            Assert.That(newPlayer.Owner.EndPoint, Is.EqualTo(firstClientEndPoint));          
        }

        [Test]
        public void ClientOwnsControlledPlayer()
        {
            GameLogic gameLogic = server.GameLogic;
            Team firstTeam = gameLogic.Teams[0];

            Assert.That(firstTeam.ControlledPlayer.Owner.EndPoint, Is.EqualTo(firstClientEndPoint));
            Assert.That(firstTeam.ControlledPlayerId, Is.EqualTo(firstTeam.ControlledPlayer.Id));
        }

        [Test]
        public void Reset()
        {
            GameLogic gameLogic = server.GameLogic;
            Team firstTeam = gameLogic.Teams[0];

            firstTeam.Reset();

            Assert.That(firstTeam.Owner, Is.EqualTo(null));
            Assert.That(firstTeam.ControlledPlayer.Owner, Is.EqualTo(null));
        }
    }
}

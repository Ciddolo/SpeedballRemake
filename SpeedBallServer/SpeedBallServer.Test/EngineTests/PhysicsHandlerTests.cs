using System;
using System.Numerics;
using NUnit.Framework;

namespace SpeedBallServer.Test.EngineTests
{
    public class PhysicsHandlerTest
    {
        private Player myPlayer;
        private Player myOpponent;
        private Player myTeammate;
        private Goalkeeper myGoalkeeper;
        private Ball myBall;
        private Obstacle obstacle;
        private PhysicsHandler physicsHandler;

        [SetUp]
        public void SetUpTest()
        {
            myPlayer = new Player(null, 1, 1);
            myPlayer.TeamId = 0;

            myTeammate = new Player(null, 1, 1);
            myTeammate.TeamId = 0;

            myOpponent = new Player(null, 1, 1);
            myOpponent.TeamId = 1;

            myGoalkeeper = new Goalkeeper(null, 1, 1);
            myGoalkeeper.TeamId = 0;
            myGoalkeeper.Ball = myBall;

            obstacle = new Obstacle(null, 10, 10);
            myBall = new Ball(null, 1, 1);
            myBall.Position = new Vector2(200f, 200f);

            physicsHandler = new PhysicsHandler();
            physicsHandler.AddItem(myPlayer.RigidBody);
            physicsHandler.AddItem(myTeammate.RigidBody);
            physicsHandler.AddItem(myOpponent.RigidBody);
            physicsHandler.AddItem(obstacle.RigidBody);
            physicsHandler.AddItem(myBall.RigidBody);
            physicsHandler.AddItem(myGoalkeeper.RigidBody);

            physicsHandler.CheckCollisions();
        }

        [Test]
        public void PlayerCollidingObstacle()
        {
            Assert.That(myPlayer.Position, Is.Not.EqualTo(Vector2.Zero));
        }

        [Test]
        public void SuccessfulCollisionBallKnowsPlayer()
        {
            myPlayer.Position = new Vector2(50, 50);
            myBall.Position = new Vector2(50, 50);
            physicsHandler.CheckCollisions();
            Assert.That(myBall.PlayerWhoOwnsTheBall, Is.EqualTo(myPlayer));
        }

        [Test]
        public void SuccessfulCollisionPlayerKnowsBall()
        {
            myPlayer.Position = new Vector2(50, 50);
            myBall.Position = new Vector2(50, 50);
            physicsHandler.CheckCollisions();
            Assert.That(myBall, Is.EqualTo(myPlayer.Ball));
        }

        [Test]
        public void SuccessfulCollisionTacklingTakingBall()
        {
            myOpponent.Position = new Vector2(50, 50);
            myBall.Position = new Vector2(50, 50);
            physicsHandler.CheckCollisions();

            myPlayer.Position = myOpponent.Position;
            myPlayer.StartTackling();
            physicsHandler.CheckCollisions();

            Assert.That(myOpponent.State, Is.EqualTo(PlayerState.Stunned));
            Assert.That(myPlayer.State, Is.EqualTo(PlayerState.Idle));
        }

        [Test]
        public void SuccessfulCollisionTacklingStunOpponent()
        {
            myOpponent.Position = new Vector2(50, 50);
            physicsHandler.CheckCollisions();

            myPlayer.Position = myOpponent.Position;
            myPlayer.StartTackling();
            physicsHandler.CheckCollisions();

            Assert.That(myOpponent.State, Is.EqualTo(PlayerState.Stunned));
            Assert.That(myPlayer.State, Is.EqualTo(PlayerState.Tackling));
        }

        [Test]
        public void UsuccessfulCollisionTacklingOpponentTooFar()
        {
            myOpponent.Position = new Vector2(50, 50);
            physicsHandler.CheckCollisions();

            myPlayer.StartTackling();
            physicsHandler.CheckCollisions();

            Assert.That(myPlayer.State, Is.EqualTo(PlayerState.Tackling));
            Assert.That(myOpponent.State, Is.EqualTo(PlayerState.Idle));
        }

        [Test]
        public void SuccessfulCollisionTacklingTeammate()
        {
            myTeammate.Position = new Vector2(50, 50);
            myBall.Position = new Vector2(50, 50);
            physicsHandler.CheckCollisions();

            myPlayer.Position = myTeammate.Position;
            myPlayer.StartTackling();
            physicsHandler.CheckCollisions();

            Assert.That(myPlayer.State, Is.EqualTo(PlayerState.Tackling));
            Assert.That(myTeammate.State, Is.EqualTo(PlayerState.Idle));
        }

        [Test]
        public void SuccessfulCollisionBetweenPlayersWhoIsPushing()
        {
            myPlayer.Position = new Vector2(10, 10);
            myPlayer.RigidBody.Velocity = new Vector2(10, 10);

            myOpponent.Position = new Vector2(10, 10);

            physicsHandler.CheckCollisions();

            Assert.That(myOpponent.Position, Is.EqualTo(new Vector2(10, 10)));
            Assert.That(myOpponent.RigidBody.Velocity, Is.EqualTo(Vector2.Zero));
            Assert.That(myPlayer.Position, Is.Not.EqualTo(new Vector2(10, 10)));
        }

        [Test]
        public void SuccessfulCollisionBetweenPlayersWhoIsPushingSameTeam()
        {
            myPlayer.Position = new Vector2(10, 10);
            myPlayer.RigidBody.Velocity = new Vector2(10, 10);

            myTeammate.Position = new Vector2(10, 10);

            physicsHandler.CheckCollisions();

            Assert.That(myTeammate.Position, Is.EqualTo(new Vector2(10, 10)));
            Assert.That(myTeammate.RigidBody.Velocity, Is.Not.EqualTo(Vector2.Zero));
            Assert.That(myPlayer.Position, Is.Not.EqualTo(new Vector2(10, 10)));
        }

        [Test]
        public void SuccessfulCollisionBetweenPlayerGoalkeeper()
        {
            myPlayer.Position = new Vector2(10, 10);

            myGoalkeeper.Position = new Vector2(10, 10);

            physicsHandler.CheckCollisions();

            Assert.That(myGoalkeeper.Position, Is.EqualTo(new Vector2(10, 10)));
            Assert.That(myPlayer.Position, Is.Not.EqualTo(new Vector2(10, 10)));
        }

        [Test]
        public void SuccessfulCollisionBetweenPlayerWithBallGoalkeeper()
        {
            Player myPlayer = new Player(null, 1, 1);
            Ball myBall = new Ball(null, 1, 1);
            Goalkeeper myGoalkeeper = new Goalkeeper(null, 1, 1);
            myGoalkeeper.Ball = myBall;

            PhysicsHandler physicsHandler = new PhysicsHandler();

            physicsHandler.AddItem(myGoalkeeper.RigidBody);
            physicsHandler.AddItem(myPlayer.RigidBody);
            physicsHandler.AddItem(myBall.RigidBody);

            myPlayer.Position = new Vector2(100, 100);
            myGoalkeeper.TeamId = 1;
            myBall.Position = new Vector2(100, 100);

            physicsHandler.CheckCollisions();

            myGoalkeeper.Position = new Vector2(100, 100);

            physicsHandler.CheckCollisions();

            Assert.That(myBall.RigidBody.Velocity, Is.EqualTo(new Vector2(0f, -12.5f)));
            Assert.That(myGoalkeeper.Position, Is.EqualTo(new Vector2(100, 100)));
        }

        [Test]
        public void SuccessfulCollisionBetweenBallGoalkeeper()
        {
            Ball myBall = new Ball(null, 1, 1);
            Goalkeeper myGoalkeeper = new Goalkeeper(null, 1, 1);
            myGoalkeeper.Ball = myBall;

            PhysicsHandler physicsHandler = new PhysicsHandler();

            physicsHandler.AddItem(myGoalkeeper.RigidBody);
            physicsHandler.AddItem(myBall.RigidBody);

            myBall.Position = new Vector2(100, 100);

            physicsHandler.CheckCollisions();

            myGoalkeeper.Position = new Vector2(100, 100);

            physicsHandler.CheckCollisions();

            Assert.That(myBall.RigidBody.Velocity, Is.EqualTo(new Vector2(0f, -12.5f)));
            Assert.That(myGoalkeeper.Position, Is.EqualTo(new Vector2(100, 100)));
        }
    }
}

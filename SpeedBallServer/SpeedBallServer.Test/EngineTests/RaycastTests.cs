using System;
using NUnit.Framework;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer.Test.ServerTests
{
    class RaycastTests
    {
        [Test]
        public void RaycastObstacle()
        {
            Obstacle obstacle = new Obstacle(null, 5, 5);
            Player myPlayer = new Player(null, 1, 1);

            obstacle.Position = new Vector2(10, 0);
            myPlayer.Position = new Vector2(0, 0);

            PhysicsHandler physicsHandler = new PhysicsHandler();

            physicsHandler.AddItem(obstacle.RigidBody);
            physicsHandler.AddItem(myPlayer.RigidBody);

            PhysicsHandler.RaycastHitInfo collider = physicsHandler.RayCast(myPlayer.Position, new Vector2(1, 0), myPlayer.RigidBody);
            Assert.That(collider.Collider, Is.EqualTo(obstacle.RigidBody));
            Assert.That(collider.Distance, Is.EqualTo(7.5f));
            Assert.That(collider.Point, Is.EqualTo(new Vector2(7.5f, 0f)));
        }

        [Test]
        public void RaycastObstacleWithMask()
        {
            Obstacle obstacle = new Obstacle(null, 5, 5);
            Player myPlayer = new Player(null, 1, 1);
            Player myTeammate = new Player(null, 1, 1);

            obstacle.Position = new Vector2(10, 0);
            myTeammate.Position = new Vector2(5, 0);
            myPlayer.Position = new Vector2(0, 0);

            PhysicsHandler physicsHandler = new PhysicsHandler();

            physicsHandler.AddItem(obstacle.RigidBody);
            physicsHandler.AddItem(myPlayer.RigidBody);
            physicsHandler.AddItem(myTeammate.RigidBody);

            PhysicsHandler.RaycastHitInfo collider = physicsHandler.RayCast(myPlayer.Position, new Vector2(1, 0), myPlayer.RigidBody, (uint)ColliderType.Obstacle);
            Assert.That(collider.Collider, Is.EqualTo(obstacle.RigidBody));
            Assert.That(collider.Distance, Is.EqualTo(7.5f));
            Assert.That(collider.Point, Is.EqualTo(new Vector2(7.5f, 0f)));
        }

        [Test]
        public void RaycastTeammate()
        {
            Obstacle obstacle = new Obstacle(null, 5, 5);
            Player myPlayer = new Player(null, 1, 1);
            Player myTeammate = new Player(null, 1, 1);

            obstacle.Position = new Vector2(10, 0);
            myTeammate.Position = new Vector2(5, 0);
            myPlayer.Position = new Vector2(1, 0);

            PhysicsHandler physicsHandler = new PhysicsHandler();

            physicsHandler.AddItem(obstacle.RigidBody);
            physicsHandler.AddItem(myPlayer.RigidBody);
            physicsHandler.AddItem(myTeammate.RigidBody);

            PhysicsHandler.RaycastHitInfo collider = physicsHandler.RayCast(myPlayer.Position, new Vector2(1, 0), myPlayer.RigidBody);
            Assert.That(collider.Collider, Is.EqualTo(myTeammate.RigidBody));
            Assert.That(collider.Distance, Is.EqualTo(3.5f));
            Assert.That(collider.Point, Is.EqualTo(new Vector2(4.5f, 0f)));
        }

        [Test]
        public void RaycastNothing()
        {
            Player myPlayer = new Player(null, 1, 1);

            myPlayer.Position = new Vector2(0, 0);

            PhysicsHandler physicsHandler = new PhysicsHandler();

            physicsHandler.AddItem(myPlayer.RigidBody);

            PhysicsHandler.RaycastHitInfo collider = physicsHandler.RayCast(myPlayer.Position, new Vector2(1, 0), myPlayer.RigidBody);
            Assert.That(collider.Collider, Is.EqualTo(null));
            Assert.That(collider.Distance, Is.EqualTo(float.MaxValue));
            Assert.That(collider.Point, Is.EqualTo(new Vector2(0f, 0f)));
        }

        [Test]
        public void RaycastNothingWithMask()
        {
            Obstacle obstacle = new Obstacle(null, 5, 5);
            Player myPlayer = new Player(null, 1, 1);
            Player myTeammate = new Player(null, 1, 1);

            obstacle.Position = new Vector2(10, 0);
            myTeammate.Position = new Vector2(5, 0);
            myPlayer.Position = new Vector2(1, 0);

            PhysicsHandler physicsHandler = new PhysicsHandler();

            physicsHandler.AddItem(obstacle.RigidBody);
            physicsHandler.AddItem(myPlayer.RigidBody);
            physicsHandler.AddItem(myTeammate.RigidBody);

            PhysicsHandler.RaycastHitInfo collider = physicsHandler.RayCast(myPlayer.Position, new Vector2(1, 0), myPlayer.RigidBody, 0);
            Assert.That(collider.Collider, Is.EqualTo(null));
        }

        [Test]
        public void RaycastNothingDiagonal()
        {
            Obstacle obstacle = new Obstacle(null, 5, 5);
            Player myPlayer = new Player(null, 1, 1);
            Player myTeammate = new Player(null, 1, 1);

            obstacle.Position = new Vector2(10, 0);
            myTeammate.Position = new Vector2(5, 0);
            myPlayer.Position = new Vector2(1, 0);

            PhysicsHandler physicsHandler = new PhysicsHandler();

            physicsHandler.AddItem(obstacle.RigidBody);
            physicsHandler.AddItem(myPlayer.RigidBody);
            physicsHandler.AddItem(myTeammate.RigidBody);

            PhysicsHandler.RaycastHitInfo collider = physicsHandler.RayCast(myPlayer.Position, Vector2.Normalize(new Vector2(1, 1)), myPlayer.RigidBody);
            Assert.That(collider.Collider, Is.EqualTo(null));
        }

        [Test]
        public void RaycastDiagonal()
        {
            Obstacle obstacle = new Obstacle(null, 5, 5);
            Player myPlayer = new Player(null, 1, 1);
            Player myTeammate = new Player(null, 1, 1);

            obstacle.Position = new Vector2(10, 1);
            myPlayer.Position = new Vector2(5, 0);

            PhysicsHandler physicsHandler = new PhysicsHandler();

            physicsHandler.AddItem(obstacle.RigidBody);
            physicsHandler.AddItem(myPlayer.RigidBody);
            physicsHandler.AddItem(myTeammate.RigidBody);

            PhysicsHandler.RaycastHitInfo collider = physicsHandler.RayCast(myPlayer.Position, Vector2.Normalize(new Vector2(1, 1)), myPlayer.RigidBody);
            Assert.That(collider.Collider, Is.EqualTo(obstacle.RigidBody));
            Assert.That(collider.Distance, Is.InRange(3.5f, 3.6f));
        }
    }
}

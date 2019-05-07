using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer
{
    public enum InternalObjectsId
    {
        Player=1,
        Obstacle,
        Net
    }

    public class GameObject
    {
        public Vector2 Position;

        public float X { get { return Position.X; } }
        public float Y { get { return Position.Y; } }

        public bool IsActive { get; set; }
        public float Width, Height;

        protected GameClient owner;
        protected GameServer server;

        public GameClient Owner
        {
            get { return owner; }
        }

        public bool IsOwnedBy(GameClient client)
        {
            return owner == client;
        }

        public void SetOwner(GameClient client)
        {
            owner = client;
        }

        private uint internalObjectType;
        public uint ObjectType
        {
            get
            {
                return internalObjectType;
            }
        }

        private static uint gameObjectCounter;
        private uint internalId;
        public uint Id
        {
            get
            {
                return internalId;
            }
        }

        public GameObject(uint objectType, GameServer server, float Height, float Width)
        {
            this.Height = Height;
            this.Width = Width;

            internalObjectType = objectType;
            IsActive = true;

            internalId = ++gameObjectCounter;
            this.server = server;

            Console.WriteLine("spawned GameObject {0} of type {1}", Id, ObjectType);
        }

        public void SetPosition(float x, float y)
        {
            SetPosition(new Vector2(x, y));
        }

        public void SetPosition(Vector2 NewPos)
        {
            this.Position = NewPos;
        }

        public virtual void Update()
        {

        }

        public virtual void Tick()
        {

        }

        public virtual void Destroy()
        {

        }

    }
}

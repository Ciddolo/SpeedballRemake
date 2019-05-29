using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer
{
    public class PhysicsHandler
    {
        List<RigidBody> items;
        static Collision collisionInfo;
        List<RigidBody> itemsToRemove;

        public PhysicsHandler()
        {
            items = new List<RigidBody>();
            itemsToRemove = new List<RigidBody>();
        }

        public void AddItem(RigidBody item)
        {
            items.Add(item);
        }

        public void RemoveItem(RigidBody item)
        {
            itemsToRemove.Add(item);
        }

        public void RemoveAll()
        {
            items.Clear();
        }

        private void DeleteItemsToRemove()
        {
            if (itemsToRemove.Count > 0)
            {
                for (int i = 0; i < itemsToRemove.Count; i++)
                {
                    items.Remove(itemsToRemove[i]);
                }
                itemsToRemove.Clear();
            }

        }

        public void Update(float deltaTime)
        {
            DeleteItemsToRemove();

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].GameObject.IsActive)
                    items[i].Update(deltaTime);
            }
        }

        public void CheckCollisions()
        {
            for (int i = 0; i < items.Count - 1; i++)
            {
                if (items[i].GameObject.IsActive && items[i].IsCollisionsAffected)
                {
                    for (int j = i + 1; j < items.Count; j++)
                    {
                        if (items[j].GameObject.IsActive && items[j].IsCollisionsAffected)
                        {
                            bool checkFirst = items[i].CheckCollisionWith(items[j]);
                            bool checkSecond = items[j].CheckCollisionWith(items[i]);

                            if ((checkFirst || checkSecond) && items[i].Collides(items[j], ref collisionInfo))
                            {
                                if (checkFirst)
                                {
                                    collisionInfo.Collider = items[j].GameObject;
                                    items[i].GameObject.OnCollide(collisionInfo);
                                }
                                if (checkSecond)
                                {
                                    collisionInfo.Collider = items[i].GameObject;
                                    items[j].GameObject.OnCollide(collisionInfo);
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}

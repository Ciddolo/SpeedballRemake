using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedBallServer
{
    public class PhysicsHandler
    {
        static Collision collisionInfo;
        List<RigidBody> items;
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

        private float[] ConvertVector2toArray(Vector2 vec)
        {
            float[] res = new float[2];
            res[0] = vec.X;
            res[1] = vec.Y;
            return res;
        }

        public struct RaycastHitInfo
        {
            public RigidBody Collider;
            public float Distance;
            public Vector2 Point;
        }

        const int DIMENSIONS = 2;
        const int RIGHT = 0;
        const int LEFT = 1;
        const int MIDDLE = 2;

        public RaycastHitInfo? RectRayIntersection(Vector2 bottomLeft, Vector2 topRight, Vector2 origin, Vector2 direction)
        {
            float[] bottomLeftCoords, topRightCoords, candidatePlane, maxT, originCoords, dirCoords, coordinates;
            int[] quadrant;
            bool inside;

            bottomLeftCoords = ConvertVector2toArray(bottomLeft);
            topRightCoords = ConvertVector2toArray(topRight);
            originCoords = ConvertVector2toArray(origin);
            dirCoords = ConvertVector2toArray(direction);

            candidatePlane = new float[DIMENSIONS];
            coordinates = new float[DIMENSIONS];
            maxT = new float[DIMENSIONS];
            quadrant = new int[DIMENSIONS];
            inside = true;

            /* Find candidate planes; this loop can be avoided if
            rays cast all from the eye(assume perpsective view) */

            for (int i = 0; i < DIMENSIONS; i++)
            {
                if (originCoords[i] < bottomLeftCoords[i])
                {
                    quadrant[i] = LEFT;
                    candidatePlane[i] = bottomLeftCoords[i];
                    inside = false;
                }
                else if (originCoords[i] > topRightCoords[i])
                {
                    quadrant[i] = RIGHT;
                    candidatePlane[i] = topRightCoords[i];
                    inside = false;
                }
                else
                {
                    quadrant[i] = MIDDLE;
                }
            }

            // Ray origin inside bounding box 
            if (inside)
            {
                return null;
            }

            // Calculate T distances to candidate planes 
            for (int i = 0; i < DIMENSIONS; i++)
            {
                if (quadrant[i] != MIDDLE && dirCoords[i] != 0)
                    maxT[i] = (candidatePlane[i] - originCoords[i]) / dirCoords[i];
                else
                    maxT[i] = -1f;
            }

            int whichPlane = 0;

            for (int i = 1; i < DIMENSIONS; i++)
            {
                if (maxT[whichPlane] < maxT[i])
                    whichPlane = i;
            }

            // Check final candidate actually inside box 
            if (maxT[whichPlane] < 0)
                return (null);

            for (int i = 0; i < DIMENSIONS; i++)
            {
                if (whichPlane != i)
                {

                    coordinates[i] = originCoords[i] + maxT[whichPlane] * dirCoords[i];
                    if (coordinates[i] < bottomLeftCoords[i] || coordinates[i] > topRightCoords[i])
                        return (null);
                }
                else
                {
                    coordinates[i] = candidatePlane[i];

                }
            }

            RaycastHitInfo raycastHit = new RaycastHitInfo();
            raycastHit.Point = new Vector2(coordinates[0], coordinates[1]);
            raycastHit.Distance = (origin - raycastHit.Point).Length();
            return raycastHit;
        }

        public RaycastHitInfo RayCast(Vector2 origin, Vector2 direction, RigidBody mySelf, uint raycastMask = 31)
        {
            RaycastHitInfo raycastHit = new RaycastHitInfo();
            raycastHit.Distance = float.MaxValue;
            raycastHit.Collider = null;
            raycastHit.Point = Vector2.Zero;

            foreach (RigidBody collider in items)
            {
                if (collider == mySelf || (raycastMask & collider.Type) == 0)
                    continue;

                RaycastHitInfo? p = RectRayIntersection(collider.BoundingBox.BottomLeft, collider.BoundingBox.TopRight, origin, direction);

                Console.WriteLine(collider.BoundingBox.BottomLeft);
                Console.WriteLine(collider.BoundingBox.TopRight);

                if (p != null)
                {
                    if (p.Value.Distance < raycastHit.Distance)
                    {
                        raycastHit = p.Value;
                        raycastHit.Collider = collider;
                    }
                }
            }
            return raycastHit;
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

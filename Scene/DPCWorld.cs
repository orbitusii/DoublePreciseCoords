/*

CVW-5 Prototype 2: Body Wrapper Hub
Author: Dan Lodholm (github: orbitusii)
Copyright: 2022

*/

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DoublePreciseCoords
{
    /// <summary>
    /// Double-Precision Coordinate Field - the main component required in a scene to enable DCPS behavior.
    /// </summary>
    [DefaultExecutionOrder(200)]
    public class DPCWorld : MonoBehaviour
    {
        public static DPCWorld Singleton { get; protected set; }

        public static List<DPCObject> AllBodies { get; protected set; } = new List<DPCObject>();

        [Tooltip("The absolute size of the physics area. Maximum positions are 1/2 of the size.")]
        public Vector2 PhysicsAreaSize = Vector2.one * 4000;
        public float GroupSpacing = 3;

        private static float dt;

        public delegate void BodiesChangedCallback();
        public static BodiesChangedCallback OnBodiesChanged;

        [field: SerializeField]
        public bool WrapSpace { get; protected set; } = true;

        public static bool Exists ()
        {
            if(Singleton == null)
            {
                DPCWorld foundDCW = FindObjectOfType(typeof(DPCWorld)) as DPCWorld;

                if (foundDCW == null)
                {
                    return false;
                }
                
                Singleton = foundDCW;
            }

            return true;
        }

        public static void Create ()
        {
            if (!Exists())
            {
                GameObject newWorld = new GameObject();
                newWorld.name = "DoubleCoordinateWorld";
                Singleton = newWorld.AddComponent(typeof(DPCWorld)) as DPCWorld;
            }
        }
        public static void Add(DPCObject body)
        {
            if (!AllBodies.Contains(body))
            {
                AllBodies.Add(body);
                
                //OnBodiesChanged();
            }
        }

        public static void Remove(DPCObject body)
        {
            AllBodies.Remove(body);

            //OnBodiesChanged();
        }

        protected virtual void FixedUpdate()
        {
            dt = Time.fixedDeltaTime;
            // Collect only objects that are moving (i.e. not marked kinematic)
            var movingObjectsQuery =
                from body in AllBodies
                where body.Interactable == true
                select body;
            List<DPCObject> movingObjects = movingObjectsQuery.ToList();

            // If we don't have any moving objects, don't bother continuing.
            if (movingObjects.Count() < 1) return;

            if (WrapSpace)
            {
                // This group won't be used for anything. It's just a container to start the recursion
                var dummyGroup = new CollisionGroup();

                dummyGroup.Bodies.AddRange(movingObjects);

                var Groups = GroupByAxis(dummyGroup, 0);
                Groups[0].RecomputeSize();

                Vector3 startCorner = new Vector3(PhysicsAreaSize.x, 0, PhysicsAreaSize.y) / -2;
                // Maximum depth (Z-axis) of any wrapper along the current row.
                // Used to prevent wrapper overlap and erroneous collisions.
                float maxDepth = 0;

                // Iterate through and place everything in the world, then sync transforms and simulate
                foreach (CollisionGroup group in Groups)
                {
                    bool hasNeighbors = group.Bodies.Count > 1;

                    // Place objects in the world
                    foreach (DPCObject body in group.Bodies)
                    {
                        Vector3 positionOffset = (Vector3)(body.Position - group.Start);
                        body.SetRawPosition(startCorner + positionOffset);
                    }

                    // After every object in a group has been placed down, then we can do custom
                    // physics things (projectiles do their raycasting here)
                    foreach (DPCObject body in group.Bodies)
                    {
                        body.OnCustomPhysics(hasNeighbors);
                    }

                    startCorner.x += (float)group.Size.x + GroupSpacing;
                    maxDepth = Mathf.Max(maxDepth, (float)group.Size.z);

                    if (startCorner.x >= PhysicsAreaSize.x / 2)
                    {
                        startCorner.x = PhysicsAreaSize.x / -2;
                        startCorner.z += maxDepth + GroupSpacing;

                        if (startCorner.z >= PhysicsAreaSize.y / 2)
                        {
                            Debug.LogError($"Physics Area has run out of room! " +
                                $"Consider expanding the PhysicsAreaSize value of this " +
                                $"WrapperHub", this);
                        }
                    }
                }

                Physics.SyncTransforms();
                Physics.Simulate(dt);

                foreach (var body in movingObjects)
                {
                    body.SyncPosition();
                }
            }
            else
            {
                foreach(var body in AllBodies)
                {
                    body.OnCustomPhysics(true);
                }

                Physics.SyncTransforms();
                Physics.Simulate(dt);

                foreach (var body in AllBodies)
                {
                    body.Position = body.transform.position;
                }
            }
        }

        protected void Update()
        {
            float sizeX = PhysicsAreaSize.x / 2;
            float sizeY = PhysicsAreaSize.y / 2;

            Vector3 LF = new Vector3(-sizeX,0 , -sizeY);
            Vector3 LR = new Vector3(-sizeX,0 , sizeY);

            Vector3 RF = new Vector3(sizeX, 0, -sizeY);
            Vector3 RR = new Vector3(sizeX, 0, sizeY);

            Debug.DrawLine(LF, LR);
            Debug.DrawLine(LF, RF);

            Debug.DrawLine(RR, LR);
            Debug.DrawLine(RR, RF);
        }

        private List<CollisionGroup> GroupByAxis(CollisionGroup group, int axis)
        {
            // If we're looking for an axis less than X or more than Z... something is wrong
            // OR if this group only contains one object, we don't need to recurse.
            // Just return the provided group
            if (axis < 0 || axis > 2 || group.Bodies.Count <= 1)
            {
                return new List<CollisionGroup> { group };
            }

            List<CollisionGroup> finalGroups = new List<CollisionGroup>();
            List<DPCObject> bodies = group.Bodies;

            // Sort by left-most bound on each object.
            bodies.Sort(delegate (DPCObject left, DPCObject right)
            {
                double leftMin = left.Position[axis] - left.BoundingRadius;
                double rightMin = right.Position[axis] - right.BoundingRadius;

                if (leftMin < rightMin) return -1;
                else return 1;
            });

            // Create the first group, which will always contain the first object.
            CollisionGroup current = new CollisionGroup(bodies[0]);

            for(int i = 1; i < bodies.Count; i++)
            {
                // Left and right limits for this body's bounds
                double iMin = bodies[i].Position[axis] - bodies[i].BoundingRadius;
                double iMax = bodies[i].Position[axis] + bodies[i].BoundingRadius;

                // This body is contained by the current group along this axis
                // i.e. body's left limit is less than the group's right limit
                if(iMin < current.End[axis])
                {
                    current.Bodies.Add(bodies[i]);
                    // Make the group's new right limit the greater value
                    current.End[axis] = current.End[axis] >= iMax ? current.End[axis] : iMax;
                }
                // This body is outside of the current group and should be put in its own new group
                else
                {
                    // Recurse through the current group to break it down further
                    var subGroups = GroupByAxis(current, axis+1);
                    finalGroups.AddRange(subGroups);

                    // Start the new group with this body
                    current = new CollisionGroup(bodies[i]);
                }
            }

            // Recurse through the last group, which will always contain the last object
            // The loop will not do this recursion on its own, so we have to call it here.
            var lastGroups = GroupByAxis(current, axis+1);
            finalGroups.AddRange(lastGroups);

            // This should be everything
            return finalGroups;
        }

        private void OnEnable()
        {
            if(Exists() && Singleton != this)
            {
                Debug.LogWarning("Multiple Wrapper Hubs exist!", this);
                gameObject.SetActive(false);
                return;
            }
            else
            {
                Singleton = this;
                Physics.autoSimulation = false;
                Physics.autoSyncTransforms = false;
            }
        }

        private void OnDisable()
        {
            if(Singleton == this)
            {
                Singleton = null;
                AllBodies.Clear();

                Physics.autoSimulation = true;
                Physics.autoSyncTransforms = true;
            }
        }
    }
}
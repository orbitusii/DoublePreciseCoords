using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DoublePreciseCoords
{
    public class DoubleCoordinateObject: MonoBehaviour
    {
        [Tooltip("This object's position... but in a 64-bit Vector format. " +
            "With a WrapperHub in the scene, this lets objects use a much larger " +
            "coordinate system in a useful capacity.")]
        public Vector64 Position;
        private Vector3 LastPhysicsPosition;

        [Tooltip("The bounding radius of this object, used by the WrapperHub to " +
            "determine what WrappedBodies are actually interacting.")]
        public float BoundingRadius = 1;

        [Tooltip("Whether or not this body will refresh its bounding radius during " +
            "OnEnable(). Set this to false if you need the BoundingRadius to be different " +
            "than what this object's colliders indicate it should be (e.g. missiles with " +
            "proximity fusing)")]
        public bool AutoRefreshBoundingRadius = true;

        public bool Interactable = true;
        public bool Viewable = true;

#pragma warning disable CS0108
        public Rigidbody rigidbody { get; protected set; }
#pragma warning restore

        public float Speed
        {
            get => rigidbody.velocity.magnitude;
        }

        public void SetPhysicsPosition (Vector3 posInUnity)
        {
            transform.position = posInUnity;
            transform.localScale = Vector3.one;

            LastPhysicsPosition = transform.position;
        }

        public void SyncPosition ()
        {
            Vector3 delta = transform.position - LastPhysicsPosition;
            MovePosition(delta);

        }

        public void MovePosition (Vector64 delta)
        {
            Position += delta;
        }

        protected void OnValidate ()
        {
            if(DoubleCoordinateWorld.Exists())
            {
                return;
            }

            DoubleCoordinateWorld.Create();
        }

        protected virtual void OnEnable ()
        {
            DoubleCoordinateWorld.Add(this);

            if(AutoRefreshBoundingRadius)
            {
                Bounds bounds = GatherBounds();

                BoundingRadius = bounds.extents.magnitude;
            }

            LastPhysicsPosition = transform.position;
        }

        private Bounds GatherBounds()
        {
            Bounds newBounds = new Bounds(transform.position, Vector3.zero);

            Collider[] colliders = GetComponentsInChildren<Collider>();

            foreach (Collider coll in colliders)
            {
                newBounds.Encapsulate(coll.bounds);
            }

            return newBounds;
        }

        protected virtual void OnDisable ()
        {
            DoubleCoordinateWorld.Remove(this);
        }

        protected virtual void OnDrawGizmos ()
        {
            Gizmos.color = Interactable ? Color.green : Color.gray;

            Gizmos.DrawWireSphere(transform.position, BoundingRadius);
        }
    }
}

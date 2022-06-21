/*

CVW-5 Prototype 2: Basic Wrapped Body
Author: Dan Lodholm (github: orbitusii)
Copyright: 2022

*/

using UnityEngine;

namespace DoublePreciseCoords
{
    /// <summary>
    /// An overarching class to handle Physics Wrapping and multi-floating-origin behavior.
    /// If you need rigidbody behavior, use DynamicBody. If you need motion, but not physics,
    /// use KinematicBody.
    /// </summary>
    public class WrappedBody : MonoBehaviour, ILargePosition
    {
        [Tooltip("This rigidbody's position... but in a 64-bit Vector format. " +
            "With a WrapperHub in the scene, this lets objects use a much larger " +
            "coordinate system in a useful capacity.")]
        public Vector64 Position;

        [Tooltip("The bounding radius of this object, used by the WrapperHub to " +
            "determine what WrappedBodies are actually interacting.")]
        public float BoundingRadius = 1;
        [Tooltip("Whether or not this body will refresh its bounding radius during " +
            "OnEnable(). Set this to false if you need the BoundingRadius to be different " +
            "than what this object's colliders indicate it should be (e.g. missiles with " +
            "proximity fusing)")]
        public bool AutoRefreshBoundingRadius = true;

        public bool Interactable = true;

        public Vector64 GetPosition()
        {
            return Position;
        }

        /// <summary>
        /// Place this object into the world using Unity's coordinates.
        /// This does not affect the object's real position, only where it is located for
        /// handling interactions using Unity's physics system.
        /// </summary>
        /// <param name="PositionInUnity"></param>
        public virtual void PlaceAt(Vector3 PositionInUnity)
        {
            transform.position = PositionInUnity;
            transform.localScale = Vector3.one;
        }
        public virtual void SyncPosition()
        {
            // DO NOTHING UNLESS OVERRIDDEN
        }

        protected virtual void OnEnable()
        {
            DoubleCoordinateField.Add(this);

            if (AutoRefreshBoundingRadius)
            {
                Bounds bounds = GatherBounds();

                BoundingRadius = bounds.extents.magnitude;
            }
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

        protected virtual void OnDisable()
        {
            DoubleCoordinateField.Remove(this);
        }

        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Interactable ? Color.red : Color.gray;

            Gizmos.DrawWireSphere(transform.position, BoundingRadius);
        }

        void ILargePosition.MovePosition(Vector64 delta)
        {
            Position += delta;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using DoublePreciseCoords.Cameras;

namespace DoublePreciseCoords
{
    [DefaultExecutionOrder(100)]
    public class DPCObject: MonoBehaviour
    {
        [Tooltip("This object's position... but in a 64-bit Vector format. " +
            "With a WrapperHub in the scene, this lets objects use a much larger " +
            "coordinate system in a useful capacity.")]
        public Vector64 Position;
        private Vector3 LastRawPosition;

        [Tooltip("The bounding radius of this object, used by the WrapperHub to " +
            "determine what WrappedBodies are actually interacting.")]
        public float BoundingRadius = 1;

        [Tooltip("Whether or not this body will refresh its bounding radius during " +
            "OnEnable(). Set this to false if you need the BoundingRadius to be different " +
            "than what this object's colliders indicate it should be (e.g. missiles with " +
            "proximity fusing)")]
        public bool AutoRefreshBoundingRadius = true;

        public bool Interactable = true;
        public DPCViewType Viewability = Cameras.DPCViewType.Visible;

#pragma warning disable CS0108
        public Rigidbody rigidbody { get; protected set; }
#pragma warning restore

        public DPCRenderer RenderController;

        public float Speed
        {
            get => rigidbody.velocity.magnitude;
        }

        /// <summary>
        /// Sets this object's raw Unity position independent of updating its Double Position
        /// </summary>
        /// <param name="posInUnity"></param>
        public void SetRawPosition (Vector3 posInUnity)
        {
            transform.position = posInUnity;
            transform.localScale = Vector3.one;

            LastRawPosition = transform.position;
        }

        public void SetViewPosition(Vector3 posInUnity, float scale, bool visible)
        {
            transform.position = posInUnity;
            transform.localScale = Vector3.one * scale;

            LastRawPosition = transform.position;

            if(RenderController)
            {
                RenderController.SetVisibility(visible);
            }
        }

        public void SyncPosition ()
        {
            Vector3 delta = transform.position - LastRawPosition;
            MovePosition(delta);
        }

        public void MovePosition (Vector64 delta)
        {
            Position += delta;
        }

        /// <summary>
        /// Use this method to implement specialized kinematic behavior
        /// </summary>
        public virtual void OnCustomPhysics (bool hasNeighbors) { }

        protected void OnValidate ()
        {
            if(DPCWorld.Exists())
            {
                return;
            }

            DPCWorld.Create();
        }

        protected virtual void OnEnable ()
        {
            DPCWorld.Add(this);

            if(AutoRefreshBoundingRadius)
            {
                Bounds bounds = GatherBounds();

                BoundingRadius = bounds.extents.magnitude;
            }

            LastRawPosition = transform.position;
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
            DPCWorld.Remove(this);
        }

        protected virtual void OnDrawGizmos ()
        {
            Gizmos.color = Interactable ? Color.green : Color.gray;

            Gizmos.DrawWireSphere(transform.position, BoundingRadius);
        }
    }
}

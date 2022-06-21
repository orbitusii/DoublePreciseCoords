/*

CVW-5 Prototype 2: Dynamic Wrapped Body
Author: Dan Lodholm (github: orbitusii)
Copyright: 2022

*/

using UnityEngine;
namespace DoublePreciseCoords
{
    [RequireComponent(typeof(Rigidbody))]
    public class DynamicBody : WrappedBody, ILargePosition
    {
        // The body's position in Unity's coordinate system. Used only to calculate position
        // differential each frame
        private Vector3 preSimulationPosition;

        // The rigidbody component on this object
#pragma warning disable CS0108
        public Rigidbody rigidbody
        {
            get; protected set;
        }
#pragma warning restore CS0108

        [Tooltip("Like Rigidbody.Interpolate, this will predict the object's instantaneous position " +
            "based on its velocity and how long it has been since the last FixedUpdate()")]
        public bool InterpolateView = false;

        public float Speed
        {
            get => rigidbody.velocity.magnitude;
        }

        protected virtual void FixedUpdate()
        {
            if (!DoubleCoordinateField.Exists())
            {
                Debug.LogWarning("Wrapped Rigidbodies exist without a Wrapper Hub!");
                enabled = false;
                return;
            }
        }

        /// <summary>
        /// Place this object into the world using Unity's coordinates.
        /// This does not affect the object's real position, only where it is located for
        /// handling interactions using Unity's physics system.
        /// </summary>
        /// <param name="PositionInUnity"></param>
        public override void PlaceAt(Vector3 PositionInUnity)
        {
            transform.position = PositionInUnity;
            preSimulationPosition = PositionInUnity;
        }

        public override void SyncPosition()
        {
            Vector3 diff = transform.position - preSimulationPosition;

            Position += diff;
        }

        protected override void OnEnable()
        {
            rigidbody = GetComponent<Rigidbody>();

            base.OnEnable();
        }
    }
}

/*

CVW-5 Prototype 2: Camera Position in 64-bit space (i.e. a floating origin camera system)
Author: Dan Lodholm (github: orbitusii)
Copyright: 2022

*/

using UnityEngine;

namespace DoublePreciseCoords.Cameras
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class CameraPosition : MonoBehaviour, ILargePosition
    {
        [SerializeField]
        protected Vector64 Position;

        [SerializeField]
        protected ViewCondObject TargetBody;
        [SerializeField, ContextMenuItem("Push Position", "PushTargetPosition")]
        protected Vector64 TargetPosition;

        protected ILargePosition PositionRef;

        [SerializeField]
        protected Vector3 ViewOffset = new Vector3(0, 3, -15);
        [SerializeField]
        protected float ViewDistance;

        [SerializeField, Tooltip("X = Pitch, Y = Yaw, Roll is not used")]
        protected Vector2 ViewAngle = Vector2.zero;
        [HideInInspector]
        protected Vector2 ViewAngleRads = Vector2.zero;

        Vector64 ILargePosition.GetPosition()
        {
            return Position;
        }

        void ILargePosition.MovePosition(Vector64 delta)
        {
            throw new System.NotSupportedException("Moving the camera via MovePosition() is not allowed!");
        }

        // We do this here rather than in LateUpdate() because while in other games the CAMERA is moving,
        // in our case the camera is stationary and EVERYTHING ELSE is moving. Thus, we do Unity's
        // recommended order in reverse.
        protected virtual void Update()
        {
            // Update our position and place the camera in the world!
            UpdatePosition();
        }

        public void UpdatePosition()
        {
            Vector64 basePosition = Position;

            // No Position Reference, we have a problem. Return (0, 0, 0)
            if (PositionRef == null)
            {
                PositionRef = TargetBody as ILargePosition;
                if (PositionRef == null) return;
            }
            // A position reference, no WrappedBody reference. Return the position reference's position
            else if (TargetBody == null)
            {
                basePosition = PositionRef.GetPosition();
            }
            // We have a WrappedBody reference. Return that body's position, interpolated if marked.
            else
            {
                if (TargetBody.InterpolateView)
                {
                    basePosition = TargetBody.GetInterpolatedPosition();
                }
                else
                {
                    basePosition = TargetBody.GetPosition();
                }
            }

            transform.eulerAngles = ViewAngle;
            Vector3 worldOffset = transform.TransformVector(ViewOffset);

            // Make sure the camera is positioned at the origin for sanity
            transform.position = Vector3.zero;
            // Store our 64-bit position in the world
            Position = basePosition + worldOffset;
        }

        protected virtual void LateUpdate()
        {
            // Uh.... if any LateUpdate stuff needs to get done, here's the place for it.
        }

        public void SetTargetBody(ViewCondObject target)
        {
            TargetBody = target;
            PositionRef = TargetBody as ILargePosition;
        }

        public void SetTargetObject(ILargePosition targetObject)
        {
            // This is intentional. We may not be looking at a WrappedBody.
            TargetBody = null;
        }

        private void PushTargetPosition()
        {
            SetTargetPosition(TargetPosition);
        }

        public void SetTargetPosition(Vector64 targetPosition)
        {
            // This is intentional. We are looking at an arbitrary position.
            var dummy = new DummyPosition(targetPosition);
            PositionRef = dummy;
        }

        public void SetViewAngle(Vector2 angle)
        {
            ViewAngle = clampAngle(angle);
            ViewAngleRads = ViewAngle * Mathf.Deg2Rad;
        }

        public void MoveViewAngle(Vector2 deltaAngle)
        {
            ViewAngle = clampAngle(ViewAngle + deltaAngle);
            ViewAngleRads = ViewAngle * Mathf.Deg2Rad;
        }

        private Vector2 clampAngle(Vector2 angle)
        {
            // Y is yaw around the object
            float clampY = angle.y % 180;

            // X is pitch around the object
            float clampX = angle.x;
            if (clampX > 90) clampX = 90;
            else if (clampX < -90) clampX = -90;

            return new Vector2(clampX, clampY);
        }

        public void SetViewDistance(float distance)
        {
            ViewDistance = distance;
            Vector3 newOffset = ViewOffset.normalized * distance;
            ViewOffset = newOffset;
        }

        public void SetViewOffset(Vector3 offset)
        {
            ViewOffset = offset;
            ViewDistance = ViewOffset.magnitude;
        }

        protected virtual void OnEnable()
        {
            if (TargetBody == null)
            {
                PushTargetPosition();
            }
        }

        protected struct DummyPosition : ILargePosition
        {
            private Vector64 Position;
            Vector64 ILargePosition.GetPosition()
            {
                return Position;
            }

            void ILargePosition.MovePosition(Vector64 delta)
            {
                Position += delta;
            }

            public DummyPosition(Vector64 pos)
            {
                Position = pos;
            }
        }
    }
}

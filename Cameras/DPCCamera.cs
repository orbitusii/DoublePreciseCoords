using System;
using System.Collections.Generic;
using UnityEngine;

namespace DoublePreciseCoords.Cameras
{
    [RequireComponent(typeof(Camera))]
    public class DPCCamera: MonoBehaviour
    {
        public DPCViewSettings Settings;

        private List<DPCObject> ViewCache;

        public DPCObject ViewTarget;
        private int ViewTargetIndex;

        public Vector2 ViewAngle = Vector2.zero;
        public float ViewDistance;

        public Vector64 CameraPosition = Vector64.zero;

        private void OnEnable ()
        {
            if(DPCWorld.Exists())
            {
                DPCWorld.OnBodiesChanged += OnBodiesChanged;
                UpdateViewCache();

                return;
            }

            Debug.LogWarning("A DPCCamera component exists in a scene with no DPCWorld.", this);
            enabled = false;
        }

        private void OnDisable ()
        {
            DPCWorld.OnBodiesChanged -= OnBodiesChanged;
        }

        public void OnBodiesChanged ()
        {
            UpdateViewCache();
        }

        protected void UpdateViewCache ()
        {
            ViewCache = DPCWorld.AllBodies.FindAll(x => x.Interactable && x.Viewability.HasFlag(DPCViewType.Visible));
        }

        public void StepViewTarget (int steps)
        {
            ViewTargetIndex += steps;
            ViewTargetIndex %= ViewCache.Count;

            ViewTarget = ViewCache[ViewTargetIndex];
        }

        public void SetViewTarget (int target)
        {
            ViewTargetIndex = target;
            ViewTargetIndex %= ViewCache.Count;

            ViewTarget = ViewCache[ViewTargetIndex];
        }

        public void ForceViewTarget (DPCObject target)
        {
            ViewTarget = target;
            ViewTargetIndex = -1;
        }

        public bool CheckIndex (int index)
        {
            return false;
        }

        public void SetAbsoluteOffset(Vector3 vector)
        {
            transform.position = Vector3.zero;
            transform.LookAt(-vector);

            ViewDistance = vector.magnitude;
            ViewAngle = transform.eulerAngles;
        }

        public void Update ()
        {
            transform.position = Vector3.zero;

            ViewAngle = clampAngle(ViewAngle);
            transform.eulerAngles = ViewAngle;

            if(ViewTarget)
            {
                Vector3 trueOffset = ViewDistance * transform.forward;
                CameraPosition = ViewTarget.Position - trueOffset;
            }
        }

        private Vector2 clampAngle(Vector2 angle)
        {
            // Y is yaw around the object, we need a range from -180 to +180
            float clampY = angle.y > 180 ? angle.y - 360 : angle.y;

            // X is pitch around the object, we need a range from -90 to +90
            float clampX = angle.x;
            if (clampX > 90) clampX = 90;
            else if (clampX < -90) clampX = -90;

            return new Vector2(clampX, clampY);
        }

        // We position objects here because of the reversed camera-object hierarchy.
        // Camera is stationary, objects move relative to it.
        public void LateUpdate ()
        {
            foreach (DPCObject obj in ViewCache)
            {
                ScaleObjectToView(obj);
            }
        }

        protected void ScaleObjectToView(DPCObject obj)
        {
            Vector3 unscaled = (Vector3)(obj.Position - CameraPosition);

            // This is called D0 for reasons explained below.
            float D0 = unscaled.magnitude;

            if (D0 <= Settings.InnerAreaRadius)
            {
                // We're within the inner bubble, just draw everything scaled normally.

                obj.SetViewPosition(unscaled, 1, true);
            }
            else
            {
                // This is some weird but fun trigonometry, the equations look like this:
                //
                //      (D0 - Ri)(Rs - Ri)
                // D1 = ------------------
                //           (Rr - Ri)
                //
                // Where D1 is the scaled distance between the two bubbles, D0 is the raw distance from
                // the origin, Ri is the inner bubble radius, Rs is the scaled outer bubble radius,
                // and Rr is the real outer bubble radius.
                // 
                // In short, D1 is based on the ratio of the scaled outer bubble's radius and the
                // unscaled outer bubble's radius, and then we multiply the unscaled distance by this
                // ratio to get the scaled distance. We subtract the inner bubble's radius from all
                // of the other radii (except D1) because we only care about scaling OUTSIDE of the
                // inner, full-scaled bubble.
                //
                // Once we have this value resolved, we can place the object along its original
                // position vector at distance D1. The next step is to scale the object using:
                //
                // S1 = D1 / D0
                //
                // Much simpler, where the corrected scale for this shrunk-down position is based
                // on the ratio of the scaled distance to the real distance. There is more trig
                // behind this ratio, but it's not important.
                //
                // If you REALLY want to know more about why this math is this way, talk to Sendit
                // in the CVW5 discord and he can explain more about it.

                float DistMinusRInner = D0 - Settings.InnerAreaRadius;

                float num = DistMinusRInner * Settings.RScaledMinusRInner;

                float D1 = num / Settings.RRealMinusRInner;

                // The final scaled position vector
                Vector3 scaledPos = unscaled.normalized * D1;

                // The final scale ratio.
                float S1 = D1 / D0;

                // True if we're within the outer bubble, false if we're outside of it.
                bool canSee = D0 <= Settings.OuterAreaRadius;

                obj.SetViewPosition(scaledPos, S1, canSee);
            }
        }
    }
}

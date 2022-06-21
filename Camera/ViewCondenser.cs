/*

CVW-5 Prototype 2: View Condenser for Floating Origin Purposes
Author: Dan Lodholm (github: orbitusii)
Copyright: 2022

*/

using UnityEngine;

namespace DoublePreciseCoords.Camera
{
    public class ViewCondenser : MonoBehaviour
    {
        protected static ViewCondenser singleton;

        [SerializeField]
        protected ViewCondenserSettings settings;

        private float RScaledMinusRInner, RRealMinusRInner;

        public static (bool visible, Vector3 position, float scale) GetScaledPosition(Vector3 unscaled)
        {
            if (singleton == null)
            {
                Debug.LogError("Something attempted to get a scaled position from the ViewCondenser," +
                    " but there is no ViewCondenser in the scene! Please add one to fix this error.");
                return (false, unscaled, 1);
            }

            return singleton.GetScaledPositionSingleton(unscaled);
        }

        public (bool visible, Vector3 position, float scale) GetScaledPositionSingleton(Vector3 unscaled)
        {
            // This is called D0 for reasons explained below.
            float D0 = unscaled.magnitude;

            if (D0 <= settings.InnerAreaRadius)
            {
                // We're within the inner bubble, just draw everything scaled normally.

                return (true, unscaled, 1);
            }
            else if (D0 <= settings.OuterAreaRadius)
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

                float DistMinusRInner = D0 - settings.InnerAreaRadius;

                float num = DistMinusRInner * RScaledMinusRInner;

                float D1 = num / RRealMinusRInner;

                // The final scaled position vector
                Vector3 scaledPos = unscaled.normalized * D1;

                // The final scale ratio.
                float S1 = D1 / D0;

                return (true, scaledPos, S1);
            }
            else
            {
                // We're outside of the outer bubble's REAL radius, so you don't need to draw
                // this object at all.

                return (false, unscaled, 1);
            }
        }

        private void OnEnable()
        {
            if (Exists())
            {
                Debug.LogWarning("Multiple View Condensers exist!", this);
                gameObject.SetActive(false);
                return;
            }
            else
            {
                singleton = this;
            }

            if (settings == null)
            {
                Debug.LogWarning("View Condenser settings are not present! Please fix!", this);
            }
            else
            {
            }
        }

        [ContextMenu("Refresh radius values from settings")]
        public void ReCacheSettings()
        {
            // Cache these values for ease of use later... might want to make their names better
            RScaledMinusRInner = settings.OuterScaledRadius - settings.InnerAreaRadius;
            RRealMinusRInner = settings.OuterAreaRadius - settings.InnerAreaRadius;
        }

        public static bool Exists()
        {
            return !(singleton == null);
        }
    }
}

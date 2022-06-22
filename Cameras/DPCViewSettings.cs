/*

CVW-5 Prototype 2: View Condenser Settings
Author: Dan Lodholm (github: orbitusii)
Copyright: 2022

*/

using System;
using UnityEngine;

namespace DoublePreciseCoords.Cameras
{
    [CreateAssetMenu()]
    public class DPCViewSettings : ScriptableObject
    {
        [Tooltip("The maximum distance at which objects will be drawn at full scale")]
        public float InnerAreaRadius = 2500;
        [Tooltip("The maximum distance at which objects will be rendered at all")]
        public float OuterAreaRadius = 50000;
        [Tooltip("The scaled-down radius to crunch down OuterAreaRadius into")]
        public float OuterScaledRadius = 3000;

        public float RScaledMinusRInner
        {
            get
            {
                return OuterScaledRadius - InnerAreaRadius;
            }
        }

        public float RRealMinusRInner
        {
            get
            {
                return OuterAreaRadius - InnerAreaRadius;
            }
        }
    }

    [Flags]
    public enum DPCViewType
    {
        None = 0,
        Visible = 1,
        Focusable = 2,
    }
}

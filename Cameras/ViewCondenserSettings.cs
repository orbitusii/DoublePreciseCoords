/*

CVW-5 Prototype 2: View Condenser Settings
Author: Dan Lodholm (github: orbitusii)
Copyright: 2022

*/

using UnityEngine;

namespace DoublePreciseCoords.Cameras
{
    [CreateAssetMenu()]
    public class ViewCondenserSettings : ScriptableObject
    {
        [Tooltip("The maximum distance at which objects will be drawn at full scale")]
        public float InnerAreaRadius = 2500;
        [Tooltip("The maximum distance at which objects will be rendered at all")]
        public float OuterAreaRadius = 50000;
        [Tooltip("The scaled-down radius to crunch down OuterAreaRadius into")]
        public float OuterScaledRadius = 3000;
    }
}

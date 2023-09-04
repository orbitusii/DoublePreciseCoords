using loki_geo;
using UnityEngine;

namespace DoublePreciseCoords
{
    internal static class Extensions
    {
        // Vector64 Extensions
        /// <summary>
        /// Converts a Vector64 into a Vector3, dropping precision from 64-bit to 32-bit; switches handedness from WGS-compliant right-handedness to left-handed Unity coords in the process.
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static Vector3 ToVector3(this Vector64 vec) => new Vector3((float)-vec.y, (float)vec.z, (float)vec.x);
        /// <summary>
        /// Switches a Vector64's handedness from right-handed WGS to Unity's left-handed paradigm.
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static Vector64 ToUnityAxes(this Vector64 vec) => new Vector64(-vec.y, vec.z, vec.x);
        /// <summary>
        /// Switches a Vector64's handedness from Unity's left-handed to WGS right-handed.
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static Vector64 FromUnityAxes(this Vector64 vec) => new Vector64(vec.z, -vec.x, vec.y);

        // Vector3 Extensions
        /// <summary>
        /// Converts a Vector3 into a Vector64, switching handedness from Unity's left-handed paradigm to WGS right-handedness.
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        public static Vector64 ToWGS(this Vector3 vec) => new Vector64(vec.z, -vec.x, vec.y);
    }
}

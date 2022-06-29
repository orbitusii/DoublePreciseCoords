using System.Collections;
using UnityEngine;

namespace DoublePreciseCoords.Cameras
{
    public interface IFirstPersonView
    {
        Vector3 this[int i] { get; }
    }
}
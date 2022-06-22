/*

CVW-5 Prototype 2: Large Position Interface
Author: Dan Lodholm (github: orbitusii)
Copyright: 2022

*/

namespace DoublePreciseCoords
{
    /// <summary>
    /// [DEPRECATED] Use DPCObject instead.
    /// </summary>
    public interface ILargePosition
    {
        Vector64 GetPosition();

        void MovePosition(Vector64 delta);
    }
}

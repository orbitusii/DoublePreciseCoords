/*

CVW-5 Prototype 2: Large Position Interface
Author: Dan Lodholm (github: orbitusii)
Copyright: 2022

*/

namespace DoublePreciseCoords
{
    public interface ILargePosition
    {
        public Vector64 GetPosition();

        public void MovePosition(Vector64 delta);
    }
}


namespace DoublePreciseCoords
{
    public struct DPCOwnerInfo
    {
        public string OwnerName;
        public int OwnerID;

        public DPCOwnerInfo (string name = "", int id = -1)
        {
            OwnerID = id;
            OwnerName = name;
        }
    }
}
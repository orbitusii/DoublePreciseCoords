using UnityEngine;

namespace DoublePreciseCoords
{
    public interface IDamageReceiver
    {
        int Health { get; set; }

        bool DoDamage(int dmg, DPCOwnerInfo owner = default);

        void OnDeath(DPCOwnerInfo owner = default);
    }
}

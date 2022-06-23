using UnityEngine;

namespace DoublePreciseCoords
{
    public interface IDamageReceiver
    {
        int Health { get; set; }

        bool DoDamage(int dmg);

        void OnDeath();
    }
}

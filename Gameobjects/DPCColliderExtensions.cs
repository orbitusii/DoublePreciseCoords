using System;
using UnityEngine;

namespace DoublePreciseCoords
{
    public static class DPCColliderExtensions
    {

        public static void PushDamage (this Collider col, int damage, DPCOwnerInfo owner = default)
        {
            IDamageReceiver damageReceiver = col.GetComponent(typeof(IDamageReceiver)) as IDamageReceiver;

            if (damageReceiver == null)
            {
                // If it's not attached to this object, check the parent
                damageReceiver = col.GetComponentInParent(typeof(IDamageReceiver)) as IDamageReceiver;
            }

            damageReceiver?.DoDamage(damage, owner);
        }
    }
}

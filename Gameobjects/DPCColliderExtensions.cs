using System;
using UnityEngine;

namespace DoublePreciseCoords
{
    public static class DPCColliderExtensions
    {
        public static void PushDamage (this Collider col, int damage)
        {
            IDamageReceiver damageReceiver = col.GetComponent(typeof(IDamageReceiver)) as IDamageReceiver;

            if(damageReceiver == null)
            {
                return;
            }

            damageReceiver.DoDamage(damage);
        }
    }
}

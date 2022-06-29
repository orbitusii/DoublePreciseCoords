using System.Collections;
using UnityEngine;

namespace DoublePreciseCoords
{
    [CreateAssetMenu]
    public class DPCProjectileData : ScriptableObject
    {
        public GameObject VisualPrefab;
        public GameObject EffectsPrefab;

        [Min(0)]
        public float ProjectileRadius = 0f;

        public int Damage = 5;
        [Min(0)]
        public float DamageRadius = 0;

        public float LaunchSpeed = 5f;
        public float Acceleration = 0f;
        public float Drag;

        public Vector2 SteeringPower = Vector2.zero;

        public float Gravity = 1f;

        public float Lifetime = -1f;
        public float ArmingDelay = 0.5f;
    }
}
using System.Collections;
using UnityEngine;

namespace DoublePreciseCoords
{
    [DefaultExecutionOrder(101)]
    public class DPCProjectile : DPCObject
    {
        public DPCProjectileData Data;
        public Vector2 SteeringInput;

        public Vector3 Velocity { get; protected set; }
        protected RaycastHit HitData;

        protected float EndOfLife = 0;
        protected float ArmedTime = 0;

        // Use this for initialization
        public void Initialize(Vector3 direction)
        {
            EndOfLife = Time.time + Data.Lifetime;
            ArmedTime = Time.time + Data.ArmingDelay;

            BoundingRadius = Data.DamageRadius > 0 ? BoundingRadius : Data.DamageRadius;

            Velocity = direction.normalized * (direction.magnitude + Data.LaunchSpeed);
        }

        public void PushSteeringCommand (Vector2 inputVec)
        {
            for (int i = 0; i < 2; i++)
            {
                SteeringInput[i] = Mathf.Clamp(inputVec[i], -1, 1);
            }
        }

        public override void OnCustomPhysics (bool hasNeighbors)
        {
            if (Interactable)
            {

                if (Data.Lifetime > 0 && Time.time >= EndOfLife)
                {
                    Detonate(null);
                    return;
                }

                float dt = Time.fixedDeltaTime;

                Vector3 accel = transform.forward * (Data.Acceleration - (Velocity.sqrMagnitude * Data.Drag));
                Vector3 gravity = Physics.gravity * Data.Gravity;
                Vector3 steering = transform.right * Data.SteeringPower.x + transform.up * Data.SteeringPower.y;

                Velocity += (accel + gravity + steering) * dt;

                Vector3 lastPos = transform.position;
                transform.position += Velocity * dt;
                transform.forward = Velocity.normalized;

                if (Time.time < ArmedTime)
                {
                    return;
                }

                if (hasNeighbors)
                {
                    bool didHit;

                    if (Data.ProjectileRadius == 0)
                    {
                        didHit = Physics.Raycast(
                            lastPos,
                            Velocity.normalized,
                            out HitData,
                            Velocity.magnitude * dt);
                    }
                    else
                    {
                        didHit = Physics.SphereCast(
                               lastPos,
                               Data.ProjectileRadius,
                               Velocity.normalized,
                               out HitData,
                               Velocity.magnitude * dt);
                    }

                    if (didHit)
                    {
                        Detonate(HitData.collider);
                    }
                }

                SteeringInput = Vector2.zero;
            }
        }

        public void Detonate (Collider hitObject)
        {
            Interactable = false;
            Velocity = Vector3.zero;

            Debug.Log($"Projectile {name} has detonated on object {hitObject}");

            // Spawn effects

            if(hitObject == null)
            {
                return;
            }

            if(Data.DamageRadius > 0)
            {
                Collider[] hitByBlast = Physics.OverlapSphere(transform.position, Data.DamageRadius);

                foreach(Collider singleHit in hitByBlast)
                {
                    singleHit.PushDamage(Data.Damage);
                }
            }
            else if (hitObject)
            {
                hitObject.PushDamage(Data.Damage);
            }

        }
    }
}
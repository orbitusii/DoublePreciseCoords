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

        protected float RemainingLifetime = 0;
        protected float ArmingDelay = 0;

        // Use this for initialization
        public void Initialize(Vector3 direction)
        {
            RemainingLifetime = Data.Lifetime;
            ArmingDelay = Data.ArmingDelay;

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

                if (Data.Lifetime > 0 && RemainingLifetime <= 0)
                {
                    Detonate(null);
                }

                float dt = Time.fixedDeltaTime;
                RemainingLifetime -= dt;
                ArmingDelay -= dt;

                Vector3 accel = transform.forward * (Data.Acceleration - (Velocity.sqrMagnitude * Data.Drag));
                Vector3 gravity = Physics.gravity * Data.Gravity;
                Vector3 steering = transform.right * Data.SteeringPower.x + transform.up * Data.SteeringPower.y;

                Velocity += (accel + gravity + steering) * dt;

                if (hasNeighbors && ArmingDelay <= 0)
                {
                    Vector3 start = transform.position;
                    bool didHit;

                    if (Data.ProjectileRadius == 0)
                    {
                        didHit = Physics.Raycast(
                            start,
                            Velocity.normalized,
                            out HitData,
                            Velocity.magnitude * dt);
                    }
                    else
                    {
                        didHit = Physics.SphereCast(
                               start,
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

                transform.position += Velocity * dt / 2;
                transform.forward = Velocity.normalized;

                SteeringInput = Vector2.zero;
            }
        }

        public void Detonate (Collider hitObject)
        {
            Interactable = false;

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
using System;
using System.Numerics;
using BilliardsGame.Interfaces;

namespace BilliardsGame.Core
{
    public class CueController : ICueController
    {
        public float Power { get; private set; }
        public Vector2 CueDirection { get; private set; }

        private const float PowerChargeRate = 1.0f; // e.g., 1 unit per second
        private const float MaxImpulse = 500f; // Tune this to actual game scale

        public void UpdateAim(Vector2 cueBallPosition, Vector2 mousePosition)
        {
            var diff = cueBallPosition - mousePosition;
            if (diff.LengthSquared() > 0.0001f)
            {
                CueDirection = Vector2.Normalize(diff);
            }
            else
            {
                CueDirection = Vector2.Zero;
            }
        }

        public void ChargeShot(float deltaTime)
        {
            Power += deltaTime * PowerChargeRate;
            if (Power > 1.0f)
            {
                Power = 0.0f; // Overcharge resets
            }
        }

        public void ResetCharge()
        {
            Power = 0.0f;
        }

        public void ExecuteShot(IPhysicsBody cueBall)
        {
            if (Power > 0f && cueBall != null && CueDirection != Vector2.Zero)
            {
                Vector2 impulse = CueDirection * Power * MaxImpulse;
                cueBall.ApplyImpulse(impulse);
            }
            Power = 0.0f;
        }
    }
}

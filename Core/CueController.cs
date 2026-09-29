using System;
using System.Numerics;
using BilliardsGame.Interfaces;
using BilliardsGame.Interfaces.Enums;
using BilliardsGame.Interfaces.Models;

namespace BilliardsGame.Core
{
    public class CueController : ICueController
    {
        public float Power { get; private set; }
        public Vector2 CueDirection { get; private set; }
        public bool IsOverheated { get; private set; }

        private const float PowerChargeRate = 1.0f; // e.g., 1 unit per second
        private const float BaseForce = 500f; 

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
            if (IsOverheated) return;

            Power += deltaTime * PowerChargeRate;
            if (Power >= 1.0f)
            {
                Power = 1.0f;
                IsOverheated = true;
            }
        }

        public void UpdateOverheat(float deltaTime)
        {
            if (IsOverheated)
            {
                Power -= deltaTime * PowerChargeRate * 0.5f; // slower cool down like syrup
                if (Power <= 0f)
                {
                    Power = 0f;
                    IsOverheated = false;
                }
            }
        }

        public void ResetCharge()
        {
            if (!IsOverheated)
            {
                Power = 0.0f;
            }
        }

        public void ExecuteShot(IPhysicsBody cueBall)
        {
            if (IsOverheated) return;

            if (Power > 0f && cueBall != null && CueDirection != Vector2.Zero)
            {
                Vector2 impulse = CueDirection * (Power * BaseForce);
                cueBall.ApplyImpulse(impulse);
            }
            Power = 0.0f;
        }
    }
}

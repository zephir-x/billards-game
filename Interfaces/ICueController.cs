using System.Numerics;

namespace BilliardsGame.Interfaces
{
    public interface ICueController
    {
        float Power { get; }
        Vector2 CueDirection { get; }
        bool IsOverheated { get; }

        void UpdateAim(Vector2 cueBallPosition, Vector2 mousePosition);
        void ChargeShot(float deltaTime);
        void ResetCharge();
        void ExecuteShot(IPhysicsBody cueBall);
        event System.Action<float>? OnCueHit;
        void UpdateOverheat(float deltaTime);
    }
}

using System.Numerics;

namespace BilliardsGame.Interfaces
{
    public interface ICueController
    {
        float Power { get; }
        Vector2 CueDirection { get; }

        void UpdateAim(Vector2 cueBallPosition, Vector2 mousePosition);
        void ChargeShot(float deltaTime);
        void ResetCharge();
        void ExecuteShot(IPhysicsBody cueBall);
    }
}

using System.Collections.Generic;
using System.Numerics;

namespace BilliardsGame.Physics
{
    /// <summary>
    /// Core physics engine interface responsible for deterministic calculations.
    /// </summary>
    public interface IPhysicsEngine
    {
        void Step(float fixedDeltaTime);
        void AddBody(IPhysicsBody body);
        void RemoveBody(IPhysicsBody body);
        IReadOnlyCollection<IPhysicsBody> GetBodies();
    }
}

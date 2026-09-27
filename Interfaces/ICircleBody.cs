using System.Numerics;

namespace BilliardsGame.Interfaces
{
    /// <summary>
    /// Represents a circular physical entity in the physics simulation.
    /// </summary>
    public interface ICircleBody : IPhysicsBody
    {
        /// <summary>
        /// Radius of the circular body.
        /// </summary>
        float Radius { get; }
    }
}

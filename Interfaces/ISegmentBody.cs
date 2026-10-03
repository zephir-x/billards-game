using System.Numerics;

namespace BilliardsGame.Interfaces
{
    /// <summary>
    /// Represents a static geometric linear segment mapped within the physics simulation, such as a table cushion.
    /// </summary>
    public interface ISegmentBody : IPhysicsBody
    {
        /// <summary>
        /// Absolute coordinate origin where the segment wall begins in world space.
        /// </summary>
        Vector2 StartPoint { get; }

        /// <summary>
        /// Absolute coordinate termination where the segment wall concludes in world space.
        /// </summary>
        Vector2 EndPoint { get; }

        /// <summary>
        /// Pre-calculated normalized perpendicular vector dictating angular reflection trajectories.
        /// </summary>
        Vector2 Normal { get; }
    }
}

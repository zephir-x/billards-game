using System.Numerics;

namespace BilliardsGame.Interfaces
{
    /// <summary>
    /// Represents a line segment physical entity in the physics simulation.
    /// </summary>
    public interface ISegmentBody : IPhysicsBody
    {
        /// <summary>
        /// Starting point of the segment in world space.
        /// </summary>
        Vector2 StartPoint { get; }

        /// <summary>
        /// Ending point of the segment in world space.
        /// </summary>
        Vector2 EndPoint { get; }

        /// <summary>
        /// Normalized normal vector of the segment surface.
        /// </summary>
        Vector2 Normal { get; }
    }
}

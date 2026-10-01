using BilliardsGame.Interfaces.Enums;
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
        
        /// <summary>
        /// Type of the ball.
        /// </summary>
        BallType BallType { get; }
        
        /// <summary>
        /// Optional Number of the ball (0-15).
        /// </summary>
        int Number { get; }

        float RotationAngle { get; set; }
        bool IsGhost { get; set; }
        float GhostLifeTime { get; set; }
    }
}

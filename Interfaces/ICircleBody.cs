using BilliardsGame.Interfaces.Enums;

namespace BilliardsGame.Interfaces
{
    /// <summary>
    /// Represents a circular physical body participating in the physics simulation.
    /// Typically used for representation of the billiard balls.
    /// </summary>
    public interface ICircleBody : IPhysicsBody
    {
        /// <summary>
        /// Radius of the circular body.
        /// </summary>
        float Radius { get; }
        
        /// <summary>
        /// Defines the variant of the ball (e.g., Cue, Solid, Striped, Black).
        /// </summary>
        BallType BallType { get; }
        
        /// <summary>
        /// The identification number of the ball (0 to 15, where 0 is typically the Cue ball).
        /// </summary>
        int Number { get; }

        /// <summary>
        /// Current localized rotation angle of the body, used visually for roll simulation.
        /// </summary>
        float RotationAngle { get; set; }

        /// <summary>
        /// Determines whether the body has entered a pocket and transitioned into a non-colliding ghost state.
        /// </summary>
        bool IsGhost { get; set; }

        /// <summary>
        /// Tracks the remaining life time logic for a ghosted body before it is completely destroyed.
        /// </summary>
        float GhostLifeTime { get; set; }
    }
}

using System.Numerics;

namespace BilliardsGame.Interfaces
{
    /// <summary>
    /// Represents a physical entity participating in the physics simulation.
    /// </summary>
    public interface IPhysicsBody
    {
        /// <summary>
        /// Unique identifier for the physics body.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Current position in world space.
        /// </summary>
        Vector2 Position { get; set; }

        /// <summary>
        /// Previous position in world space, essential for rendering interpolation.
        /// </summary>
        Vector2 PreviousPosition { get; set; }

        /// <summary>
        /// Current linear velocity vector.
        /// </summary>
        Vector2 Velocity { get; set; }

        /// <summary>
        /// Mass of the body. For static bodies, this is effectively infinite.
        /// </summary>
        float Mass { get; }

        /// <summary>
        /// Restitution (elasticity) coefficient in the range [0.0, 1.0].
        /// </summary>
        float Restitution { get; }

        /// <summary>
        /// Radius of the body if it is circular; 0 for linear or static boundaries.
        /// </summary>
        float Radius { get; }

        /// <summary>
        /// Indicates whether the body is static (unmovable) or dynamic.
        /// </summary>
        bool IsStatic { get; }

        /// <summary>
        /// Applies an instantaneous impulse vector to the body, adjusting its velocity.
        /// </summary>
        /// <param name="impulse">The impulse vector to apply.</param>
        void ApplyImpulse(Vector2 impulse);
    }
}

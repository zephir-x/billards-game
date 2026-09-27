using BilliardsGame.Interfaces;
using System;
using System.Numerics;

namespace BilliardsGame.Physics
{
    /// <summary>
    /// Represents a dynamic circular billiard ball participating in physics simulation.
    /// </summary>
    public class Ball : ICircleBody
    {
        /// <inheritdoc />
        public int Id { get; }

        /// <inheritdoc />
        public Vector2 Position { get; set; }

        /// <inheritdoc />
        public Vector2 PreviousPosition { get; set; }

        /// <inheritdoc />
        public Vector2 Velocity { get; set; }

        /// <inheritdoc />
        public float Mass { get; }

        /// <inheritdoc />
        public float Restitution { get; }

        /// <inheritdoc />
        public float Radius { get; }

        /// <inheritdoc />
        public bool IsStatic => false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Ball"/> class.
        /// </summary>
        /// <param name="id">Unique identifier for the ball.</param>
        /// <param name="initialPosition">Initial coordinates in world space.</param>
        /// <param name="mass">Mass of the ball (must be greater than zero).</param>
        /// <param name="radius">Radius of the ball (must be greater than zero).</param>
        /// <param name="restitution">Coefficient of restitution [0.0, 1.0].</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when mass or radius is less than or equal to zero.</exception>
        public Ball(int id, Vector2 initialPosition, float mass, float radius, float restitution = 0.95f)
        {
            if (mass <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(mass), "Mass must be strictly positive.");
            }

            if (radius <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be strictly positive.");
            }

            Id = id;
            Position = initialPosition;
            PreviousPosition = initialPosition;
            Mass = mass;
            Radius = radius;
            Restitution = restitution;
            Velocity = Vector2.Zero;
        }

        /// <inheritdoc />
        public void ApplyImpulse(Vector2 impulse)
        {
            if (Mass > 0f)
            {
                Velocity += impulse / Mass;
            }
        }
    }
}

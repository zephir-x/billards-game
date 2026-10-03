using BilliardsGame.Interfaces;
using BilliardsGame.Interfaces.Enums;
using System;
using System.Numerics;

namespace BilliardsGame.Physics
{
    /// <summary>
    /// Represents a dynamic circular billiard ball participating in physics simulation.
    /// Acts as the primary physical entity governed by integration steps and collision resolution.
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

        /// <inheritdoc />
        public BallType BallType { get; }

        /// <inheritdoc />
        public int Number { get; }

        /// <inheritdoc />
        public float RotationAngle { get; set; }

        /// <inheritdoc />
        public bool IsGhost { get; set; }

        /// <inheritdoc />
        public float GhostLifeTime { get; set; } = 1.5f;

        /// <summary>
        /// Initializes a new instance of the <see cref="Ball"/> class with predefined physical limits.
        /// </summary>
        /// <param name="id">Unique identifier for the engine tracking system.</param>
        /// <param name="initialPosition">Spawning coordinate in world space.</param>
        /// <param name="mass">Weight constant used to scale collision impulse distribution.</param>
        /// <param name="radius">Geometrical extension used for overlap detection.</param>
        /// <param name="restitution">Bounciness mapping how much kinetic energy is preserved post-collision.</param>
        /// <param name="type">Semantic identity determining visual features and rule-bound behaviors.</param>
        /// <param name="number">Numeric identifier physically drawn onto the ball surface.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if mass or radius metrics are zero or less.</exception>
        public Ball(int id, Vector2 initialPosition, float mass, float radius, float restitution = 0.95f, BallType type = BallType.Cue, int number = 0)
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
            BallType = type;
            Number = number;
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

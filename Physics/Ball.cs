using BilliardsGame.Interfaces;
using BilliardsGame.Interfaces.Enums;
using BilliardsGame.Interfaces.Models;
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

        /// <inheritdoc />
        public BallType BallType { get; }

        /// <inheritdoc />
        public int Number { get; }

        public float RotationAngle { get; set; }
        public bool IsGhost { get; set; }
        public float FadeTimer { get; set; } = 1.5f;

        /// <summary>
        /// Initializes a new instance of the <see cref="Ball"/> class.
        /// </summary>
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
using BilliardsGame.Interfaces;
using System.Numerics;

namespace BilliardsGame.Physics
{
    /// <summary>
    /// Represents a static linear barricade constraining boundaries on the billiard table.
    /// Used actively to violently reverse trajectory momentum on interaction.
    /// </summary>
    public class Cushion : ISegmentBody
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
        public float Mass => float.PositiveInfinity;

        /// <inheritdoc />
        public float Restitution { get; }

        /// <inheritdoc />
        public bool IsStatic => true;

        /// <inheritdoc />
        public Vector2 StartPoint { get; }

        /// <inheritdoc />
        public Vector2 EndPoint { get; }

        /// <inheritdoc />
        public Vector2 Normal { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref=""Cushion""/> class defining absolute table limits.
        /// </summary>
        /// <param name=""id"">Unique identifier for the cushion.</param>
        /// <param name=""startPoint"">Absolute metric origin point of the segment.</param>
        /// <param name=""endPoint"">Absolute metric termination point of the segment.</param>
        /// <param name=""restitution"">Kinetic bounciness value indicating energy retention [0.0, 1.0].</param>
        public Cushion(int id, Vector2 startPoint, Vector2 endPoint, float restitution = 0.8f)
        {
            Id = id;
            StartPoint = startPoint;
            EndPoint = endPoint;
            Restitution = restitution;

            // Represent the position as the midpoint of the cushion segment
            Position = (startPoint + endPoint) * 0.5f;
            PreviousPosition = Position;
            Velocity = Vector2.Zero;

            Vector2 edge = endPoint - startPoint;
            if (edge.LengthSquared() > 0f)
            {
                // Perpendicular vector (-dy, dx) normalized
                Vector2 perp = new Vector2(-edge.Y, edge.X);
                Normal = Vector2.Normalize(perp);
            }
            else
            {
                Normal = Vector2.Zero;
            }
        }

        /// <summary>
        /// Explicit exception for static bodies enforcing immunity to kinetic impulse mutations.
        /// </summary>
        /// <param name=""impulse"">The impulse vector (discarded context).</param>
        public void ApplyImpulse(Vector2 impulse)
        {
            // No-op for absolutely static bodies
        }
    }
}

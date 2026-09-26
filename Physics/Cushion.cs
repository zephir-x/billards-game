using BilliardsGame.Interfaces;
using System.Numerics;

namespace BilliardsGame.Physics
{
    /// <summary>
    /// Represents a static boundary or cushion of the billiard table.
    /// </summary>
    public class Cushion : IPhysicsBody
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
        public float Radius => 0f;

        /// <inheritdoc />
        public bool IsStatic => true;

        /// <summary>
        /// Starting point of the cushion segment in world space.
        /// </summary>
        public Vector2 StartPoint { get; }

        /// <summary>
        /// Ending point of the cushion segment in world space.
        /// </summary>
        public Vector2 EndPoint { get; }

        /// <summary>
        /// Normalized normal vector of the cushion surface used for collision response.
        /// </summary>
        public Vector2 Normal { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Cushion"/> class.
        /// </summary>
        /// <param name="id">Unique identifier for the cushion.</param>
        /// <param name="startPoint">Start coordinates of the segment.</param>
        /// <param name="endPoint">End coordinates of the segment.</param>
        /// <param name="restitution">Coefficient of restitution for bouncing [0.0, 1.0].</param>
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
        /// Static bodies are immune to impulses; this method performs no operation.
        /// </summary>
        /// <param name="impulse">The impulse vector (ignored).</param>
        public void ApplyImpulse(Vector2 impulse)
        {
            // No-op for static bodies
        }
    }
}

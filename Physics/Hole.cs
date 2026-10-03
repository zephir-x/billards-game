using System.Numerics;
using BilliardsGame.Interfaces;

namespace BilliardsGame.Physics
{
    /// <summary>
    /// Implements specific physical trap triggers mimicking geographic pockets scattered across the field.
    /// </summary>
    public class Hole : IPocket
    {
        /// <inheritdoc />
        public Vector2 Position { get; }

        /// <inheritdoc />
        public float Radius { get; }

        /// <summary>
        /// Defines the rigid positional and dimensional metrics for a trap.
        /// </summary>
        /// <param name=""position"">Absolute world-space vector target.</param>
        /// <param name=""radius"">Active limit determining intersection bounds with dynamic overlapping bodies.</param>
        public Hole(Vector2 position, float radius)
        {
            Position = position;
            Radius = radius;
        }
    }
}

using System.Numerics;
using Xunit;

namespace BilliardsGame.Physics.Tests
{
    /// <summary>
    /// Validates static line boundary components simulating solid rubber tables enclosing the energetic field cleanly.
    /// </summary>
    public class CushionTests
    {
        [Fact]
        public void Constructor_InitializesStaticPropertiesCorrectly()
        {
            int id = 10;
            Vector2 start = new Vector2(0f, 0f);
            Vector2 end = new Vector2(100f, 0f);
            float restitution = 0.85f;

            // Constructs explicit barrier edges anchoring immovable vertices defining exact play bounds.
            var cushion = new Cushion(id, start, end, restitution);

            // Validates that immovable boundaries accurately lock mass parameters securing infinite resistance mimicking clamped structural walls.
            Assert.Equal(id, cushion.Id);
            Assert.Equal(start, cushion.StartPoint);
            Assert.Equal(end, cushion.EndPoint);
            Assert.Equal(new Vector2(50f, 0f), cushion.Position);
            Assert.Equal(new Vector2(50f, 0f), cushion.PreviousPosition);
            Assert.Equal(Vector2.Zero, cushion.Velocity);
            Assert.True(float.IsPositiveInfinity(cushion.Mass));
            Assert.Equal(restitution, cushion.Restitution);
            Assert.True(cushion.IsStatic);
        }

        [Fact]
        public void ApplyImpulse_IsNoOp_VelocityRemainsZero()
        {
            var cushion = new Cushion(1, new Vector2(0f, 0f), new Vector2(100f, 0f));
            Vector2 initialVelocity = cushion.Velocity;
            Vector2 initialPosition = cushion.Position;

            // Ensures hard walls silently absorb energetic force injections bypassing vector modifications remaining completely dormant structurally without crashing frameworks explicitly.
            cushion.ApplyImpulse(new Vector2(500f, -500f));

            Assert.Equal(initialVelocity, cushion.Velocity);
            Assert.Equal(initialPosition, cushion.Position);
        }

        [Fact]
        public void Normal_IsPerpendicularAndNormalized()
        {
            var horizontalCushion = new Cushion(1, new Vector2(0f, 0f), new Vector2(10f, 0f));

            // Validates perpendicular boundary vector generations mapping rigid projections pushing penetrating objects perfectly back mimicking reflection mechanics perfectly.
            Assert.Equal(0f, horizontalCushion.Normal.X, precision: 5);
            Assert.Equal(1f, horizontalCushion.Normal.Y, precision: 5);
            Assert.Equal(1f, horizontalCushion.Normal.Length(), precision: 5);
        }
    }
}

using System.Numerics;
using Xunit;

namespace BilliardsGame.Physics.Tests
{
    public class CushionTests
    {
        [Fact]
        public void Constructor_InitializesStaticPropertiesCorrectly()
        {
            // Arrange
            int id = 10;
            Vector2 start = new Vector2(0f, 0f);
            Vector2 end = new Vector2(100f, 0f);
            float restitution = 0.85f;

            // Act
            var cushion = new Cushion(id, start, end, restitution);

            // Assert
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
            // Arrange
            var cushion = new Cushion(1, new Vector2(0f, 0f), new Vector2(100f, 0f));
            Vector2 initialVelocity = cushion.Velocity;
            Vector2 initialPosition = cushion.Position;

            // Act
            cushion.ApplyImpulse(new Vector2(500f, -500f));

            // Assert
            Assert.Equal(initialVelocity, cushion.Velocity);
            Assert.Equal(initialPosition, cushion.Position);
        }

        [Fact]
        public void Normal_IsPerpendicularAndNormalized()
        {
            // Horizontal segment pointing right: (0,0) to (10,0)
            var horizontalCushion = new Cushion(1, new Vector2(0f, 0f), new Vector2(10f, 0f));

            // Perpendicular to (10, 0) normalized is (0, 1)
            Assert.Equal(0f, horizontalCushion.Normal.X, precision: 5);
            Assert.Equal(1f, horizontalCushion.Normal.Y, precision: 5);
            Assert.Equal(1f, horizontalCushion.Normal.Length(), precision: 5);
        }
    }
}

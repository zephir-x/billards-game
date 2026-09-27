using System;
using System.Numerics;
using Xunit;

namespace BilliardsGame.Physics.Tests
{
    public class BallTests
    {
        [Fact]
        public void Constructor_InitializesPropertiesCorrectly()
        {
            // Arrange
            int id = 1;
            Vector2 initialPosition = new Vector2(100f, 200f);
            float mass = 0.17f;
            float radius = 5.7f;
            float restitution = 0.95f;

            // Act
            var ball = new Ball(id, initialPosition, mass, radius, restitution);

            // Assert
            Assert.Equal(id, ball.Id);
            Assert.Equal(initialPosition, ball.Position);
            Assert.Equal(initialPosition, ball.PreviousPosition);
            Assert.Equal(Vector2.Zero, ball.Velocity);
            Assert.Equal(mass, ball.Mass);
            Assert.Equal(radius, ball.Radius);
            Assert.Equal(restitution, ball.Restitution);
            Assert.False(ball.IsStatic);
        }

        [Theory]
        [InlineData(0f)]
        [InlineData(-1f)]
        public void Constructor_ThrowsArgumentOutOfRangeException_WhenMassIsZeroOrNegative(float invalidMass)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new Ball(1, Vector2.Zero, invalidMass, 5f));
        }

        [Theory]
        [InlineData(0f)]
        [InlineData(-2f)]
        public void Constructor_ThrowsArgumentOutOfRangeException_WhenRadiusIsZeroOrNegative(float invalidRadius)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new Ball(1, Vector2.Zero, 0.2f, invalidRadius));
        }

        [Fact]
        public void ApplyImpulse_ModifiesVelocityProportionallyToMass()
        {
            // Arrange
            float mass = 0.2f;
            var ball = new Ball(1, Vector2.Zero, mass, 5f);
            Vector2 impulse = new Vector2(2f, -1f);
            Vector2 expectedVelocity = impulse / mass;

            // Act
            ball.ApplyImpulse(impulse);

            // Assert
            Assert.Equal(expectedVelocity.X, ball.Velocity.X, precision: 5);
            Assert.Equal(expectedVelocity.Y, ball.Velocity.Y, precision: 5);
        }

        [Fact]
        public void ApplyImpulse_AccumulatesVelocityOnSuccessiveCalls()
        {
            // Arrange
            float mass = 0.5f;
            var ball = new Ball(1, Vector2.Zero, mass, 5f);
            Vector2 impulse1 = new Vector2(1f, 2f);
            Vector2 impulse2 = new Vector2(3f, -1f);
            Vector2 expectedVelocity = (impulse1 + impulse2) / mass;

            // Act
            ball.ApplyImpulse(impulse1);
            ball.ApplyImpulse(impulse2);

            // Assert
            Assert.Equal(expectedVelocity.X, ball.Velocity.X, precision: 5);
            Assert.Equal(expectedVelocity.Y, ball.Velocity.Y, precision: 5);
        }
    }
}

using System;
using System.Numerics;
using Xunit;

namespace BilliardsGame.Physics.Tests
{
    /// <summary>
    /// Enforces rigid structural definitions verifying explicit geometric bounding constructs maintaining independent object data.
    /// </summary>
    public class BallTests
    {
        [Fact]
        public void Constructor_InitializesPropertiesCorrectly()
        {
            // Setups explicit mapping coordinates creating basic physics components cleanly without invoking main engines explicitly ensuring structural correctness.
            int id = 1;
            Vector2 initialPosition = new Vector2(100f, 200f);
            float mass = 0.17f;
            float radius = 5.7f;
            float restitution = 0.95f;

            var ball = new Ball(id, initialPosition, mass, radius, restitution);

            // Validates that baseline constraints default exactly simulating stable zero-energy resting logic matching game defaults seamlessly.
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
            // Protects global calculations securing division logic catching bad data parameters before impacting engines breaking vectors into NaN paths explicitly.
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new Ball(1, Vector2.Zero, invalidMass, 5f));
        }

        [Theory]
        [InlineData(0f)]
        [InlineData(-2f)]
        public void Constructor_ThrowsArgumentOutOfRangeException_WhenRadiusIsZeroOrNegative(float invalidRadius)
        {
            // Secures continuous collision thresholds keeping spatial bounding matrices accurate rejecting collapsed geometry forcefully preventing spatial overlapping visually breaking.
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new Ball(1, Vector2.Zero, 0.2f, invalidRadius));
        }

        [Fact]
        public void ApplyImpulse_ModifiesVelocityProportionallyToMass()
        {
            float mass = 0.2f;
            var ball = new Ball(1, Vector2.Zero, mass, 5f);
            Vector2 impulse = new Vector2(2f, -1f);
            Vector2 expectedVelocity = impulse / mass;

            // Simulates exact force injections validating explicit momentum distribution equations executing flawlessly scaling mass proportionally mimicking real impacts correctly.
            ball.ApplyImpulse(impulse);

            Assert.Equal(expectedVelocity.X, ball.Velocity.X, precision: 5);
            Assert.Equal(expectedVelocity.Y, ball.Velocity.Y, precision: 5);
        }

        [Fact]
        public void ApplyImpulse_AccumulatesVelocityOnSuccessiveCalls()
        {
            float mass = 0.5f;
            var ball = new Ball(1, Vector2.Zero, mass, 5f);
            Vector2 impulse1 = new Vector2(1f, 2f);
            Vector2 impulse2 = new Vector2(3f, -1f);
            
            // Replicates compounded impacts simulating multi-touch frame overlaps combining forces consistently resolving complex interactions correctly retaining correct limits perfectly.
            Vector2 expectedVelocity = (impulse1 + impulse2) / mass;

            ball.ApplyImpulse(impulse1);
            ball.ApplyImpulse(impulse2);

            Assert.Equal(expectedVelocity.X, ball.Velocity.X, precision: 5);
            Assert.Equal(expectedVelocity.Y, ball.Velocity.Y, precision: 5);
        }
    }
}

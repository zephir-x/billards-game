using System;
using System.Numerics;
using Xunit;
using BilliardsGame.Physics;

namespace BilliardsGame.Physics.Tests
{
    public class PhysicsEngineTests
    {
        [Fact]
        public void Step_LinearMovement_UpdatesPositionAndPreviousPosition()
        {
            var engine = new PhysicsEngine(tableFriction: 0f); // No friction for this test
            var ball = new Ball(1, new Vector2(0, 0), 1f, 1f, 1f)
            {
                Velocity = new Vector2(10f, 0f)
            };
            engine.AddBody(ball);

            engine.Step(0.1f);

            Assert.Equal(new Vector2(0, 0), ball.PreviousPosition);
            Assert.Equal(new Vector2(1, 0), ball.Position);
        }

        [Fact]
        public void Step_Friction_ReducesVelocityAndCausesSleep()
        {
            var engine = new PhysicsEngine(tableFriction: 0.5f, sleepVelocityThreshold: 0.2f);
            var ball = new Ball(1, new Vector2(0, 0), 1f, 1f, 1f)
            {
                Velocity = new Vector2(1f, 0f)
            };
            engine.AddBody(ball);

            engine.Step(1f); // Velocity = 1 * (1 - 0.5*1) = 0.5f
            Assert.True(ball.Velocity.X > 0 && ball.Velocity.X < 1f);

            // Next step, speed goes below 0.2
            engine.Step(1f); // Velocity = 0.5 * 0.5 = 0.25
            engine.Step(1f); // Velocity = 0.25 * 0.5 = 0.125 < 0.2 -> 0

            Assert.Equal(Vector2.Zero, ball.Velocity);
        }

        [Fact]
        public void Step_BallBallHeadOnCollision_TransfersMomentum()
        {
            var engine = new PhysicsEngine(tableFriction: 0f);
            var ballA = new Ball(1, new Vector2(0, 0), mass: 1f, radius: 1f, restitution: 1.0f)
            {
                Velocity = new Vector2(1f, 0f)
            };
            var ballB = new Ball(2, new Vector2(2f - 0.001f, 0), mass: 1f, radius: 1f, restitution: 1.0f);

            engine.AddBody(ballA);
            engine.AddBody(ballB);

            engine.Step(0.01667f);

            // They should collide. Since mass is same and e=1, ballA stops, ballB moves.
            Assert.True(Math.Abs(ballA.Velocity.X) < 0.01f);
            Assert.True(ballB.Velocity.X > 0.99f);
        }

        [Fact]
        public void Step_BallBallAngledCollision_ScattersMomentum()
        {
            var engine = new PhysicsEngine(tableFriction: 0f);
            var ballA = new Ball(1, new Vector2(0, 0), mass: 1f, radius: 1f, restitution: 1.0f)
            {
                Velocity = new Vector2(1f, 0f)
            };
            var ballB = new Ball(2, new Vector2(1.5f, -1f), mass: 1f, radius: 1f, restitution: 1.0f);

            engine.AddBody(ballA);
            engine.AddBody(ballB);

            engine.Step(0.1f);

            Assert.True(Math.Abs(ballA.Velocity.Y) > 0.01f); // Y momentum should not be zero
            Assert.True(Math.Abs(ballB.Velocity.Y) > 0.01f);
        }

        [Fact]
        public void Step_PenetrationCorrection_SeparatesBalls()
        {
            var engine = new PhysicsEngine(tableFriction: 0f);
            var ballA = new Ball(1, new Vector2(0, 0), mass: 1f, radius: 1f);
            var ballB = new Ball(2, new Vector2(0.5f, 0), mass: 1f, radius: 1f);

            engine.AddBody(ballA);
            engine.AddBody(ballB);

            engine.Step(0.01667f);

            float dist = (ballA.Position - ballB.Position).Length();
            Assert.True(dist >= 2f - 0.001f, "Balls should be separated to sum of radii");
        }

        [Fact]
        public void Step_BallCushionCollision_BouncesBack()
        {
            var engine = new PhysicsEngine(tableFriction: 0f);
            var ball = new Ball(1, new Vector2(2f, 0), mass: 1f, radius: 1f, restitution: 0.5f)
            {
                Velocity = new Vector2(10f, 0)
            };
            var cushion = new Cushion(2, new Vector2(3f, -5f), new Vector2(3f, 5f), restitution: 1.0f);
            
            engine.AddBody(ball);
            engine.AddBody(cushion);

            engine.Step(0.1f);

            // Ball hits vertical cushion at x=3.
            // Starts at x=2, v=10, dt=0.1 => pos goes to 3.
            // Penetrates the cushion, so it should bounce.
            // Reduced energy due to ball e=0.5 -> post-col velocity should be -5f
            Assert.True(ball.Velocity.X < 0f);
        }

        [Fact]
        public void AreAllBodiesAtRest_WorksCorrectly()
        {
            var engine = new PhysicsEngine(tableFriction: 0.1f);
            var ball = new Ball(1, new Vector2(0, 0), 1f, 1f)
            {
                Velocity = new Vector2(0.05f, 0)
            };
            engine.AddBody(ball);

            Assert.True(engine.AreAllBodiesAtRest(0.051f)); // 0.05 < 0.051 => rested, true
            
            ball.Velocity = new Vector2(0.1f, 0f);
            Assert.False(engine.AreAllBodiesAtRest(0.051f)); // 0.1 > 0.051 => stays moving, false
            
            ball.Velocity = new Vector2(0.01f, 0f);
            Assert.True(engine.AreAllBodiesAtRest(0.051f)); // < 0.051 => rested, true
        }
    }
}
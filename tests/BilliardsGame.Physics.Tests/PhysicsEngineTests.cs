using System;
using System.Numerics;
using Xunit;
using BilliardsGame.Physics;

namespace BilliardsGame.Physics.Tests
{
    /// <summary>
    /// Macro-level engine simulations verifying broad kinematics resolving overlapping bodies accurately distributing momentum cleanly simulating physics logic fully.
    /// </summary>
    public class PhysicsEngineTests
    {
        [Fact]
        public void Step_LinearMovement_UpdatesPositionAndPreviousPosition()
        {
            var engine = new PhysicsEngine(tableFriction: 0f); 
            var ball = new Ball(1, new Vector2(0, 0), 1f, 1f, 1f)
            {
                Velocity = new Vector2(10f, 0f)
            };
            engine.AddBody(ball);

            // Tests basic unencumbered spatial translations mapping single frame evolutions generating expected vectors correctly mapping ghost trails smoothly later visually.
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

            // Analyzes continuous energy bleed emulating rough cloth resistance clamping exact values strictly resolving to zero avoiding perpetual micro-sliding.
            engine.Step(1f); 
            Assert.True(ball.Velocity.X > 0 && ball.Velocity.X < 1f);

            engine.Step(1f); 
            engine.Step(1f); 

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

            // Sets up a perfect Newton's cradle equivalent translating exactly 100% force across identical bodies cleanly freezing the initiator confirming vector transfers perfectly.
            engine.AddBody(ballA);
            engine.AddBody(ballB);

            engine.Step(0.01667f);

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

            // Validates non-linear reflections projecting force tangentially across bounding borders spreading energy along dual axis correctly mirroring glancing blows completely perfectly.
            Assert.True(Math.Abs(ballA.Velocity.Y) > 0.01f); 
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
            // Asserts defensive clipping routines actively pushing embedded boundaries far apart resolving overlap glitches cleanly without breaking geometry vectors explicitly.
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

            // Confirms edge absorption reducing kinetic values multiplying constraints by the assigned boundary elasticity projecting inverted velocity exactly mirroring real interactions neatly.
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

            // Enforces global checks locking game-states validating rigid stillness limits triggering turn closures flawlessly communicating exactly with the Rule Validators sequentially mapping outputs.
            Assert.True(engine.AreAllBodiesAtRest(0.051f)); 
            
            ball.Velocity = new Vector2(0.1f, 0f);
            Assert.False(engine.AreAllBodiesAtRest(0.051f)); 
            
            ball.Velocity = new Vector2(0.01f, 0f);
            Assert.True(engine.AreAllBodiesAtRest(0.051f)); 
        }
    }
}

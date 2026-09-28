using BilliardsGame.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BilliardsGame.Physics
{
    /// <summary>
    /// Implements the core physics simulation engine for the 2D billiards game.
    /// </summary>
    public class PhysicsEngine : IPhysicsEngine
    {
        private readonly List<IPhysicsBody> _bodies;
        private readonly List<IPocket> _pockets;
        private readonly float _tableFriction;
        private readonly float _sleepVelocityThreshold;

        private const int CollisionIterations = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicsEngine"/> class.
        /// </summary>
        /// <param name="tableFriction">The friction coefficient of the table cloth.</param>
        /// <param name="sleepVelocityThreshold">The velocity threshold below which a body is put to sleep (velocity set to zero).</param>
        public PhysicsEngine(float tableFriction = 0.25f, float sleepVelocityThreshold = 0.001f)
        {
            _bodies = new List<IPhysicsBody>();
            _pockets = new List<IPocket>();
            _tableFriction = tableFriction;
            _sleepVelocityThreshold = sleepVelocityThreshold;
        }

        /// <inheritdoc />
        public void AddBody(IPhysicsBody body)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            if (!_bodies.Contains(body))
            {
                _bodies.Add(body);
            }
        }

        /// <inheritdoc />
        public void RemoveBody(IPhysicsBody body)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            _bodies.Remove(body);
        }

        /// <inheritdoc />
        public IReadOnlyCollection<IPhysicsBody> GetBodies()
        {
            return _bodies.AsReadOnly();
        }

        /// <inheritdoc />
        public void AddPocket(IPocket pocket)
        {
            if (pocket == null) throw new ArgumentNullException(nameof(pocket));
            if (!_pockets.Contains(pocket))
            {
                _pockets.Add(pocket);
            }
        }

        /// <inheritdoc />
        public IReadOnlyCollection<IPocket> GetPockets()
        {
            return _pockets.AsReadOnly();
        }

        /// <inheritdoc />
        public bool AreAllBodiesAtRest(float sleepVelocityThreshold = 0.001f)
        {
            float sqrThreshold = sleepVelocityThreshold * sleepVelocityThreshold;
            foreach (var body in _bodies)
            {
                if (!body.IsStatic && body.Velocity.LengthSquared() >= sqrThreshold)
                {
                    return false;
                }
            }
            return true;
        }

        /// <inheritdoc />
        public void Step(float fixedDeltaTime)
        {
            if (fixedDeltaTime <= 0f) return;

            // 1. Save previous positions (for rendering interpolation)
            foreach (var body in _bodies)
            {
                body.PreviousPosition = body.Position;
            }

            // 2. Update velocities and apply friction (Drag)
            float frictionFactor = Math.Max(0f, 1f - _tableFriction * fixedDeltaTime);
            foreach (var body in _bodies)
            {
                if (body.IsStatic) continue;

                body.Velocity *= frictionFactor;

                // If velocity drops below the rest threshold, zero it out
                if (body.Velocity.LengthSquared() < _sleepVelocityThreshold * _sleepVelocityThreshold)
                {
                    body.Velocity = Vector2.Zero;
                }
            }

            // 3. Update positions (Backward Euler Integration)
            foreach (var body in _bodies)
            {
                if (body.IsStatic) continue;

                body.Position += body.Velocity * fixedDeltaTime;
            }

            // 4. Collision Detection and Resolution Phase
            for (int i = 0; i < CollisionIterations; i++)
            {
                ResolveCollisions();
            }

            // 5. Pocket Detection
            for (int i = _bodies.Count - 1; i >= 0; i--)
            {
                var body = _bodies[i];
                if (body.IsStatic) continue;

                foreach (var pocket in _pockets)
                {
                    float distSq = (body.Position - pocket.Position).LengthSquared();
                    if (distSq < pocket.Radius * pocket.Radius)
                    {
                        _bodies.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        private void ResolveCollisions()
        {
            for (int i = 0; i < _bodies.Count; i++)
            {
                var bodyA = _bodies[i];

                for (int j = i + 1; j < _bodies.Count; j++)
                {
                    var bodyB = _bodies[j];
                    
                    if (bodyA.IsStatic && bodyB.IsStatic) continue;

                    if (bodyA is ICircleBody circleA && bodyB is ICircleBody circleB)
                    {
                        ResolveBallBallCollision(circleA, circleB);
                    }
                    else if (bodyA is ICircleBody circleA2 && bodyB is ISegmentBody segmentB)
                    {
                        ResolveBallSegmentCollision(circleA2, segmentB);
                    }
                    else if (bodyB is ICircleBody circleB2 && bodyA is ISegmentBody segmentA)
                    {
                        ResolveBallSegmentCollision(circleB2, segmentA);
                    }
                }
            }
        }

        private void ResolveBallBallCollision(ICircleBody bodyA, ICircleBody bodyB)
        {
            Vector2 delta = bodyA.Position - bodyB.Position;
            float distSquared = delta.LengthSquared();
            float rSum = bodyA.Radius + bodyB.Radius;

            if (distSquared < rSum * rSum && distSquared > 0.0000000001f) // > 0.00001f squared approx
            {
                float dist = (float)Math.Sqrt(distSquared);
                if (dist <= 0f) dist = 0.00001f;
                Vector2 n = delta / dist;

                // Positional correction to prevent penetration
                float penetration = rSum - dist;
                Vector2 correction = n * (penetration * 0.5f);
                
                bodyA.Position += correction;
                bodyB.Position -= correction;

                // Relative velocity
                Vector2 relVel = bodyA.Velocity - bodyB.Velocity;
                float vn = Vector2.Dot(relVel, n);

                if (vn >= 0f) return;

                // Combined restitution
                float e = Math.Min(bodyA.Restitution, bodyB.Restitution);

                float invMassA = 1f / bodyA.Mass;
                float invMassB = 1f / bodyB.Mass;
                
                float j = -(1f + e) * vn / (invMassA + invMassB);
                Vector2 impulse = j * n;

                bodyA.ApplyImpulse(impulse);
                bodyB.ApplyImpulse(-impulse);
            }
        }

        private void ResolveBallSegmentCollision(ICircleBody ball, ISegmentBody segment)
        {
            Vector2 edge = segment.EndPoint - segment.StartPoint;
            float edgeLengthSq = edge.LengthSquared();

            if (edgeLengthSq < 0.000001f) return;

            float t = Vector2.Dot(ball.Position - segment.StartPoint, edge) / edgeLengthSq;
            t = Math.Clamp(t, 0f, 1f);

            Vector2 closestPoint = segment.StartPoint + t * edge;
            Vector2 d = ball.Position - closestPoint;
            float distSquared = d.LengthSquared();

            if (distSquared < ball.Radius * ball.Radius)
            {
                float dist = (float)Math.Sqrt(distSquared);
                Vector2 n = dist > 0.00001f ? d / dist : segment.Normal;

                // Positional correction
                float penetration = ball.Radius - dist;
                ball.Position += n * penetration;

                // Velocity along the normal
                float vn = Vector2.Dot(ball.Velocity, n);

                if (vn < 0f)
                {
                    float e = Math.Min(ball.Restitution, segment.Restitution);
                    float j = -(1f + e) * vn * ball.Mass;
                    
                    Vector2 impulse = j * n;
                    ball.ApplyImpulse(impulse);
                }
            }
        }
    }
}
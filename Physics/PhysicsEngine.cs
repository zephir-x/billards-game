using BilliardsGame.Interfaces;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace BilliardsGame.Physics
{
    /// <summary>
    /// Implements the core physics simulation engine for the 2D billiards game.
    /// </summary>
    public class PhysicsEngine : IPhysicsEngine
    {
        private readonly List<IPhysicsBody> _bodies;
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
        public bool AreAllBodiesAtRest(float sleepVelocityThreshold = 0.001f)
        {
            float sqrThreshold = sleepVelocityThreshold * sleepVelocityThreshold;
            bool allAtRest = true;

            foreach (var body in _bodies)
            {
                if (body.IsStatic) continue;

                if (body.Velocity.LengthSquared() < sqrThreshold)
                {
                    body.Velocity = Vector2.Zero;
                }
                else
                {
                    allAtRest = false;
                }
            }

            return allAtRest;
        }

        /// <inheritdoc />
        public void Step(float fixedDeltaTime)
        {
            if (fixedDeltaTime <= 0f) return;

            // 1. Zapis pozycji poprzedniej (dla interpolacji renderingu)
            foreach (var body in _bodies)
            {
                body.PreviousPosition = body.Position;
            }

            // 2. Aktualizacja prędkości i tarcie (Drag / Friction)
            float frictionFactor = Math.Max(0f, 1f - _tableFriction * fixedDeltaTime);
            foreach (var body in _bodies)
            {
                if (body.IsStatic) continue;

                body.Velocity *= frictionFactor;

                // Jeśli prędkość spadnie poniżej progu zatrzymania, wyzeruj ją
                if (body.Velocity.LengthSquared() < _sleepVelocityThreshold * _sleepVelocityThreshold)
                {
                    body.Velocity = Vector2.Zero;
                }
            }

            // 3. Aktualizacja pozycji (Wsteczne całkowanie Eulera)
            foreach (var body in _bodies)
            {
                if (body.IsStatic) continue;

                body.Position += body.Velocity * fixedDeltaTime;
            }

            // 4. Detekcja i Rozstrzyganie Kolizji (Collision Resolution Phase)
            for (int i = 0; i < CollisionIterations; i++)
            {
                ResolveCollisions();
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

                    if (!bodyA.IsStatic && !bodyB.IsStatic)
                    {
                        ResolveBallBallCollision(bodyA, bodyB);
                    }
                    else if (!bodyA.IsStatic && bodyB is Cushion cushionB)
                    {
                        ResolveBallCushionCollision(bodyA, cushionB);
                    }
                    else if (!bodyB.IsStatic && bodyA is Cushion cushionA)
                    {
                        ResolveBallCushionCollision(bodyB, cushionA);
                    }
                }
            }
        }

        private void ResolveBallBallCollision(IPhysicsBody bodyA, IPhysicsBody bodyB)
        {
            Vector2 delta = bodyA.Position - bodyB.Position;
            float distSquared = delta.LengthSquared();
            float rSum = bodyA.Radius + bodyB.Radius;

            if (distSquared < rSum * rSum && distSquared > 0.0000000001f) // > 0.00001f squared approx
            {
                float dist = (float)Math.Sqrt(distSquared);
                if (dist <= 0f) dist = 0.00001f;
                Vector2 n = delta / dist;

                // Korekcja penetracji (Positional Correction)
                float penetration = rSum - dist;
                Vector2 correction = n * (penetration * 0.5f);
                
                bodyA.Position += correction;
                bodyB.Position -= correction;

                // Prędkość względna
                Vector2 relVel = bodyA.Velocity - bodyB.Velocity;
                float vn = Vector2.Dot(relVel, n);

                if (vn >= 0f) return;

                // Połączona sprężystość
                float e = Math.Min(bodyA.Restitution, bodyB.Restitution);

                float invMassA = 1f / bodyA.Mass;
                float invMassB = 1f / bodyB.Mass;
                
                float j = -(1f + e) * vn / (invMassA + invMassB);
                Vector2 impulse = j * n;

                bodyA.ApplyImpulse(impulse);
                bodyB.ApplyImpulse(-impulse);
            }
        }

        private void ResolveBallCushionCollision(IPhysicsBody ball, Cushion cushion)
        {
            Vector2 edge = cushion.EndPoint - cushion.StartPoint;
            float edgeLengthSq = edge.LengthSquared();

            if (edgeLengthSq < 0.000001f) return;

            float t = Vector2.Dot(ball.Position - cushion.StartPoint, edge) / edgeLengthSq;
            t = Math.Clamp(t, 0f, 1f);

            Vector2 closestPoint = cushion.StartPoint + t * edge;
            Vector2 d = ball.Position - closestPoint;
            float distSquared = d.LengthSquared();

            if (distSquared < ball.Radius * ball.Radius)
            {
                float dist = (float)Math.Sqrt(distSquared);
                Vector2 n = dist > 0.00001f ? d / dist : cushion.Normal;

                // Korekcja pozycji
                float penetration = ball.Radius - dist;
                ball.Position += n * penetration;

                // Prędkość wzdłuż normalnej
                float vn = Vector2.Dot(ball.Velocity, n);

                if (vn < 0f)
                {
                    float e = Math.Min(ball.Restitution, cushion.Restitution);
                    float j = -(1f + e) * vn * ball.Mass;
                    
                    Vector2 impulse = j * n;
                    ball.ApplyImpulse(impulse);
                }
            }
        }
    }
}

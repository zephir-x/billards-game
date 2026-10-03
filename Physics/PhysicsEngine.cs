using BilliardsGame.Interfaces;
using BilliardsGame.Interfaces.Enums;
using BilliardsGame.Interfaces.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Drawing;

namespace BilliardsGame.Physics
{
    /// <summary>
    /// Implements the deterministic physics engine orchestrating spatial integration, kinematics, constraints, and collisions.
    /// Acts as the concrete implementation of <see cref=""IPhysicsEngine""/>.
    /// </summary>
    public class PhysicsEngine : IPhysicsEngine
    {
        private readonly List<IPhysicsBody> _bodies;
        private readonly List<IPhysicsBody> _ghostBodies;
        private readonly List<IPocket> _pockets;
        
        private readonly float _tableFriction;
        private readonly float _sleepVelocityThreshold;

        // Determines how many collision checks occur per frame. Higher = stiffer physical restitution.
        private const int CollisionIterations = 2;

        /// <inheritdoc />
        public StrokeData CurrentStrokeData { get; private set; }

        /// <inheritdoc />
        public event Action<float>? OnCollisionOccurred;

        /// <inheritdoc />
        public event Action<float>? OnCushionHit;

        /// <inheritdoc />
        public event Action? OnBallPocketed;

        /// <inheritdoc />
        public RectangleF GetPlayfieldBounds()
        {
            // Explicit hardcoded bounds used for determining valid Cue Ball placement boundaries.
            return new RectangleF(210f, 210f, 790f - 210f, 590f - 210f);
        }

        /// <summary>
        /// Initializes a new instance of the Engine configuring specific kinetic constraints.
        /// </summary>
        /// <param name=""tableFriction"">Constant drag applied per step to dynamic bodies.</param>
        /// <param name=""sleepVelocityThreshold"">Float cutoff treating microscopic energies as zero.</param>
        public PhysicsEngine(float tableFriction = 0.25f, float sleepVelocityThreshold = 0.001f)
        {
            _bodies = new List<IPhysicsBody>();
            _ghostBodies = new List<IPhysicsBody>();
            _pockets = new List<IPocket>();
            
            _tableFriction = tableFriction;
            _sleepVelocityThreshold = sleepVelocityThreshold;
            CurrentStrokeData = new StrokeData { SunkBallsIds = new List<int>() };
        }

        /// <inheritdoc />
        public void ResetStrokeData()
        {
            CurrentStrokeData = new StrokeData { SunkBallsIds = new List<int>() };
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
        public IReadOnlyCollection<IPhysicsBody> GetGhostBodies()
        {
            return _ghostBodies.AsReadOnly();
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

            // Step 1: Capture states mapping references for visual interpolation blending
            foreach (var body in _bodies)
            {
                body.PreviousPosition = body.Position;
            }

            // Step 2: Extract kinetic energy through dragging constraints
            float frictionFactor = Math.Max(0f, 1f - _tableFriction * fixedDeltaTime);
            foreach (var body in _bodies)
            {
                if (body.IsStatic) continue;

                body.Velocity *= frictionFactor;

                // Enforce mathematical rest if energy drops below cutoff threshold
                if (body.Velocity.LengthSquared() < _sleepVelocityThreshold * _sleepVelocityThreshold)
                {
                    body.Velocity = Vector2.Zero;
                }
            }

            // Step 3: Advance translation mapping by velocities
            foreach (var body in _bodies)
            {
                if (body.IsStatic) continue;

                body.Position += body.Velocity * fixedDeltaTime;
                
                // Emulate directional rolling mapping X/Y vectors along circular arc limits
                if (body is ICircleBody circle)
                {
                    circle.RotationAngle += ((body.Velocity.X + body.Velocity.Y) / circle.Radius) * fixedDeltaTime;
                }
            }
            
            // Step 4: Advance transient fading logic specifically for decaying sunk bodies
            for (int i = _ghostBodies.Count - 1; i >= 0; i--)
            {
                if (_ghostBodies[i] is ICircleBody gb)
                {
                    gb.GhostLifeTime -= fixedDeltaTime * 2.5f;
                    
                    IPocket? closestPocket = null;
                    float minDistSq = float.MaxValue;
                    
                    // Discover gravity-center relative to sunk holes simulating drop mechanics
                    foreach (var p in _pockets) 
                    {
                       float dSq = (p.Position - gb.Position).LengthSquared();
                       if (dSq < minDistSq) { minDistSq = dSq; closestPocket = p; }
                    }
                    
                    if (closestPocket != null) 
                    {
                       Vector2 toCenter = closestPocket.Position - gb.Position;
                       gb.Velocity += toCenter * 20f * fixedDeltaTime;
                       gb.Velocity *= Math.Max(0f, 1f - 4f * fixedDeltaTime);
                    }
                    
                    gb.PreviousPosition = gb.Position;
                    gb.Position += gb.Velocity * fixedDeltaTime;
                    gb.RotationAngle += ((gb.Velocity.X + gb.Velocity.Y) / gb.Radius) * fixedDeltaTime;

                    // Unload completely when lifecycle hits zero
                    if (gb.GhostLifeTime <= 0f)
                    {
                        _ghostBodies.RemoveAt(i);
                    }
                }
            }

            // Step 5: Iteratively enforce physical elastic rules
            for (int i = 0; i < CollisionIterations; i++)
            {
                ResolveCollisions();
            }

            // Step 6: Query proximity mapping on all pockets transitioning bodies resolving sinks
            for (int i = _bodies.Count - 1; i >= 0; i--)
            {
                var body = _bodies[i];
                if (body.IsStatic) continue;

                foreach (var pocket in _pockets)
                {
                    float distSq = (body.Position - pocket.Position).LengthSquared();
                    if (distSq < pocket.Radius * pocket.Radius)
                    {
                        OnBallPocketed?.Invoke();
                        CurrentStrokeData.SunkBallsIds.Add(body.Id);
                        CurrentStrokeData.SunkBallsToPockets[body.Id] = pocket;
                        
                        if (body is ICircleBody c)
                        {
                            c.IsGhost = true;
                            // Crush the kinetic momentum forcing the ball to visually fall downward rather than fly across
                            c.Velocity *= 0.02f; 
                            _ghostBodies.Add(c);
                        }
                        _bodies.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        private void ResolveCollisions()
        {
            // Evaluate N^2 pairs, restricting check complexity limits
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

            // Epsilon check mitigating division explosions
            if (distSquared < rSum * rSum && distSquared > 0.0000000001f)
            {
                float dist = (float)Math.Sqrt(distSquared);
                if (dist <= 0f) dist = 0.00001f;
                Vector2 n = delta / dist;

                // Penetration correction preserving stable constraints (separates overlapping surfaces)
                float penetration = rSum - dist;
                Vector2 correction = n * (penetration * 0.5f);
                
                bodyA.Position += correction;
                bodyB.Position -= correction;

                Vector2 relVel = bodyA.Velocity - bodyB.Velocity;
                float vn = Vector2.Dot(relVel, n);

                if (vn >= 0f) return;

                // Impulse mapping based on mass and material bounciness
                float e = Math.Min(bodyA.Restitution, bodyB.Restitution);
                float invMassA = 1f / bodyA.Mass;
                float invMassB = 1f / bodyB.Mass;
                
                float j = -(1f + e) * vn / (invMassA + invMassB);
                Vector2 impulse = j * n;

                OnCollisionOccurred?.Invoke(Math.Abs(j));
                bodyA.ApplyImpulse(impulse);
                bodyB.ApplyImpulse(-impulse);
                
                // Analytically register legal initial impacts for Domain-layer Rule assertions
                if (CurrentStrokeData.FirstBallHitId == null)
                {
                    if (bodyA.Id == 0)
                    {
                        CurrentStrokeData = new StrokeData 
                        { 
                            FirstBallHitId = bodyB.Id, 
                            RailsHitAfterContact = CurrentStrokeData.RailsHitAfterContact, 
                            SunkBallsIds = CurrentStrokeData.SunkBallsIds,
                            SunkBallsToPockets = CurrentStrokeData.SunkBallsToPockets
                        };
                    }
                    else if (bodyB.Id == 0)
                    {
                        CurrentStrokeData = new StrokeData 
                        { 
                            FirstBallHitId = bodyA.Id, 
                            RailsHitAfterContact = CurrentStrokeData.RailsHitAfterContact, 
                            SunkBallsIds = CurrentStrokeData.SunkBallsIds,
                            SunkBallsToPockets = CurrentStrokeData.SunkBallsToPockets
                        };
                    }
                }
            }
        }

        private void ResolveBallSegmentCollision(ICircleBody ball, ISegmentBody segment)
        {
            Vector2 edge = segment.EndPoint - segment.StartPoint;
            float edgeLengthSq = edge.LengthSquared();

            if (edgeLengthSq < 0.000001f) return;

            // Map projection scalar onto segment boundaries isolating closest clamping coordinate
            float t = Vector2.Dot(ball.Position - segment.StartPoint, edge) / edgeLengthSq;
            t = Math.Clamp(t, 0f, 1f);

            Vector2 closestPoint = segment.StartPoint + t * edge;
            Vector2 d = ball.Position - closestPoint;
            float distSquared = d.LengthSquared();

            if (distSquared < ball.Radius * ball.Radius)
            {
                float dist = (float)Math.Sqrt(distSquared);
                Vector2 n = dist > 0.00001f ? d / dist : segment.Normal;

                float penetration = ball.Radius - dist;
                ball.Position += n * penetration;

                float vn = Vector2.Dot(ball.Velocity, n);

                if (vn < 0f)
                {
                    float e = Math.Min(ball.Restitution, segment.Restitution);
                    float j = -(1f + e) * vn * ball.Mass;
                    
                    Vector2 impulse = j * n;
                    OnCushionHit?.Invoke(Math.Abs(j));
                    ball.ApplyImpulse(impulse);

                    // Increment rail checks exclusively following an initial body collision sequence
                    if (CurrentStrokeData.FirstBallHitId != null)
                    {
                        CurrentStrokeData = new StrokeData 
                        { 
                            FirstBallHitId = CurrentStrokeData.FirstBallHitId,
                            RailsHitAfterContact = CurrentStrokeData.RailsHitAfterContact + 1,
                            SunkBallsIds = CurrentStrokeData.SunkBallsIds,
                            SunkBallsToPockets = CurrentStrokeData.SunkBallsToPockets
                        };
                    }
                }
            }
        }
    }
}

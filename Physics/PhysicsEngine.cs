using BilliardsGame.Interfaces;
using BilliardsGame.Interfaces.Enums;
using BilliardsGame.Interfaces.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BilliardsGame.Physics
{
    public class PhysicsEngine : IPhysicsEngine
    {
        private readonly List<IPhysicsBody> _bodies;
        private readonly List<IPhysicsBody> _ghostBodies;
        private readonly List<IPocket> _pockets;
        private readonly float _tableFriction;
        private readonly float _sleepVelocityThreshold;

        private const int CollisionIterations = 2;

        public StrokeData CurrentStrokeData { get; private set; }

        public event Action<float>? OnCollisionOccurred;
        public event Action<float>? OnCushionHit;
        public event Action? OnBallPocketed;

        public System.Drawing.RectangleF GetPlayfieldBounds()
        {
            return new System.Drawing.RectangleF(210f, 210f, 790f - 210f, 590f - 210f);
        }

        public PhysicsEngine(float tableFriction = 0.25f, float sleepVelocityThreshold = 0.001f)
        {
            _bodies = new List<IPhysicsBody>();
            _ghostBodies = new List<IPhysicsBody>();
            _pockets = new List<IPocket>();
            _tableFriction = tableFriction;
            _sleepVelocityThreshold = sleepVelocityThreshold;
            CurrentStrokeData = new StrokeData { SunkBallsIds = new List<int>() };
        }

        public void ResetStrokeData()
        {
            CurrentStrokeData = new StrokeData { SunkBallsIds = new List<int>() };
        }

        public void AddBody(IPhysicsBody body)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            if (!_bodies.Contains(body))
            {
                _bodies.Add(body);
            }
        }

        public void RemoveBody(IPhysicsBody body)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            _bodies.Remove(body);
        }

        public IReadOnlyCollection<IPhysicsBody> GetBodies()
        {
            return _bodies.AsReadOnly();
        }
        
        public IReadOnlyCollection<IPhysicsBody> GetGhostBodies()
        {
            return _ghostBodies.AsReadOnly();
        }

        public void AddPocket(IPocket pocket)
        {
            if (pocket == null) throw new ArgumentNullException(nameof(pocket));
            if (!_pockets.Contains(pocket))
            {
                _pockets.Add(pocket);
            }
        }

        public IReadOnlyCollection<IPocket> GetPockets()
        {
            return _pockets.AsReadOnly();
        }

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

        public void Step(float fixedDeltaTime)
        {
            if (fixedDeltaTime <= 0f) return;

            foreach (var body in _bodies)
            {
                body.PreviousPosition = body.Position;
            }

            float frictionFactor = Math.Max(0f, 1f - _tableFriction * fixedDeltaTime);
            foreach (var body in _bodies)
            {
                if (body.IsStatic) continue;

                body.Velocity *= frictionFactor;

                if (body.Velocity.LengthSquared() < _sleepVelocityThreshold * _sleepVelocityThreshold)
                {
                    body.Velocity = Vector2.Zero;
                }
            }

            foreach (var body in _bodies)
            {
                if (body.IsStatic) continue;

                body.Position += body.Velocity * fixedDeltaTime;
                
                if (body is ICircleBody circle)
                {
                    circle.RotationAngle += ((body.Velocity.X + body.Velocity.Y) / circle.Radius) * fixedDeltaTime;
                }
            }
            
            for (int i = _ghostBodies.Count - 1; i >= 0; i--)
            {
                if (_ghostBodies[i] is ICircleBody gb)
                {
                    gb.FadeTimer -= fixedDeltaTime * 2.5f;
                    
                    IPocket? closestPocket = null;
                    float minDistSq = float.MaxValue;
                    foreach(var p in _pockets) {
                       float dSq = (p.Position - gb.Position).LengthSquared();
                       if (dSq < minDistSq) { minDistSq = dSq; closestPocket = p; }
                    }
                    
                    if (closestPocket != null) {
                       Vector2 toCenter = closestPocket.Position - gb.Position;
                       gb.Velocity += toCenter * 20f * fixedDeltaTime;
                       gb.Velocity *= Math.Max(0f, 1f - 4f * fixedDeltaTime);
                    }
                    
                    gb.PreviousPosition = gb.Position;
                    gb.Position += gb.Velocity * fixedDeltaTime;
                    gb.RotationAngle += ((gb.Velocity.X + gb.Velocity.Y) / gb.Radius) * fixedDeltaTime;

                    if (gb.FadeTimer <= 0f)
                    {
                        _ghostBodies.RemoveAt(i);
                    }
                }
            }

            for (int i = 0; i < CollisionIterations; i++)
            {
                ResolveCollisions();
            }

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
                            c.Velocity *= 0.02f; // Completely crush the momentum! Takes away 98% of its velocity.
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

                OnCollisionOccurred?.Invoke(Math.Abs(j));
                bodyA.ApplyImpulse(impulse);
                bodyB.ApplyImpulse(-impulse);
                
                // Track FirstBallHitId if not set, 0 = cue ball
                if (CurrentStrokeData.FirstBallHitId == null)
                {
                    if (bodyA.Id == 0)
                    {
                        CurrentStrokeData = new StrokeData 
                        { 
                            FirstBallHitId = bodyB.Id, 
                            RailsHitAfterContact = CurrentStrokeData.RailsHitAfterContact, 
                            SunkBallsIds = CurrentStrokeData.SunkBallsIds 
                        };
                    }
                    else if (bodyB.Id == 0)
                    {
                        CurrentStrokeData = new StrokeData 
                        { 
                            FirstBallHitId = bodyA.Id, 
                            RailsHitAfterContact = CurrentStrokeData.RailsHitAfterContact, 
                            SunkBallsIds = CurrentStrokeData.SunkBallsIds 
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
                    OnCushionHit?.Invoke(Math.Abs(j));
                    ball.ApplyImpulse(impulse);

                    if (CurrentStrokeData.FirstBallHitId != null)
                    {
                        CurrentStrokeData = new StrokeData 
                        { 
                            FirstBallHitId = CurrentStrokeData.FirstBallHitId,
                            RailsHitAfterContact = CurrentStrokeData.RailsHitAfterContact + 1,
                            SunkBallsIds = CurrentStrokeData.SunkBallsIds
                        };
                    }
                }
            }
        }
    }
}

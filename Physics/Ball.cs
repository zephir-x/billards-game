using System.Numerics;

namespace BilliardsGame.Physics
{
    /// <summary>
    /// Represents a dynamic circle colliding body.
    /// </summary>
    public class Ball : IPhysicsBody
    {
        public int Id { get; private set; }
        public Vector2 Position { get; set; }
        public Vector2 PreviousPosition { get; set; }
        public Vector2 Velocity { get; set; }
        
        public float Mass { get; private set; }
        public float Restitution { get; private set; }
        public float Radius { get; private set; }
        
        public bool IsStatic => false;

        public Ball(int id, Vector2 initialPosition, float mass, float radius, float restitution)
        {
            Id = id;
            Position = initialPosition;
            PreviousPosition = initialPosition;
            Mass = mass;
            Radius = radius;
            Restitution = restitution;
            Velocity = Vector2.Zero;
        }

        public void ApplyImpulse(Vector2 impulse)
        {
            // Empty implementation as requested
        }
    }
}

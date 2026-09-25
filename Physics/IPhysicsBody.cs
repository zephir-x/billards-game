using System.Numerics;

namespace BilliardsGame.Physics
{
    /// <summary>
    /// Represents a physical entity within the physics simulation.
    /// </summary>
    public interface IPhysicsBody
    {
        int Id { get; }
        Vector2 Position { get; set; }
        Vector2 PreviousPosition { get; set; }
        Vector2 Velocity { get; set; }
        float Mass { get; }
        float Restitution { get; }
        float Radius { get; }
        bool IsStatic { get; }
        
        void ApplyImpulse(Vector2 impulse);
    }
}

using BilliardsGame.Interfaces.Models;
using System.Collections.Generic;
using System.Numerics;

namespace BilliardsGame.Interfaces
{
    /// <summary>
    /// Core physics engine interface responsible for deterministic calculations.
    /// </summary>
    public interface IPhysicsEngine
    {
        StrokeData CurrentStrokeData { get; }

        System.Drawing.RectangleF GetPlayfieldBounds();

        void ResetStrokeData();

        /// <summary>
        /// Calculates positions, movement, and collisions after a fixed time step.
        /// </summary>
        /// <param name="fixedDeltaTime">The duration of the simulation step in seconds.</param>
        void Step(float fixedDeltaTime);

        /// <summary>
        /// Adds a physical body to the physics simulation.
        /// </summary>
        /// <param name="body">The physics body to register.</param>
        void AddBody(IPhysicsBody body);

        /// <summary>
        /// Removes a physical body from the physics simulation.
        /// </summary>
        /// <param name="body">The physics body to unregister.</param>
        void RemoveBody(IPhysicsBody body);

        /// <summary>
        /// Returns all physics bodies currently registered in the simulation.
        /// </summary>
        /// <returns>A read-only collection of physics bodies.</returns>
        IReadOnlyCollection<IPhysicsBody> GetBodies();

        /// <summary>
        /// Returns all dying/ghost physics bodies currently fading out.
        /// </summary>
        IReadOnlyCollection<IPhysicsBody> GetGhostBodies();

        /// <summary>
        /// Adds a pocket to the simulation to trap balls.
        /// </summary>
        void AddPocket(IPocket pocket);

        /// <summary>
        /// Returns all pockets.
        /// </summary>
        IReadOnlyCollection<IPocket> GetPockets();

        /// <summary>
        /// Determines whether all bodies in the simulation have essentially stopped moving.
        /// </summary>
        /// <param name="sleepVelocityThreshold">The velocity magnitude below which a body is considered at rest.</param>
        /// <returns><c>true</c> if all bodies are at rest; otherwise, <c>false</c>.</returns>
        bool AreAllBodiesAtRest(float sleepVelocityThreshold = 0.001f);
    }
}
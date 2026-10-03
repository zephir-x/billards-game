using BilliardsGame.Interfaces.Models;
using System.Collections.Generic;
using System.Drawing;
using System;

namespace BilliardsGame.Interfaces
{
    /// <summary>
    /// Core simulation constraint engine responsible for highly deterministic kinematics and collision evaluation.
    /// </summary>
    public interface IPhysicsEngine
    {
        /// <summary>
        /// Global read-only packet aggregating exact chronological hit and pocket statistics produced over one simulated execution.
        /// </summary>
        StrokeData CurrentStrokeData { get; }

        /// <summary>
        /// Dispatched synchronously when dynamic bodies exchange physical kinetic forces.
        /// </summary>
        event Action<float>? OnCollisionOccurred;

        /// <summary>
        /// Dispatched synchronously when dynamic bodies reflect and impact against static boundary voids.
        /// </summary>
        event Action<float>? OnCushionHit;

        /// <summary>
        /// Dispatched synchronously when dynamic bodies overlap and succumb to designated pocket boundaries.
        /// </summary>
        event Action? OnBallPocketed;

        /// <summary>
        /// Scans geometric cushions mapping an outer envelope threshold defining the legal play area bounds.
        /// </summary>
        RectangleF GetPlayfieldBounds();

        /// <summary>
        /// Explicity flushes current StrokeData aggregations resetting analytical states for the next player execution.
        /// </summary>
        void ResetStrokeData();

        /// <summary>
        /// Calculates positions, movement trajectories, and enforces collision restrictions across a fixed temporal horizon.
        /// </summary>
        /// <param name="fixedDeltaTime">The constant duration of the integration step relative to seconds.</param>
        void Step(float fixedDeltaTime);

        /// <summary>
        /// Registers a physical body internally into the integration loop pipeline.
        /// </summary>
        /// <param name="body">The entity to simulate.</param>
        void AddBody(IPhysicsBody body);

        /// <summary>
        /// Extracts and detaches a physical body from the internal pipeline.
        /// </summary>
        /// <param name="body">The entity to detach.</param>
        void RemoveBody(IPhysicsBody body);

        /// <summary>
        /// Scans all active interacting entity bodies contained and validated within the loop pipeline.
        /// </summary>
        /// <returns>A read-only iterable snapshot collection of dynamic constraints.</returns>
        IReadOnlyCollection<IPhysicsBody> GetBodies();

        /// <summary>
        /// Scans all deactivated non-collidable entity bodies resolving their remaining aesthetic physical drifts.
        /// </summary>
        /// <returns>A read-only iterable snapshot collection of ghost constraints.</returns>
        IReadOnlyCollection<IPhysicsBody> GetGhostBodies();

        /// <summary>
        /// Registers a discrete geometric trap designed to absorb boundaries into ghost sequences.
        /// </summary>
        void AddPocket(IPocket pocket);

        /// <summary>
        /// Retrieves aggregated geometric limits classifying target destinations.
        /// </summary>
        IReadOnlyCollection<IPocket> GetPockets();

        /// <summary>
        /// Scans active linear kinematics across bodies to mathematically determine simulation resolutions.
        /// </summary>
        /// <param name="sleepVelocityThreshold">The float magnitude ceiling treating motion outputs as mathematically stagnant.</param>
        /// <returns><c>true</c> if magnitudes dictate rest state; otherwise, <c>false</c>.</returns>
        bool AreAllBodiesAtRest(float sleepVelocityThreshold = 0.001f);
    }
}

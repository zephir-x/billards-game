using System;
using System.Numerics;

namespace BilliardsGame.Interfaces
{
    /// <summary>
    /// Abstracts underlying raw hardware peripheral events and buffers state for uniform processing by the domain layer.
    /// </summary>
    public interface IInputProvider
    {
        /// <summary>
        /// Injects a mathematical translation delegate dynamically converting hardware view pixels into world metrics.
        /// </summary>
        /// <param name="screenToWorldMapper">Translation function taking Screen vector and outputting World vector.</param>
        void SetCoordinateMapper(Func<Vector2, Vector2> screenToWorldMapper);

        /// <summary>
        /// Represents the current physical cursor coordinate translated into physics engine space.
        /// </summary>
        Vector2 MouseWorldPosition { get; }

        /// <summary>
        /// Evaluates strictly to true only on the first frame a primary confirmation sequence (Left click) is initiated.
        /// </summary>
        bool WasLeftMousePressed { get; }

        /// <summary>
        /// Evaluates to true for the entire consecutive duration the primary input (Left click) is sustained.
        /// </summary>
        bool IsLeftMouseDown { get; }

        /// <summary>
        /// Evaluates strictly to true only on the immediate frame following a sustained sequence termination (Release).
        /// </summary>
        bool WasLeftMouseReleased { get; }
    }
}

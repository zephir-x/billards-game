using BilliardsGame.Interfaces.Enums;
using System.Collections.Generic;
using System.Numerics;

namespace BilliardsGame.Interfaces
{
    /// <summary>
    /// Operates as a completely safe read-only payload wrapper mapping domain complexities into pure visual presentation references.
    /// </summary>
    public interface ISceneParameters
    {
        /// <summary>
        /// Global state representing the currently active phase of the game loop mapping UI layers.
        /// </summary>
        GameState CurrentState { get; }

        /// <summary>
        /// Maps identity rendering semantics (like names or scores) of the player controlling the flow.
        /// </summary>
        IPlayer? CurrentTurnPlayer { get; }

        /// <summary>
        /// Captures terminal states showing the ultimate player reference owning the win condition.
        /// </summary>
        IPlayer? Winner { get; }

        /// <summary>
        /// Physical aggregation representing valid bodies on the play field.
        /// </summary>
        IReadOnlyCollection<IPhysicsBody>? Bodies { get; }

        /// <summary>
        /// In-memory temporal objects mapped exclusively for sinking/dying animation loops.
        /// </summary>
        IReadOnlyCollection<IPhysicsBody>? GhostBodies { get; }

        /// <summary>
        /// Represent positional constraints mapped on static cushions representing table voids.
        /// </summary>
        IReadOnlyCollection<IPocket>? Pockets { get; }

        /// <summary>
        /// Read-only cue constraints used to visualize aim trajectory vectors and power intensities.
        /// </summary>
        ICueController? CueInfo { get; }

        /// <summary>
        /// Immutable reference to the primary local player instance.
        /// </summary>
        IPlayer Player1 { get; }

        /// <summary>
        /// Immutable reference to the primary local opposing player instance.
        /// </summary>
        IPlayer Player2 { get; }
        
        /// <summary>
        /// High-level text payload of the active notification banner designed for immediate renderer injection.
        /// </summary>
        string NotificationMessage { get; }

        /// <summary>
        /// Active duration calculation value for animating transparency transitions on banners.
        /// </summary>
        float NotificationTimer { get; }

        /// <summary>
        /// Semantic intent of the UI banner used directly to allocate appropriate dynamic Hex shades.
        /// </summary>
        NotificationType NotificationType { get; }
        
        /// <summary>
        /// Target pocket implicitly highlighted under CallingPocket game states representing valid win routes.
        /// </summary>
        IPocket? TargetPocket { get; }

        /// <summary>
        /// Facilitates queries calculating whether the ghosted white ball cursor overlaps impossible dimensions (cushions or balls).
        /// </summary>
        /// <param name="position">Checked world-space coordinates.</param>
        /// <returns>True against completely valid surfaces, false if intersecting geometries.</returns>
        bool IsValidCueBallPlacement(Vector2 position);
    }
}

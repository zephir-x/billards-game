using System;
using System.Numerics;
using BilliardsGame.Interfaces.Enums;

namespace BilliardsGame.Interfaces
{
    /// <summary>
    /// Acts as the core domain supervisor managing the game states, rule validators, and turn progression.
    /// </summary>
    public interface IGameManager
    {
        /// <summary>
        /// Global state representing the currently active phase of the game loop.
        /// </summary>
        GameState CurrentState { get; }

        /// <summary>
        /// Represents the player whose turn is currently active.
        /// </summary>
        IPlayer ActivePlayer { get; }

        /// <summary>
        /// Contains the winning player reference if the game has concluded, otherwise null.
        /// </summary>
        IPlayer? Winner { get; }
        
        /// <summary>
        /// Read-only access to the first player entity.
        /// </summary>
        IPlayer Player1 { get; }

        /// <summary>
        /// Read-only access to the second player entity.
        /// </summary>
        IPlayer Player2 { get; }
        
        /// <summary>
        /// High-level text payload of the active notification banner.
        /// </summary>
        string NotificationMessage { get; }

        /// <summary>
        /// Remaining active duration in seconds for the current notification banner.
        /// </summary>
        float NotificationTimer { get; }

        /// <summary>
        /// Indicates the semantic intent context of the UI notification banner.
        /// </summary>
        NotificationType NotificationType { get; }

        /// <summary>
        /// The explicitly called pocket chosen by a player during the 8-Ball ending phase.
        /// </summary>
        IPocket? TargetPocket { get; }

        /// <summary>
        /// Assigns the designated pocket for the 8-ball validation rule.
        /// </summary>
        /// <param name="pocket">The pocket instance selected through the provider UI.</param>
        void SelectTargetPocket(IPocket pocket);

        /// <summary>
        /// Scans playfield constraints querying the physics engine to determine if cursor coordinates represent a strictly valid Ball-in-Hand placement.
        /// </summary>
        /// <param name="position">Checked world-space coordinates.</param>
        /// <returns>True if the coordinates do not overlap cushions or other balls, otherwise false.</returns>
        bool IsValidCueBallPlacement(Vector2 position);

        /// <summary>
        /// Advances the logical conditions, validates states and processes sequential rule pipelines.
        /// </summary>
        /// <param name="deltaTime">Time passed since the last logical frame.</param>
        void UpdateLogic(float deltaTime);

        /// <summary>
        /// Refreshes all internal state parameters enforcing a fresh match.
        /// </summary>
        void StartGame();

        /// <summary>
        /// Unconditionally terminates the active player's turn and yields control.
        /// </summary>
        void EndTurn();

        /// <summary>
        /// Dispatched synchronously when a foul condition results in standard cue ball scratch.
        /// </summary>
        event Action OnScratchFoul;

        /// <summary>
        /// Dispatched synchronously when the active player places the cue ball on a valid world coordinate.
        /// </summary>
        event Action<Vector2> OnPlaceCueBall;

        /// <summary>
        /// Dispatched synchronously when terminal game over rule conditions are met. (Win/Lose context held in Winner property).
        /// </summary>
        event Action OnGameOver;

        /// <summary>
        /// Emitted asynchronously when a new banner notification is queued by the semantic domain.
        /// </summary>
        event Action<NotificationType> OnNotificationEvent;
    }
}

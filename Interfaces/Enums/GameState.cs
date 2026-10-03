namespace BilliardsGame.Interfaces.Enums
{
    /// <summary>
    /// Explicitly maps the active sequence machine boundary of the overall game lifecycle loop.
    /// </summary>
    public enum GameState 
    {
        /// <summary>
        /// Initial application wait-state prompting the user to start or exit.
        /// </summary>
        Menu, 

        /// <summary>
        /// Passive phase evaluating active cursor positions awaiting a charge initiation.
        /// </summary>
        PlayerTurn, 

        /// <summary>
        /// Interrupted holding phase where cue power expands relative to elapsed delta times.
        /// </summary>
        ChargingShot, 

        /// <summary>
        /// Active engine state executing integration steps until all bodies achieve rest.
        /// </summary>
        SimulatingBalls, 

        /// <summary>
        /// Penalty sequence locking progression until a player verifies a valid Cue Ball spawn coordinate.
        /// </summary>
        BallInHand,

        /// <summary>
        /// Terminal state resolving match victors and suspending core integrations.
        /// </summary>
        GameOver,

        /// <summary>
        /// Intermediate decision loop expecting cursor-click pocket verification indicating terminal Black Ball trajectory intention.
        /// </summary>
        CallingPocket 
    }
}

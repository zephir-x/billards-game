namespace BilliardsGame.Interfaces.Enums
{
    /// <summary>
    /// Encodes validated return statuses outputted by the internal 8-Ball Rule evaluation constraints.
    /// </summary>
    public enum RuleResult
    {
        /// <summary>
        /// A valid play occurred prompting the active player to stroke again.
        /// </summary>
        Continue,

        /// <summary>
        /// A play resolved safely but failed to satisfy 'Continue' criteria, yielding the turn.
        /// </summary>
        TurnLost,

        /// <summary>
        /// Represents rule violations triggering penalty conditions (e.g. Ball-in-Hand).
        /// </summary>
        Foul,

        /// <summary>
        /// Terminal indication the active player satisfied ultimate winning criteria.
        /// </summary>
        GameOverWin,

        /// <summary>
        /// Terminal indication the active player engaged a fatal loss state.
        /// </summary>
        GameOverLose
    }
}

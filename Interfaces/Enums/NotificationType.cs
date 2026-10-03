namespace BilliardsGame.Interfaces.Enums
{
    /// <summary>
    /// Explicitly maps semantic context boundaries guiding visual translations in presentation layers.
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// General feedback like turn assignments or informational texts.
        /// </summary>
        Info,
        
        /// <summary>
        /// Penalty notifications reflecting broken rules triggering ball-in-hand constraints.
        /// </summary>
        Foul,
        
        /// <summary>
        /// High-priority notifications requesting active manual input choices (ex: calling a pocket).
        /// </summary>
        Decision
    }
}

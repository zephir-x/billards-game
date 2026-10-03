using BilliardsGame.Interfaces.Enums;

namespace BilliardsGame.Interfaces
{
    /// <summary>
    /// Represents competitive profile states mapped logically to a user session or AI controller during the match sequence.
    /// </summary>
    public interface IPlayer
    {
        /// <summary>
        /// Unique integer identifying the player instance internally. Typically 1 or 2.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Human-readable label designated for visualization via UI layer.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Contextual variant definition mapping the player to Solid or Striped balls, allocated after first legal strike constraint evaluation.
        /// </summary>
        BallType? AssignedType { get; set; }

        /// <summary>
        /// Dynamic scoring aggregate capturing the amount of own valid type balls successfully deposited.
        /// </summary>
        int Score { get; set; }
    }
}

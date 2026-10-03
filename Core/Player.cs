using BilliardsGame.Interfaces;
using BilliardsGame.Interfaces.Enums;

namespace BilliardsGame.Core
{
    /// <summary>
    /// Represents a concrete player entity engaged in the billiard match holding progress and states.
    /// </summary>
    public class Player : IPlayer
    {
        /// <inheritdoc />
        public int Id { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public BallType? AssignedType { get; set; }

        /// <inheritdoc />
        public int Score { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref=""Player""/> entity.
        /// </summary>
        /// <param name=""id"">Internal numerical identifier.</param>
        /// <param name=""name"">Human readable title for the presentation layer.</param>
        public Player(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}

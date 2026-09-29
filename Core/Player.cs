using BilliardsGame.Interfaces;

namespace BilliardsGame.Core
{
    public class Player : IPlayer
    {
        public int Id { get; }
        public string Name { get; }
        public BallType? AssignedType { get; set; }

        public Player(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
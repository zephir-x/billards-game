using BilliardsGame.Interfaces;
using BilliardsGame.Interfaces.Enums;
using BilliardsGame.Interfaces.Models;

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

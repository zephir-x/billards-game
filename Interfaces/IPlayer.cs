namespace BilliardsGame.Interfaces
{
    public interface IPlayer
    {
        int Id { get; }
        string Name { get; }
        BallType? AssignedType { get; set; }
    }
}
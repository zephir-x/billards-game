using System.Collections.Generic;

namespace BilliardsGame.Interfaces
{
    public interface ISceneParameters
    {
        GameState CurrentState { get; }
        IPlayer CurrentTurnPlayer { get; }
        IReadOnlyCollection<IPhysicsBody> Bodies { get; }
        IReadOnlyCollection<IPocket> Pockets { get; }
        ICueController CueInfo { get; }
    }
}

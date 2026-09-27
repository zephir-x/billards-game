using System.Collections.Generic;

namespace BilliardsGame.Interfaces
{
    public interface ISceneParameters
    {
        GameState CurrentState { get; }
        IPlayer CurrentTurnPlayer { get; }
        IReadOnlyCollection<IPhysicsBody> Bodies { get; }
        ICueController CueInfo { get; }
    }
}

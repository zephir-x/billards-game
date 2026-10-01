using BilliardsGame.Interfaces.Enums;
using System.Collections.Generic;

namespace BilliardsGame.Interfaces
{
    public interface ISceneParameters
    {
        GameState CurrentState { get; }
        IPlayer? CurrentTurnPlayer { get; }
        IPlayer? Winner { get; }
        IReadOnlyCollection<IPhysicsBody>? Bodies { get; }
        IReadOnlyCollection<IPhysicsBody>? GhostBodies { get; }
        IReadOnlyCollection<IPocket>? Pockets { get; }
        ICueController? CueInfo { get; }

        IPlayer Player1 { get; }
        IPlayer Player2 { get; }
        
        string NotificationMessage { get; }
        float NotificationTimer { get; }
        NotificationType NotificationType { get; }
        
        IPocket? TargetPocket { get; }
        bool IsValidCueBallPlacement(System.Numerics.Vector2 position);
    }
}

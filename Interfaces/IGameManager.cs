using BilliardsGame.Interfaces.Enums;

namespace BilliardsGame.Interfaces
{
    public interface IGameManager
    {
        GameState CurrentState { get; }
        IPlayer ActivePlayer { get; }
        IPlayer? Winner { get; }
        
        IPlayer Player1 { get; }
        IPlayer Player2 { get; }
        
        string NotificationMessage { get; }
        float NotificationTimer { get; }
        NotificationType NotificationType { get; }
        IPocket? TargetPocket { get; }

        void SelectTargetPocket(IPocket pocket);
        bool IsValidCueBallPlacement(System.Numerics.Vector2 position);
        void UpdateLogic(float deltaTime);
        void StartGame();
        void EndTurn();
        event System.Action OnScratchFoul;
        event System.Action<System.Numerics.Vector2> OnPlaceCueBall;
        event System.Action OnGameOver;
        event System.Action<NotificationType> OnNotificationEvent;
    }
}

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
        string NotificationColorHex { get; }
        IPocket? TargetPocket { get; }

        void SelectTargetPocket(IPocket pocket);
        void UpdateLogic(float deltaTime);
        void StartGame();
        void EndTurn();
        event System.Action OnScratchFoul;
        event System.Action<System.Numerics.Vector2> OnPlaceCueBall;
        event System.Action OnGameOver;
        event System.Action<bool> OnNotificationEvent;
    }
}

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
        
        string FoulMessage { get; }
        float FoulMessageTimer { get; }

        void UpdateLogic(float deltaTime);
        void StartGame();
        void EndTurn();
        event System.Action OnScratchFoul;
        event System.Action<System.Numerics.Vector2> OnPlaceCueBall;
    }
}
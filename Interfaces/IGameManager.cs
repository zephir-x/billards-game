namespace BilliardsGame.Interfaces
{
    public enum GameState 
    { 
        Menu, 
        PlayerTurn, 
        ChargingShot, 
        SimulatingBalls, 
        GameOver 
    }

    public interface IGameManager
    {
        GameState CurrentState { get; }
        IPlayer ActivePlayer { get; }
        IPlayer? Winner { get; }
        void UpdateLogic(float deltaTime);
        void StartGame();
        void EndTurn();
        event System.Action OnScratchFoul;
    }
}


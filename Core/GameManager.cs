using System.Linq;
using BilliardsGame.Interfaces;

namespace BilliardsGame.Core
{
    public class GameManager : IGameManager
    {
        public GameState CurrentState { get; private set; }
        public IPlayer ActivePlayer { get; private set; }

        private readonly IPhysicsEngine _physicsEngine;
        private readonly ICueController _cueController;
        private readonly IInputProvider _inputProvider;

        private readonly IPlayer _player1;
        private readonly IPlayer _player2;

        public GameManager(
            IPhysicsEngine physicsEngine,
            ICueController cueController,
            IInputProvider inputProvider)
        {
            _physicsEngine = physicsEngine;
            _cueController = cueController;
            _inputProvider = inputProvider;

            _player1 = new Player(1, "Player 1");
            _player2 = new Player(2, "Player 2");

            CurrentState = GameState.Menu;
            ActivePlayer = _player1;
        }

        public void StartGame()
        {
            CurrentState = GameState.PlayerTurn;
            ActivePlayer = _player1;
        }

        public void EndTurn()
        {
            ActivePlayer = ActivePlayer == _player1 ? _player2 : _player1;
            CurrentState = GameState.PlayerTurn;
        }

        public void UpdateLogic(float deltaTime)
        {
            switch (CurrentState)
            {
                case GameState.Menu:
                    // Waiting for StartGame() to be called from the outside
                    break;

                case GameState.PlayerTurn:
                {
                    var cueBall = GetCueBall();
                    if (cueBall != null)
                    {
                        _cueController.UpdateAim(cueBall.Position, _inputProvider.MouseWorldPosition);
                    }

                    if (_inputProvider.IsLeftMouseDown)
                    {
                        CurrentState = GameState.ChargingShot;
                    }
                    break;
                }

                case GameState.ChargingShot:
                {
                    var cueBall = GetCueBall();
                    if (cueBall != null)
                    {
                        _cueController.UpdateAim(cueBall.Position, _inputProvider.MouseWorldPosition);
                    }

                    _cueController.ChargeShot(deltaTime);

                    if (_cueController.Power == 0.0f) // Overcharge reset
                    {
                        CurrentState = GameState.PlayerTurn;
                    }
                    else if (_inputProvider.WasLeftMouseReleased)
                    {
                        if (cueBall != null)
                        {
                            _cueController.ExecuteShot(cueBall);
                        }
                        CurrentState = GameState.SimulatingBalls;
                    }
                    break;
                }

                case GameState.SimulatingBalls:
                {
                    if (_physicsEngine.AreAllBodiesAtRest())
                    {
                        EndTurn();
                    }
                    break;
                }
                
                case GameState.GameOver:
                    break;
            }
        }

        private IPhysicsBody? GetCueBall()
        {
            var bodies = _physicsEngine.GetBodies();
            if (bodies == null)
            {
                return null;
            }
            return bodies.FirstOrDefault(b => b.Id == 0);
        }
    }
}

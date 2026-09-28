using System;
using System.Linq;
using System.Numerics;
using BilliardsGame.Interfaces;

namespace BilliardsGame.Core
{
    public class GameManager : IGameManager
    {
        public GameState CurrentState { get; private set; }
        public IPlayer ActivePlayer { get; private set; }
        public IPlayer? Winner { get; private set; }

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
            Winner = null;
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
                    _cueController.UpdateOverheat(deltaTime);

                    var cueBall = GetCueBall();
                    if (cueBall != null)
                    {
                        _cueController.UpdateAim(cueBall.Position, _inputProvider.MouseWorldPosition);
                    }

                    if (!_cueController.IsOverheated)
                    {
                        if (_inputProvider.IsLeftMouseDown)
                        {
                            _cueController.ChargeShot(deltaTime);
                        }
                        
                        if (_inputProvider.WasLeftMouseReleased)
                        {
                            if (cueBall != null)
                            {
                                _cueController.ExecuteShot(cueBall);
                            }
                            CurrentState = GameState.SimulatingBalls;
                        }
                    }
                    else
                    {
                        if (_cueController.Power <= 0.0f)
                        {
                            CurrentState = GameState.PlayerTurn;
                        }
                    }
                    break;
                }

                case GameState.SimulatingBalls:
                {
                    if (_physicsEngine.AreAllBodiesAtRest())
                    {
                        var bodies = _physicsEngine.GetBodies() ?? Array.Empty<IPhysicsBody>();
                        bool hasWhite = bodies.Any(b => b.Id == 0);
                        bool hasBlack = bodies.Any(b => b.Id == 1);

                        if (!hasBlack)
                        {
                            Winner = ActivePlayer;
                            CurrentState = GameState.GameOver;
                            break;
                        }

                        if (!hasWhite)
                        {
                            var spawnPosition = new Vector2(200f, 300f);
                            var respawnedCue = new HardcodedCueBall
                            {
                                Position = spawnPosition,
                                PreviousPosition = spawnPosition,
                                Velocity = Vector2.Zero
                            };
                            _physicsEngine.AddBody(respawnedCue);
                        }

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

        private class HardcodedCueBall : ICircleBody
        {
            public int Id => 0;
            public Vector2 Position { get; set; }
            public Vector2 PreviousPosition { get; set; }
            public Vector2 Velocity { get; set; }
            public float Mass => 0.15f;
            public float Restitution => 0.8f;
            public float Radius => 10f;
            public bool IsStatic => false;

            public void ApplyImpulse(Vector2 impulse)
            {
                if (Mass > 0f)
                {
                    Velocity += impulse / Mass;
                }
            }
        }
    }
}



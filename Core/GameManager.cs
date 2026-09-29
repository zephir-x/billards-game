using System;
using System.Collections.Generic;
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
        
        public IPlayer Player1 => _player1;
        public IPlayer Player2 => _player2;

        public event Action? OnScratchFoul;

        private readonly IPhysicsEngine _physicsEngine;
        private readonly ICueController _cueController;
        private readonly IInputProvider _inputProvider;

        private readonly IPlayer _player1;
        private readonly IPlayer _player2;

        private List<ICircleBody> _ballsAtStartOfTurn = new List<ICircleBody>();

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
            _player1.AssignedType = null;
            _player2.AssignedType = null;
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
                            
                            // Snapshot balls before simulation
                            var prev_bodies = _physicsEngine.GetBodies();
                            _ballsAtStartOfTurn = (prev_bodies != null ? prev_bodies : Array.Empty<IPhysicsBody>())
                                .OfType<ICircleBody>()
                                .ToList();

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
                        var current_bodies = _physicsEngine.GetBodies();
                        var currentBodies = (current_bodies != null ? current_bodies : Array.Empty<IPhysicsBody>()).OfType<ICircleBody>().ToList();
                        
                        bool hasWhite = currentBodies.Any(b => b.Id == 0);
                        bool hasBlack = currentBodies.Any(b => b.Id == 8); // Black ball ID is 8

                        var pocketedBalls = _ballsAtStartOfTurn
                            .Where(b => !currentBodies.Any(cb => cb.Id == b.Id))
                            .ToList();

                        bool scratch = !hasWhite;
                        bool blackPocketed = !hasBlack;

                        bool continueTurn = false;
                        
                        this.Winner = null; // assure reset

                        // Rule: First pocketed after break assigns color (if not white or black)
                        if (!scratch && 
                            _player1.AssignedType == null && 
                            pocketedBalls.Any(b => b.BallType == BallType.Solid || b.BallType == BallType.Striped))
                        {
                            var first = pocketedBalls.First(b => b.BallType == BallType.Solid || b.BallType == BallType.Striped);
                            ActivePlayer.AssignedType = first.BallType;
                            IPlayer opponent = ActivePlayer == _player1 ? _player2 : _player1;
                            opponent.AssignedType = first.BallType == BallType.Solid ? BallType.Striped : BallType.Solid;
                        }

                        if (blackPocketed)
                        {
                            // Did the current player pocket all their balls?
                            if (ActivePlayer.AssignedType != null)
                            {
                                int ownBallsRemaining = currentBodies.Count(b => b.BallType == ActivePlayer.AssignedType);
                                if (ownBallsRemaining == 0)
                                {
                                    Winner = ActivePlayer; // Success!
                                }
                                else
                                {
                                    Winner = ActivePlayer == _player1 ? _player2 : _player1; // Failure
                                }
                            }
                            else
                            {
                                // Black pocketed before assigning types = instant lose
                                Winner = ActivePlayer == _player1 ? _player2 : _player1;
                            }
                            CurrentState = GameState.GameOver;
                            break;
                        }

                        if (scratch)
                        {
                            OnScratchFoul?.Invoke();
                            EndTurn();
                            break; // Scratch means turn over
                        }

                        // Check if pocketed own ball
                        if (ActivePlayer.AssignedType != null)
                        {
                            bool pocketedOpponentBall = pocketedBalls.Any(b => b.BallType != ActivePlayer.AssignedType && b.BallType != BallType.Cue && b.BallType != BallType.Black);
                            bool pocketedOwnBall = pocketedBalls.Any(b => b.BallType == ActivePlayer.AssignedType);
                            
                            if (pocketedOpponentBall)
                            {
                                continueTurn = false;
                            }
                            else if (pocketedOwnBall)
                            {
                                continueTurn = true;
                            }
                        }

                        if (continueTurn)
                        {
                            // "Continue Turn"
                            CurrentState = GameState.PlayerTurn;
                        }
                        else
                        {
                            EndTurn();
                        }
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BilliardsGame.Interfaces;
using BilliardsGame.Interfaces.Enums;
using BilliardsGame.Interfaces.Models;

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
        public event Action<Vector2>? OnPlaceCueBall;

        private readonly IPhysicsEngine _physicsEngine;
        private readonly ICueController _cueController;
        private readonly IInputProvider _inputProvider;
        private readonly RuleValidator _ruleValidator;

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
            _ruleValidator = new RuleValidator();

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

                case GameState.BallInHand:
                {
                    if (_inputProvider.WasLeftMouseReleased)
                    {
                        var mousePos = _inputProvider.MouseWorldPosition;
                        
                        // Enforce placement inside the table's green cloth boundary
                        // Ball radius is 10f, cushion limits are (200..800, 200..600)
                        bool isInsideBounds = mousePos.X >= 210f && mousePos.X <= 790f && 
                                              mousePos.Y >= 210f && mousePos.Y <= 590f;

                        bool isValidPlacement = isInsideBounds;

                        if (isValidPlacement)
                        {
                            // Ensure the ball does not overlap with pockets (would instantly fall)
                            var pockets = _physicsEngine.GetPockets();
                            foreach (var pocket in pockets)
                            {
                                float distSqToPocket = (pocket.Position - mousePos).LengthSquared();
                                if (distSqToPocket < pocket.Radius * pocket.Radius)
                                {
                                    isValidPlacement = false;
                                    break;
                                }
                            }
                        }

                        if (isValidPlacement)
                        {
                            var bodies = _physicsEngine.GetBodies();
                            foreach (var body in bodies)
                            {
                                if (body is ICircleBody circle)
                                {
                                    float distSq = (circle.Position - mousePos).LengthSquared();
                                    // 10f cue ball radius + object ball radius
                                    float minDist = circle.Radius + 10f; 
                                    if (distSq < minDist * minDist)
                                    {
                                        isValidPlacement = false;
                                        break;
                                    }
                                }
                            }
                        }

                        if (isValidPlacement)
                        {
                            OnPlaceCueBall?.Invoke(mousePos);
                            CurrentState = GameState.PlayerTurn;
                        }
                    }
                    break;
                }

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
                            _physicsEngine.ResetStrokeData();

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
                        var currentBodies = (_physicsEngine.GetBodies() ?? Array.Empty<IPhysicsBody>())
                            .OfType<ICircleBody>().ToList();
                        
                        var pocketedBalls = _ballsAtStartOfTurn
                            .Where(b => !currentBodies.Any(cb => cb.Id == b.Id))
                            .ToList();

                        var strokeData = _physicsEngine.CurrentStrokeData;

                        BallType? firstHitType = null;
                        if (strokeData.FirstBallHitId.HasValue)
                        {
                            var firstHit = _ballsAtStartOfTurn.FirstOrDefault(b => b.Id == strokeData.FirstBallHitId.Value);
                            firstHitType = firstHit?.BallType;
                        }
                        
                        int ownBallsRemaining = 0;
                        if (ActivePlayer.AssignedType != null)
                        {
                            ownBallsRemaining = _ballsAtStartOfTurn.Count(b => b.BallType == ActivePlayer.AssignedType);
                        }
                        else
                        {
                            ownBallsRemaining = 7; 
                        }

                        var ctx = new RuleContext
                        {
                            PlayerAssignedType = ActivePlayer.AssignedType,
                            FirstHitBallId = strokeData.FirstBallHitId,
                            FirstHitBallType = firstHitType,
                            RailsHitAfterContact = strokeData.RailsHitAfterContact,
                            IsCueBallSunk = pocketedBalls.Any(b => b.Id == 0),
                            Is8BallSunk = pocketedBalls.Any(b => b.Id == 8),
                            AreAllOwnBallsSunkBeforeShot = (ownBallsRemaining == 0),
                            PocketedBallTypes = pocketedBalls.Where(b => b.Id != 0 && b.Id != 8).Select(b => b.BallType).ToList(),
                            PocketedBallIds = pocketedBalls.Select(b => b.Id).ToList()
                        };

                        var ruleResult = _ruleValidator.Validate(ctx);

                        if (ruleResult == RuleResult.Continue || ruleResult == RuleResult.TurnLost)
                        {
                            if (ActivePlayer.AssignedType == null)
                            {
                                var validPocketed = pocketedBalls.FirstOrDefault(b => b.BallType == BallType.Solid || b.BallType == BallType.Striped);
                                if (validPocketed != null)
                                {
                                    ActivePlayer.AssignedType = validPocketed.BallType;
                                    IPlayer opponent = ActivePlayer == _player1 ? _player2 : _player1;
                                    opponent.AssignedType = validPocketed.BallType == BallType.Solid ? BallType.Striped : BallType.Solid;
                                }
                            }
                        }

                        switch (ruleResult)
                        {
                            case RuleResult.Foul:
                                OnScratchFoul?.Invoke();
                                var existingCueBall = currentBodies.FirstOrDefault(b => b.Id == 0);
                                if (existingCueBall != null)
                                {
                                    _physicsEngine.RemoveBody(existingCueBall);
                                }
                                ActivePlayer = ActivePlayer == _player1 ? _player2 : _player1;
                                CurrentState = GameState.BallInHand;
                                break;
                            case RuleResult.Continue:
                                CurrentState = GameState.PlayerTurn;
                                break;
                            case RuleResult.TurnLost:
                                EndTurn();
                                break;
                            case RuleResult.GameOverWin:
                                Winner = ActivePlayer;
                                CurrentState = GameState.GameOver;
                                break;
                            case RuleResult.GameOverLose:
                                Winner = ActivePlayer == _player1 ? _player2 : _player1;
                                CurrentState = GameState.GameOver;
                                break;
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
            if (bodies == null) return null;
            return bodies.FirstOrDefault(b => b.Id == 0);
        }
    }
}

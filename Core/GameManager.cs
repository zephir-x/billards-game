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
        
        public string NotificationMessage { get; private set; } = "";
        public float NotificationTimer { get; private set; } = 0f;
        public NotificationType NotificationType { get; private set; } = NotificationType.Info;

        public IPocket? TargetPocket { get; private set; }
        
        public event Action? OnScratchFoul;
        public event Action<Vector2>? OnPlaceCueBall;
        public event Action? OnGameOver;
        public event Action<NotificationType>? OnNotificationEvent;

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

        public void SetNotification(string message, NotificationType type, float time)
        {
            NotificationMessage = message;
            NotificationType = type;
            NotificationTimer = time;
            OnNotificationEvent?.Invoke(type);
        }

        public void StartGame()
        {
            _player1.AssignedType = null;
            _player2.AssignedType = null;
            _player1.Score = 0;
            _player2.Score = 0;
            Winner = null;
            NotificationMessage = "";
            NotificationTimer = 0f;
            TargetPocket = null;
            
            ActivePlayer = _player1;
            SetTurnState();
        }

        public void EndTurn()
        {
            TargetPocket = null;
            ActivePlayer = ActivePlayer == _player1 ? _player2 : _player1;
            SetTurnState();
        }

        private void SetTurnState()
        {
            if (ActivePlayer.Score == 7)
            {
                TargetPocket = null;
                CurrentState = GameState.CallingPocket;
                SetNotification("Select a pocket for the 8-Ball!", NotificationType.Decision, 3f);
            }
            else
            {
                CurrentState = GameState.PlayerTurn;
            }
        }

        public void SelectTargetPocket(IPocket pocket)
        {
            if (CurrentState == GameState.CallingPocket)
            {
                TargetPocket = pocket;
                CurrentState = GameState.PlayerTurn;
            }
        }

        public void UpdateLogic(float deltaTime)
        {
            if (NotificationTimer > 0f)
            {
                NotificationTimer -= deltaTime;
                if (NotificationTimer <= 0f) NotificationMessage = "";
            }

            switch (CurrentState)
            {
                case GameState.Menu:
                    break;

                case GameState.CallingPocket:
                {
                    if (_inputProvider.WasLeftMousePressed)
                    {
                        var mousePos = _inputProvider.MouseWorldPosition;
                        var pockets = _physicsEngine.GetPockets();
                        foreach (var pocket in pockets)
                        {
                            if ((pocket.Position - mousePos).LengthSquared() < pocket.Radius * pocket.Radius)
                            {
                                SelectTargetPocket(pocket);
                                break;
                            }
                        }
                    }
                    break;
                }

                case GameState.BallInHand:
                {
                    if (_inputProvider.WasLeftMouseReleased)
                    {
                        var mousePos = _inputProvider.MouseWorldPosition;
                        
                        var bounds = _physicsEngine.GetPlayfieldBounds();
                        bool isInsideBounds = mousePos.X >= bounds.Left + 10f && mousePos.X <= bounds.Right - 10f &&
                                              mousePos.Y >= bounds.Top + 10f && mousePos.Y <= bounds.Bottom - 10f;

                        bool isValidPlacement = isInsideBounds;

                        if (isValidPlacement)
                        {
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
                            SetTurnState();
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

                    if (_inputProvider.WasLeftMousePressed)
                    {
                        CurrentState = GameState.ChargingShot;
                    }
                    break;
                }

                case GameState.ChargingShot:
                {
                    bool wasOverheated = _cueController.IsOverheated;
                    _cueController.UpdateOverheat(deltaTime);

                    if (wasOverheated && !_cueController.IsOverheated)
                    {
                        CurrentState = GameState.PlayerTurn;
                        break;
                    }

                    var cueBall = GetCueBall();

                    if (!_cueController.IsOverheated)
                    {
                        if (_inputProvider.IsLeftMouseDown)
                        {
                            _cueController.ChargeShot(deltaTime);
                        }
                        else if (!_inputProvider.WasLeftMouseReleased)
                        {
                            CurrentState = GameState.PlayerTurn;
                        }
                        
                        if (_inputProvider.WasLeftMouseReleased)
                        {
                            _physicsEngine.ResetStrokeData();

                            if (cueBall != null)
                            {
                                _cueController.ExecuteShot(cueBall);
                            }
                            
                            var prev_bodies = _physicsEngine.GetBodies();
                            _ballsAtStartOfTurn = (prev_bodies != null ? prev_bodies : Array.Empty<IPhysicsBody>())
                                .OfType<ICircleBody>()
                                .ToList();

                            CurrentState = GameState.SimulatingBalls;
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

                        bool is8Sunk = pocketedBalls.Any(b => b.Id == 8);
                        bool is8SunkInTarget = false;
                        if (is8Sunk && TargetPocket != null)
                        {
                            if (strokeData.SunkBallsToPockets.TryGetValue(8, out var sunkPocket) && sunkPocket == TargetPocket)
                            {
                                is8SunkInTarget = true;
                            }
                        }

                        var ctx = new RuleContext
                        {
                            PlayerAssignedType = ActivePlayer.AssignedType,
                            FirstHitBallId = strokeData.FirstBallHitId,
                            FirstHitBallType = firstHitType,
                            RailsHitAfterContact = strokeData.RailsHitAfterContact,
                            IsCueBallSunk = pocketedBalls.Any(b => b.Id == 0),
                            Is8BallSunk = is8Sunk,
                            Is8BallSunkInTarget = is8SunkInTarget,
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
                                    SetNotification($"Player {ActivePlayer.Id} assigned to {validPocketed.BallType}s!", NotificationType.Decision, 3f);
                                }
                            }
                        }

                        _player1.Score = _player1.AssignedType != null ? 7 - currentBodies.Count(b => b is ICircleBody cb && cb.BallType == _player1.AssignedType) : 0;
                        _player2.Score = _player2.AssignedType != null ? 7 - currentBodies.Count(b => b is ICircleBody cb && cb.BallType == _player2.AssignedType) : 0;

                        switch (ruleResult)
                        {
                            case RuleResult.Foul:
                                if (ctx.IsCueBallSunk) { SetNotification("FOUL: Scratch", NotificationType.Foul, 3f); }
                                else if (ctx.FirstHitBallId == null) { SetNotification("FOUL: Missed", NotificationType.Foul, 3f); }
                                else if (ctx.PocketedBallIds.Count == 0 && ctx.RailsHitAfterContact == 0) { SetNotification("FOUL: No Rail Contact", NotificationType.Foul, 3f); }
                                else { SetNotification("FOUL: Wrong Ball First", NotificationType.Foul, 3f); }
                                
                                OnScratchFoul?.Invoke();
                                var existingCueBall = currentBodies.FirstOrDefault(b => b.Id == 0);
                                if (existingCueBall != null)
                                {
                                    _physicsEngine.RemoveBody(existingCueBall);
                                }
                                TargetPocket = null;
                                ActivePlayer = ActivePlayer == _player1 ? _player2 : _player1;
                                CurrentState = GameState.BallInHand;
                                break;
                            case RuleResult.Continue:
                                SetTurnState();
                                break;
                            case RuleResult.TurnLost:
                                EndTurn();
                                break;
                            case RuleResult.GameOverWin:
                                Winner = ActivePlayer;
                                CurrentState = GameState.GameOver;
                                OnGameOver?.Invoke(); break;
                            case RuleResult.GameOverLose:
                                Winner = ActivePlayer == _player1 ? _player2 : _player1;
                                CurrentState = GameState.GameOver;
                                OnGameOver?.Invoke(); break;
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

using System.Collections.Generic;
using BilliardsGame.Interfaces;

namespace BilliardsGame.App
{
    public class SceneParameters : ISceneParameters
    {
        private readonly IGameManager _gameManager;
        private readonly IPhysicsEngine _physicsEngine;
        private readonly ICueController _cueController;

        public SceneParameters(IGameManager gameManager, IPhysicsEngine physicsEngine, ICueController cueController)
        {
            _gameManager = gameManager;
            _physicsEngine = physicsEngine;
            _cueController = cueController;
        }

        public GameState CurrentState => _gameManager.CurrentState;
        public IPlayer CurrentTurnPlayer => _gameManager.ActivePlayer;
        public IPlayer? Winner => _gameManager.Winner;
        public IReadOnlyCollection<IPhysicsBody> Bodies => _physicsEngine.GetBodies();
        public IReadOnlyCollection<IPocket> Pockets => _physicsEngine.GetPockets();
        public ICueController CueInfo => _cueController;
    }
}


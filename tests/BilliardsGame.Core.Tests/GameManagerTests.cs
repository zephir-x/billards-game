using System.Numerics;
using Moq;
using Xunit;
using BilliardsGame.Core;
using BilliardsGame.Interfaces;
using System.Collections.Generic;

namespace BilliardsGame.Core.Tests
{
    public class GameManagerTests
    {
        [Fact]
        public void UpdateLogic_StateTransitions_PlayerTurn_To_ChargingShot_To_SimulatingBalls_To_PlayerTurn()
        {
            // Arrange
            var physicsMock = new Mock<IPhysicsEngine>();
            var cueMock = new Mock<ICueController>();
            var inputMock = new Mock<IInputProvider>();

            var cueBallMock = new Mock<IPhysicsBody>();
            cueBallMock.SetupGet(b => b.Id).Returns(0);
            
            physicsMock.Setup(p => p.GetBodies()).Returns(new List<IPhysicsBody> { cueBallMock.Object });

            var gameManager = new GameManager(physicsMock.Object, cueMock.Object, inputMock.Object);
            gameManager.StartGame(); // Set state to PlayerTurn

            Assert.Equal(GameState.PlayerTurn, gameManager.CurrentState);
            var player1 = gameManager.ActivePlayer;

            // Step 1: Mouse down -> transition to ChargingShot
            inputMock.SetupGet(i => i.IsLeftMouseDown).Returns(true);
            gameManager.UpdateLogic(0.016f);

            Assert.Equal(GameState.ChargingShot, gameManager.CurrentState);

            // Step 2: Mouse released -> transition to SimulatingBalls
            inputMock.SetupGet(i => i.IsLeftMouseDown).Returns(false);
            inputMock.SetupGet(i => i.WasLeftMouseReleased).Returns(true);
            
            // In ChargingShot state, the manager calls ChargeShot and ExecuteShot
            cueMock.SetupGet(c => c.Power).Returns(0.5f); // simulate partial charge
            gameManager.UpdateLogic(0.016f);

            Assert.Equal(GameState.SimulatingBalls, gameManager.CurrentState);
            cueMock.Verify(c => c.ExecuteShot(cueBallMock.Object), Times.Once);

            // Step 3: AtRest == false -> still SimulatingBalls
            physicsMock.Setup(p => p.AreAllBodiesAtRest(It.IsAny<float>())).Returns(false);
            gameManager.UpdateLogic(0.016f);
            Assert.Equal(GameState.SimulatingBalls, gameManager.CurrentState);

            // Step 4: AtRest == true -> transition back to PlayerTurn, change active player
            physicsMock.Setup(p => p.AreAllBodiesAtRest(It.IsAny<float>())).Returns(true);
            gameManager.UpdateLogic(0.016f);

            Assert.Equal(GameState.PlayerTurn, gameManager.CurrentState);
            Assert.NotEqual(player1, gameManager.ActivePlayer); // Turn changed
        }

        [Fact]
        public void UpdateLogic_ChargingShot_Overheat_ResetsToPlayerTurn_WhenPowerReachesZero()
        {
            // Arrange
            var physicsMock = new Mock<IPhysicsEngine>();
            var cueMock = new Mock<ICueController>();
            var inputMock = new Mock<IInputProvider>();
            
            var cueBallMock = new Mock<IPhysicsBody>();
            cueBallMock.SetupGet(b => b.Id).Returns(0);
            
            physicsMock.Setup(p => p.GetBodies()).Returns(new List<IPhysicsBody> { cueBallMock.Object });

            var gameManager = new GameManager(physicsMock.Object, cueMock.Object, inputMock.Object);
            gameManager.StartGame();

            // Set to ChargingShot
            inputMock.SetupGet(i => i.IsLeftMouseDown).Returns(true);
            gameManager.UpdateLogic(0.016f); 
            Assert.Equal(GameState.ChargingShot, gameManager.CurrentState);

            // Trigger Overheat with Power > 0
            cueMock.SetupGet(c => c.IsOverheated).Returns(true);
            cueMock.SetupGet(c => c.Power).Returns(0.5f); // cooling down
            inputMock.SetupGet(i => i.WasLeftMouseReleased).Returns(false);

            gameManager.UpdateLogic(0.016f);
            
            Assert.Equal(GameState.ChargingShot, gameManager.CurrentState); // Still charging state while cooling down

            // Simulate cooled down fully
            cueMock.SetupGet(c => c.Power).Returns(0.0f);
            gameManager.UpdateLogic(0.016f);
            
            Assert.Equal(GameState.PlayerTurn, gameManager.CurrentState);
        }
    }
}

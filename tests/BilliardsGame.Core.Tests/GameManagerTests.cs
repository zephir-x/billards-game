using System.Numerics;
using Moq;
using Xunit;
using BilliardsGame.Core;
using BilliardsGame.Interfaces;
using System.Collections.Generic;
using System.Linq;

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
            
            var blackBallMock = new Mock<IPhysicsBody>();
            blackBallMock.SetupGet(b => b.Id).Returns(1);

            physicsMock.Setup(p => p.GetBodies()).Returns(new List<IPhysicsBody> { cueBallMock.Object, blackBallMock.Object });

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
            var blackBallMock = new Mock<IPhysicsBody>();
            blackBallMock.SetupGet(b => b.Id).Returns(1);
            
            physicsMock.Setup(p => p.GetBodies()).Returns(new List<IPhysicsBody> { cueBallMock.Object, blackBallMock.Object });

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

        [Fact]
        public void UpdateLogic_SimulatingBalls_NoCueBall_RespawnAndReturnToPlayerTurn()
        {
            // Arrange
            var physicsMock = new Mock<IPhysicsEngine>();
            var cueMock = new Mock<ICueController>();
            var inputMock = new Mock<IInputProvider>();
            
            // Setup with ONLY black ball (simulate cue ball fell in pocket)
            var blackBallMock = new Mock<IPhysicsBody>();
            blackBallMock.SetupGet(b => b.Id).Returns(1);
            
            physicsMock.Setup(p => p.GetBodies()).Returns(new List<IPhysicsBody> { blackBallMock.Object });
            physicsMock.Setup(p => p.AreAllBodiesAtRest(It.IsAny<float>())).Returns(true);

            var gameManager = new GameManager(physicsMock.Object, cueMock.Object, inputMock.Object);
            // Transition directly to SimulatingBalls is not exposed, so we simulate a shot
            gameManager.StartGame();
            inputMock.SetupGet(i => i.WasLeftMouseReleased).Returns(true);
            // First transition to ChargingShot
            var cueBallMock = new Mock<IPhysicsBody>();
            cueBallMock.SetupGet(b => b.Id).Returns(0);
            physicsMock.SetupSequence(p => p.GetBodies())
                .Returns(new List<IPhysicsBody> { cueBallMock.Object, blackBallMock.Object }) // For initial GetCueBall
                .Returns(new List<IPhysicsBody> { cueBallMock.Object, blackBallMock.Object }) // For ChargingShot
                .Returns(new List<IPhysicsBody> { blackBallMock.Object });                    // For SimulatingBalls (missing cue ball)

            inputMock.SetupGet(i => i.IsLeftMouseDown).Returns(true);
            gameManager.UpdateLogic(0.016f); // To ChargingShot
            inputMock.SetupGet(i => i.IsLeftMouseDown).Returns(false);
            gameManager.UpdateLogic(0.016f); // To SimulatingBalls

            // Act
            gameManager.UpdateLogic(0.016f); // In SimulatingBalls

            // Assert
            physicsMock.Verify(p => p.AddBody(It.Is<IPhysicsBody>(b => b.Id == 0 && b.Position == new Vector2(200f, 300f))), Times.Once);
            Assert.Equal(GameState.PlayerTurn, gameManager.CurrentState);
        }

        [Fact]
        public void UpdateLogic_SimulatingBalls_NoBlackBall_GameOver()
        {
            // Arrange
            var physicsMock = new Mock<IPhysicsEngine>();
            var cueMock = new Mock<ICueController>();
            var inputMock = new Mock<IInputProvider>();
            
            // Setup with ONLY white ball (simulate black ball fell in pocket)
            var cueBallMock = new Mock<IPhysicsBody>();
            cueBallMock.SetupGet(b => b.Id).Returns(0);
            
            physicsMock.Setup(p => p.GetBodies()).Returns(new List<IPhysicsBody> { cueBallMock.Object });
            physicsMock.Setup(p => p.AreAllBodiesAtRest(It.IsAny<float>())).Returns(true);

            var gameManager = new GameManager(physicsMock.Object, cueMock.Object, inputMock.Object);
            gameManager.StartGame();
            inputMock.SetupGet(i => i.WasLeftMouseReleased).Returns(true);
            inputMock.SetupGet(i => i.IsLeftMouseDown).Returns(true);
            gameManager.UpdateLogic(0.016f); // To ChargingShot
            inputMock.SetupGet(i => i.IsLeftMouseDown).Returns(false);
            gameManager.UpdateLogic(0.016f); // To SimulatingBalls

            // Act
            gameManager.UpdateLogic(0.016f); // In SimulatingBalls

            // Assert
            Assert.Equal(GameState.GameOver, gameManager.CurrentState);
        }
    }
}


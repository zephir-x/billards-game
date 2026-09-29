using System.Numerics;
using Moq;
using Xunit;
using BilliardsGame.Core;
using BilliardsGame.Interfaces;
using BilliardsGame.Interfaces.Enums;
using BilliardsGame.Interfaces.Models;
using System.Collections.Generic;
using System.Linq;

namespace BilliardsGame.Core.Tests
{
    public class GameManagerTests
    {
        [Fact]
        public void UpdateLogic_SimulatingBalls_NoCueBall_BallInHandFoul()
        {
            // Arrange
            var physicsMock = new Mock<IPhysicsEngine>();
            var cueMock = new Mock<ICueController>();
            var inputMock = new Mock<IInputProvider>();
            
            // Setup with ONLY black ball (simulate cue ball fell in pocket)
            var blackBallMock = new Mock<ICircleBody>();
            blackBallMock.SetupGet(b => b.Id).Returns(8);
            blackBallMock.SetupGet(b => b.BallType).Returns(BallType.Black);
            
            physicsMock.Setup(p => p.GetBodies()).Returns(new List<IPhysicsBody> { blackBallMock.Object });
            physicsMock.Setup(p => p.AreAllBodiesAtRest(It.IsAny<float>())).Returns(true);
            
            var strokeData = new StrokeData { FirstBallHitId = 8, RailsHitAfterContact = 1, SunkBallsIds = new List<int> { 0 } };
            physicsMock.Setup(p => p.CurrentStrokeData).Returns(strokeData);

            var gameManager = new GameManager(physicsMock.Object, cueMock.Object, inputMock.Object);
            
            bool eventFired = false;
            gameManager.OnScratchFoul += delegate { eventFired = true; };

            // Transition directly to SimulatingBalls is not exposed, so we simulate a shot
            gameManager.StartGame();
            inputMock.SetupGet(i => i.WasLeftMouseReleased).Returns(true);
            
            var cueBallMock = new Mock<ICircleBody>();
            cueBallMock.SetupGet(b => b.Id).Returns(0);
            cueBallMock.SetupGet(b => b.BallType).Returns(BallType.Cue);
            physicsMock.SetupSequence(p => p.GetBodies())
                .Returns(new List<IPhysicsBody> { cueBallMock.Object, blackBallMock.Object }) // For initial GetCueBall
                .Returns(new List<IPhysicsBody> { cueBallMock.Object, blackBallMock.Object }) // For ChargingShot
                .Returns(new List<IPhysicsBody> { cueBallMock.Object, blackBallMock.Object })
                .Returns(new List<IPhysicsBody> { blackBallMock.Object })
                .Returns(new List<IPhysicsBody> { blackBallMock.Object })
                .Returns(new List<IPhysicsBody> { blackBallMock.Object })
                .Returns(new List<IPhysicsBody> { blackBallMock.Object });

            inputMock.SetupGet(i => i.IsLeftMouseDown).Returns(true);
            gameManager.UpdateLogic(0.016f); // To ChargingShot
            inputMock.SetupGet(i => i.IsLeftMouseDown).Returns(false);
            gameManager.UpdateLogic(0.016f); // To SimulatingBalls

            // Act
            gameManager.UpdateLogic(0.016f); // In SimulatingBalls

            // Assert
            Assert.True(eventFired);
            Assert.Equal(GameState.BallInHand, gameManager.CurrentState);
        }

        [Fact]
        public void UpdateLogic_SimulatingBalls_NoBlackBall_GameOver()
        {
            // Arrange
            var physicsMock = new Mock<IPhysicsEngine>();
            var cueMock = new Mock<ICueController>();
            var inputMock = new Mock<IInputProvider>();
            
            // Setup with ONLY white ball (simulate black ball fell in pocket)
            var cueBallMock = new Mock<ICircleBody>();
            cueBallMock.SetupGet(b => b.Id).Returns(0);
            cueBallMock.SetupGet(b => b.BallType).Returns(BallType.Cue);
            
            var blackBallMock = new Mock<ICircleBody>();
            blackBallMock.SetupGet(b => b.Id).Returns(8);
            blackBallMock.SetupGet(b => b.BallType).Returns(BallType.Black);

            var strokeData = new StrokeData { FirstBallHitId = 8, RailsHitAfterContact = 1, SunkBallsIds = new List<int> { 8 } };
            physicsMock.Setup(p => p.CurrentStrokeData).Returns(strokeData);

            physicsMock.SetupSequence(p => p.GetBodies())
                .Returns(new List<IPhysicsBody> { cueBallMock.Object, blackBallMock.Object }) // initial GetCueBall
                .Returns(new List<IPhysicsBody> { cueBallMock.Object, blackBallMock.Object }) // ChargingShot
                .Returns(new List<IPhysicsBody> { cueBallMock.Object, blackBallMock.Object }) // Snapshot
                .Returns(new List<IPhysicsBody> { cueBallMock.Object })
                .Returns(new List<IPhysicsBody> { cueBallMock.Object })
                .Returns(new List<IPhysicsBody> { cueBallMock.Object });

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

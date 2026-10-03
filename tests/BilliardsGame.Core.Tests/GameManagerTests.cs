using System.Collections.Generic;
using Moq;
using Xunit;
using BilliardsGame.Core;
using BilliardsGame.Interfaces;
using BilliardsGame.Interfaces.Enums;
using BilliardsGame.Interfaces.Models;

namespace BilliardsGame.Core.Tests
{
    /// <summary>
    /// Testing matrix verifying the primary state manager mapping complex transitions mapping strokes across rule verifications validating state mutations exclusively.
    /// </summary>
    public class GameManagerTests
    {
        [Fact]
        public void UpdateLogic_SimulatingBalls_NoCueBall_BallInHandFoul()
        {
            // Initializes mock structures encapsulating disconnected Physics engine dependencies locking evaluations securely down testing pure domain logic only.
            var physicsMock = new Mock<IPhysicsEngine>();
            var cueMock = new Mock<ICueController>();
            var inputMock = new Mock<IInputProvider>();
            
            var blackBallMock = new Mock<ICircleBody>();
            blackBallMock.SetupGet(b => b.Id).Returns(8);
            blackBallMock.SetupGet(b => b.BallType).Returns(BallType.Black);
            
            // Spoofs the array explicitly removing the Cue Ball mocking a scratch mapping perfectly onto engine conditions simulating rules properly.
            physicsMock.Setup(p => p.GetBodies()).Returns(new List<IPhysicsBody> { blackBallMock.Object });
            physicsMock.Setup(p => p.AreAllBodiesAtRest(It.IsAny<float>())).Returns(true);
            
            var strokeData = new StrokeData { FirstBallHitId = 8, RailsHitAfterContact = 1, SunkBallsIds = new List<int> { 0 } };
            physicsMock.Setup(p => p.CurrentStrokeData).Returns(strokeData);

            var gameManager = new GameManager(physicsMock.Object, cueMock.Object, inputMock.Object);
            
            bool eventFired = false;
            gameManager.OnScratchFoul += delegate { eventFired = true; };

            // Triggers state transitions stepping forward from initialization into targeted resolving states ensuring rule paths execute cleanly.
            gameManager.StartGame();
            inputMock.SetupGet(i => i.WasLeftMouseReleased).Returns(true);
            
            var cueBallMock = new Mock<ICircleBody>();
            cueBallMock.SetupGet(b => b.Id).Returns(0);
            cueBallMock.SetupGet(b => b.BallType).Returns(BallType.Cue);
            physicsMock.SetupSequence(p => p.GetBodies())
                .Returns(new List<IPhysicsBody> { cueBallMock.Object, blackBallMock.Object }) 
                .Returns(new List<IPhysicsBody> { cueBallMock.Object, blackBallMock.Object }) 
                .Returns(new List<IPhysicsBody> { cueBallMock.Object, blackBallMock.Object })
                .Returns(new List<IPhysicsBody> { blackBallMock.Object })
                .Returns(new List<IPhysicsBody> { blackBallMock.Object })
                .Returns(new List<IPhysicsBody> { blackBallMock.Object })
                .Returns(new List<IPhysicsBody> { blackBallMock.Object });

            inputMock.SetupGet(i => i.WasLeftMousePressed).Returns(true);
            gameManager.UpdateLogic(0.016f); // To ChargingShot
            inputMock.SetupGet(i => i.IsLeftMouseDown).Returns(false);
            gameManager.UpdateLogic(0.016f); // To SimulatingBalls

            // Actuate the target validation phase.
            gameManager.UpdateLogic(0.016f); 

            // Assert exact state change mappings validating transition handlers reacting successfully against logic anomalies explicitly missing cue subsets.
            Assert.True(eventFired);
            Assert.Equal(GameState.BallInHand, gameManager.CurrentState);
        }

        [Fact]
        public void UpdateLogic_SimulatingBalls_NoBlackBall_GameOver()
        {
            // Arranges identical proxy stubs mocking engine outputs matching standard simulation closures checking Game Over conditionals accurately matching active ball bounds.
            var physicsMock = new Mock<IPhysicsEngine>();
            var cueMock = new Mock<ICueController>();
            var inputMock = new Mock<IInputProvider>();
            
            var cueBallMock = new Mock<ICircleBody>();
            cueBallMock.SetupGet(b => b.Id).Returns(0);
            cueBallMock.SetupGet(b => b.BallType).Returns(BallType.Cue);
            
            var blackBallMock = new Mock<ICircleBody>();
            blackBallMock.SetupGet(b => b.Id).Returns(8);
            blackBallMock.SetupGet(b => b.BallType).Returns(BallType.Black);

            var strokeData = new StrokeData { FirstBallHitId = 8, RailsHitAfterContact = 1, SunkBallsIds = new List<int> { 8 } };
            physicsMock.Setup(p => p.CurrentStrokeData).Returns(strokeData);

            // Sequences exact logic pipelines projecting expected output limits cleanly mapping into ending frames validating array deletions structurally triggering end menus.
            physicsMock.SetupSequence(p => p.GetBodies())
                .Returns(new List<IPhysicsBody> { cueBallMock.Object, blackBallMock.Object }) 
                .Returns(new List<IPhysicsBody> { cueBallMock.Object, blackBallMock.Object }) 
                .Returns(new List<IPhysicsBody> { cueBallMock.Object, blackBallMock.Object }) 
                .Returns(new List<IPhysicsBody> { cueBallMock.Object })
                .Returns(new List<IPhysicsBody> { cueBallMock.Object })
                .Returns(new List<IPhysicsBody> { cueBallMock.Object });

            physicsMock.Setup(p => p.AreAllBodiesAtRest(It.IsAny<float>())).Returns(true);

            var gameManager = new GameManager(physicsMock.Object, cueMock.Object, inputMock.Object);
            gameManager.StartGame();
            inputMock.SetupGet(i => i.WasLeftMouseReleased).Returns(true);
            inputMock.SetupGet(i => i.WasLeftMousePressed).Returns(true);
            gameManager.UpdateLogic(0.016f);
            inputMock.SetupGet(i => i.IsLeftMouseDown).Returns(false);
            gameManager.UpdateLogic(0.016f); 

            // Actuates closing logic boundaries resolving empty arrays successfully transitioning onto final overlays accurately validating missing elements cleanly avoiding crashes.
            gameManager.UpdateLogic(0.016f); 
            Assert.Equal(GameState.GameOver, gameManager.CurrentState);
        }
    }
}

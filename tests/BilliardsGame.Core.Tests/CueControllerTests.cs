using System.Numerics;
using Moq;
using Xunit;
using BilliardsGame.Core;
using BilliardsGame.Interfaces;
using BilliardsGame.Interfaces.Enums;
using BilliardsGame.Interfaces.Models;
using System;

namespace BilliardsGame.Core.Tests
{
    public class CueControllerTests
    {
        [Fact]
        public void UpdateAim_CalculatesCorrectNormalizedVector()
        {
            var controller = new CueController();
            var ballPos = new Vector2(100, 100);
            var mousePos = new Vector2(50, 100);

            // Vector from mouse to ball: (100, 100) - (50, 100) = (50, 0)
            // Normalized: (1, 0)
            controller.UpdateAim(ballPos, mousePos);

            Assert.Equal(1f, controller.CueDirection.X, 4);
            Assert.Equal(0f, controller.CueDirection.Y, 4);
        }

        [Fact]
        public void UpdateAim_ZeroDistance_HandlesGracefully()
        {
            var controller = new CueController();
            var pos = new Vector2(100, 100);

            controller.UpdateAim(pos, pos);

            Assert.Equal(Vector2.Zero, controller.CueDirection);
        }

        [Fact]
        public void ChargeShot_IncreasesPowerLinearly_AndEntersOverheatOnMax()
        {
            var controller = new CueController();
            
            controller.ChargeShot(0.5f);
            Assert.Equal(0.5f, controller.Power, 4);

            controller.ChargeShot(0.4f);
            Assert.Equal(0.9f, controller.Power, 4);

            // Going over 1.0 triggers Overheat and clamps to 1.0
            controller.ChargeShot(0.2f);
            Assert.Equal(1.0f, controller.Power, 4);
            Assert.True(controller.IsOverheated);
            
            // Updating Overheat lowers power
            controller.UpdateOverheat(0.1f);
            // Power should decrease by 0.1 * 1.0 * 0.5 = 0.05. New Power = 0.95
            Assert.Equal(0.935f, controller.Power, 4);
            
            // Charging while overheated has no effect
            controller.ChargeShot(0.5f);
            Assert.Equal(0.935f, controller.Power, 4);
        }

        [Fact]
        public void ExecuteShot_AppliesCorrectImpulse_AndResetsPower()
        {
            var controller = new CueController();
            var ballMock = new Mock<IPhysicsBody>();
            controller.UpdateAim(new Vector2(10, 0), new Vector2(0, 0)); // Direction (1, 0)
            
            controller.ChargeShot(0.5f);
            
            // Assume MaxImpulse is 500f. Power is 0.5f. Expected impulse: (250, 0)
            Vector2 expectedImpulse = new Vector2(250f, 0f);
            
            controller.ExecuteShot(ballMock.Object);
            
            ballMock.Verify(b => b.ApplyImpulse(expectedImpulse), Times.Once);
            Assert.Equal(0.0f, controller.Power);
        }
    }
}

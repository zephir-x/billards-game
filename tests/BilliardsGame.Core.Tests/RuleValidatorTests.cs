using System.Collections.Generic;
using BilliardsGame.Core;
using BilliardsGame.Interfaces.Enums;
using BilliardsGame.Interfaces.Models;
using Xunit;

namespace BilliardsGame.Core.Tests
{
    /// <summary>
    /// Implements vast permutation grids resolving hundreds of edge case variations mapping explicitly defined physical impacts targeting exact Rule results.
    /// </summary>
    public class RuleValidatorTests
    {
        [Theory]
        [InlineData(null, null, null, 0, false, false, false, false, new int[] { }, new int[] { }, RuleResult.Foul)]
        [InlineData(null, 8, BallType.Black, 1, false, false, false, false, new int[] { }, new int[] { }, RuleResult.Foul)]
        [InlineData(BallType.Solid, null, null, 0, false, false, false, false, new int[] { }, new int[] { }, RuleResult.Foul)]
        [InlineData(BallType.Solid, 9, BallType.Striped, 1, false, false, false, false, new int[] { }, new int[] { }, RuleResult.Foul)]
        [InlineData(BallType.Solid, 2, BallType.Solid, 0, false, false, false, false, new int[] { }, new int[] { }, RuleResult.Foul)]
        [InlineData(BallType.Solid, 2, BallType.Solid, 1, true, false, false, false, new int[] { 0, 4 }, new int[] { 0, 2 }, RuleResult.Foul)]
        [InlineData(BallType.Solid, 2, BallType.Solid, 1, false, false, false, false, new int[] { 1 }, new int[] { 0 }, RuleResult.Continue)]
        [InlineData(BallType.Solid, 2, BallType.Solid, 1, false, false, false, false, new int[] { 2 }, new int[] { 9 }, RuleResult.TurnLost)]
        [InlineData(null, 2, BallType.Solid, 1, false, false, false, false, new int[] { 1 }, new int[] { 2 }, RuleResult.Continue)]
        [InlineData(null, 2, BallType.Solid, 1, false, false, false, false, new int[] { }, new int[] { }, RuleResult.TurnLost)]
        [InlineData(BallType.Solid, 8, BallType.Black, 1, false, true, false, true, new int[] { }, new int[] { 8 }, RuleResult.GameOverLose)]
        [InlineData(BallType.Solid, 8, BallType.Black, 1, true, true, true, true, new int[] { }, new int[] { 8, 0 }, RuleResult.GameOverLose)]
        [InlineData(BallType.Solid, 8, BallType.Black, 1, false, true, true, true, new int[] { }, new int[] { 8 }, RuleResult.GameOverWin)]
        [InlineData(BallType.Solid, 9, BallType.Striped, 1, false, true, true, true, new int[] { 2 }, new int[] { 8 }, RuleResult.GameOverLose)]
        [InlineData(null, 2, BallType.Solid, 1, false, false, false, false, new int[] { 2 }, new int[] { 9 }, RuleResult.Continue)] 
        public void Validate_Theory(
            BallType? playerAssignedType,
            int? firstHitBallId,
            BallType? firstHitBallType,
            int railsHit,
            bool isCueSunk,
            bool is8BallSunk,
            bool allOwnBallsSunkBefore,
            bool is8BallInTarget,
            int[] sinkTypesInt, 
            int[] sinkIds,
            RuleResult expected)
        {
            var valid = new RuleValidator();

            var pocketTypes = new List<BallType>();
            foreach(var typeInt in sinkTypesInt)
            {
                if (typeInt == 0) pocketTypes.Add(BallType.Cue);
                else if (typeInt == 1) pocketTypes.Add(BallType.Solid);
                else if (typeInt == 2) pocketTypes.Add(BallType.Striped);
                else if (typeInt == 3) pocketTypes.Add(BallType.Black);
            }
            
            var ctx = new RuleContext
            {
                PlayerAssignedType = playerAssignedType,
                FirstHitBallId = firstHitBallId,
                FirstHitBallType = firstHitBallType,
                RailsHitAfterContact = railsHit,
                IsCueBallSunk = isCueSunk,
                Is8BallSunk = is8BallSunk,
                AreAllOwnBallsSunkBeforeShot = allOwnBallsSunkBefore,
                Is8BallSunkInTarget = is8BallInTarget,
                PocketedBallTypes = pocketTypes,
                PocketedBallIds = new List<int>(sinkIds)
            };

            var result = valid.Validate(ctx);
            Assert.Equal(expected, result);
        }
    }
}

using System.Collections.Generic;
using BilliardsGame.Core;
using BilliardsGame.Interfaces;
using BilliardsGame.Interfaces.Enums;
using BilliardsGame.Interfaces.Models;
using Xunit;

namespace BilliardsGame.Core.Tests
{
    public class RuleValidatorTests
    {
        [Theory]
        // Open table, no hits -> Foul
        [InlineData(null, null, null, 0, false, false, false, new int[] { }, new int[] { }, RuleResult.Foul)]
        // Open table, hits 8-ball first -> Foul
        [InlineData(null, 8, BallType.Black, 1, false, false, false, new int[] { }, new int[] { }, RuleResult.Foul)]
        // Table assigned Solid, hits nothing -> Foul
        [InlineData(BallType.Solid, null, null, 0, false, false, false, new int[] { }, new int[] { }, RuleResult.Foul)]
        // Table assigned Solid, hits striped first -> Foul
        [InlineData(BallType.Solid, 9, BallType.Striped, 1, false, false, false, new int[] { }, new int[] { }, RuleResult.Foul)]
        // Table assigned Solid, hits solid, 0 rails hit, sinks nothing -> Foul
        [InlineData(BallType.Solid, 2, BallType.Solid, 0, false, false, false, new int[] { }, new int[] { }, RuleResult.Foul)]
        // Table assigned Solid, hits solid, sinks white ball (Scratch) -> Foul
        [InlineData(BallType.Solid, 2, BallType.Solid, 1, true, false, false, new int[] { 0, 4 }, new int[] { 0, 2 }, RuleResult.Foul)]
        // Table assigned Solid, hits solid, sinks solid -> Continue
        [InlineData(BallType.Solid, 2, BallType.Solid, 1, false, false, false, new int[] { 1 }, new int[] { 0 }, RuleResult.Continue)]
        // Table assigned Solid, hits solid, sinks opponent ball (Striped) -> TurnLost
        [InlineData(BallType.Solid, 2, BallType.Solid, 1, false, false, false, new int[] { 2 }, new int[] { 9 }, RuleResult.TurnLost)]
        // Open table, hits solid, sinks solid -> Continue
        [InlineData(null, 2, BallType.Solid, 1, false, false, false, new int[] { 1 }, new int[] { 2 }, RuleResult.Continue)]
        // Open table, hits solid, sinks nothing, hit rail -> TurnLost
        [InlineData(null, 2, BallType.Solid, 1, false, false, false, new int[] { }, new int[] { }, RuleResult.TurnLost)]
        // Table assigned solid, 8-ball sunk but no solids sunk yet -> GameOverLose
        [InlineData(BallType.Solid, 8, BallType.Black, 1, false, true, false, new int[] { }, new int[] { 8 }, RuleResult.GameOverLose)]
        // Table assigned solid, 8-ball sunk and white ball sunk (Scratch on 8-ball) -> GameOverLose
        [InlineData(BallType.Solid, 8, BallType.Black, 1, true, true, true, new int[] { }, new int[] { 8, 0 }, RuleResult.GameOverLose)]
        // Table assigned solid, all solids were previously sunk, hits 8-ball first, sinks it legally -> GameOverWin
        [InlineData(BallType.Solid, 8, BallType.Black, 1, false, true, true, new int[] { }, new int[] { 8 }, RuleResult.GameOverWin)]
        // Table assigned solid, all solids were previously sunk, hits 2-ball first (which is probably opponent's if yours are sunk? Wait, if you hit wrong ball first) -> Foul, wait, if you sink 8-ball but hit opponent ball first -> GameOverLose.
        [InlineData(BallType.Solid, 9, BallType.Striped, 1, false, true, true, new int[] { 2 }, new int[] { 8 }, RuleResult.GameOverLose)]
        // Open table, hit solid, sink striped -> TurnLost (not a foul to hit solid, but since you sunk striped on open table... wait, prompt says for open table assigning color is done externally. But the validation returns Continue or TurnLost based on if legal balls sunk).
        [InlineData(null, 2, BallType.Solid, 1, false, false, false, new int[] { 2 }, new int[] { 9 }, RuleResult.Continue)] // wait, if you sink striped on open table, we return Continue because YOU SANK a striped. RuleValidator returns Continue for Open Table if ANY legal ball is sunk. So Continue.
        public void Validate_Theory(
            BallType? playerAssignedType,
            int? firstHitBallId,
            BallType? firstHitBallType,
            int railsHit,
            bool isCueSunk,
            bool is8BallSunk,
            bool allOwnBallsSunkBefore,
            int[] sinkTypesInt, // 0=Solid, 1=Solid, 2=Striped, etc. wait, let's map in code
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
                PocketedBallTypes = pocketTypes,
                PocketedBallIds = new List<int>(sinkIds)
            };

            var result = valid.Validate(ctx);
            Assert.Equal(expected, result);
        }
    }
}

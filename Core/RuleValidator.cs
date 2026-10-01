using System.Collections.Generic;
using System.Linq;
using BilliardsGame.Interfaces;
using BilliardsGame.Interfaces.Enums;
using BilliardsGame.Interfaces.Models;

namespace BilliardsGame.Core
{
    public class RuleContext
    {
        public BallType? PlayerAssignedType { get; set; }
        public int? FirstHitBallId { get; set; }
        public BallType? FirstHitBallType { get; set; }
        public int RailsHitAfterContact { get; set; }
        public bool IsCueBallSunk { get; set; }
        public bool Is8BallSunk { get; set; }
        public bool Is8BallSunkInTarget { get; set; }
        public bool AreAllOwnBallsSunkBeforeShot { get; set; }
        public List<BallType> PocketedBallTypes { get; set; } = new List<BallType>();
        public List<int> PocketedBallIds { get; set; } = new List<int>();
    }

    public class RuleValidator
    {
        public RuleResult Validate(RuleContext ctx)
        {
            // Empty shot (no hits at all)
            if (ctx.FirstHitBallId == null)
            {
                return RuleResult.Foul;
            }

            // 8-Ball rules
            if (ctx.Is8BallSunk)
            {
                if (ctx.IsCueBallSunk || !ctx.AreAllOwnBallsSunkBeforeShot)
                {
                    return RuleResult.GameOverLose;
                }
                
                // If you are on the 8-ball, the first hit MUST be the 8-ball.
                if (ctx.FirstHitBallType != BallType.Black)
                {
                    return RuleResult.GameOverLose;
                }

                if (!ctx.Is8BallSunkInTarget)
                {
                    return RuleResult.GameOverLose;
                }

                return RuleResult.GameOverWin;
            }

            // Cue ball scratch
            if (ctx.IsCueBallSunk)
            {
                return RuleResult.Foul;
            }

            // Wrong first hit
            if (ctx.PlayerAssignedType == null)
            {
                // Open table - hitting 8-ball first is a foul
                if (ctx.FirstHitBallType == BallType.Black)
                {
                    return RuleResult.Foul;
                }
            }
            else
            {
                // Table is assigned - hitting wrong type first is a foul
                // (except if you are on the 8-ball, then you must hit 8-ball first)
                if (ctx.AreAllOwnBallsSunkBeforeShot)
                {
                    if (ctx.FirstHitBallType != BallType.Black)
                    {
                        return RuleResult.Foul;
                    }
                }
                else
                {
                    if (ctx.FirstHitBallType != ctx.PlayerAssignedType)
                    {
                        return RuleResult.Foul;
                    }
                }
            }

            // No rail hit after contact and nothing pocketed
            if (ctx.PocketedBallIds.Count == 0 && ctx.RailsHitAfterContact == 0)
            {
                return RuleResult.Foul;
            }

            // Continuation logic
            if (ctx.PlayerAssignedType == null)
            {
                // If open table, and some legal balls are potted (not black)
                var legalPotted = ctx.PocketedBallTypes.Where(t => t == BallType.Solid || t == BallType.Striped).ToList();
                if (legalPotted.Count > 0)
                {
                    return RuleResult.Continue;
                }
            }
            else
            {
                // If own ball potted
                if (ctx.PocketedBallTypes.Contains(ctx.PlayerAssignedType.Value))
                {
                    return RuleResult.Continue;
                }
            }

            return RuleResult.TurnLost;
        }
    }
}

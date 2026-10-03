using System.Collections.Generic;
using System.Linq;
using BilliardsGame.Interfaces;
using BilliardsGame.Interfaces.Enums;
using BilliardsGame.Interfaces.Models;

namespace BilliardsGame.Core
{
    /// <summary>
    /// Serves as a mutable configuration packet passing dynamic physical aftermath variables into the rule evaluation pipeline.
    /// </summary>
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

    /// <summary>
    /// Pure functional domain service decoupling the rigid 8-Ball match rules from the physics engine and game lifecycle loop.
    /// </summary>
    public class RuleValidator
    {
        /// <summary>
        /// Reads simulated physical results checking against sequential condition branches determining valid plays, fouls, and wins.
        /// </summary>
        /// <param name=""ctx"">Context defining exactly what occurred on the play field during the strike execution.</param>
        /// <returns>Resolution status reflecting the consequences of the shot for the active player.</returns>
        public RuleResult Validate(RuleContext ctx)
        {
            // Empty shot (no hits at all against any ball) resulting in a penalty.
            if (ctx.FirstHitBallId == null)
            {
                return RuleResult.Foul;
            }

            // Extreme condition: The 8-Ball was sunk. We must evaluate if it grants a win or a fatal loss.
            if (ctx.Is8BallSunk)
            {
                if (ctx.IsCueBallSunk || !ctx.AreAllOwnBallsSunkBeforeShot)
                {
                    return RuleResult.GameOverLose;
                }
                
                // If you are legitimately on the 8-ball phase, the first collision MUST be against the 8-ball itself.
                if (ctx.FirstHitBallType != BallType.Black)
                {
                    return RuleResult.GameOverLose;
                }

                // Disallow random pots; 8-Ball must violently sink strictly into a explicitly defined pocket.
                if (!ctx.Is8BallSunkInTarget)
                {
                    return RuleResult.GameOverLose;
                }

                return RuleResult.GameOverWin;
            }

            // A cue ball scratch unconditionally resolves into a foul bypassing all continuous assertions.
            if (ctx.IsCueBallSunk)
            {
                return RuleResult.Foul;
            }

            // Validate the first targeted classification against current player assignments.
            if (ctx.PlayerAssignedType == null)
            {
                // Open table boundary: Hitting the 8-ball directly first is an immediate foul.
                if (ctx.FirstHitBallType == BallType.Black)
                {
                    return RuleResult.Foul;
                }
            }
            else
            {
                // Table is assigned to player types. The active player must hit their respective ball type first.
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

            // ""No Rail"" Foul: The shot struck an object ball legally but no target touched a cushion and nothing was deposited.
            if (ctx.PocketedBallIds.Count == 0 && ctx.RailsHitAfterContact == 0)
            {
                return RuleResult.Foul;
            }

            // Favorable Conditions: Grant continuity to the active player.
            if (ctx.PlayerAssignedType == null)
            {
                // On an open table, potting solid or striped assigns that type and grants a repeat turn.
                var legalPotted = ctx.PocketedBallTypes.Where(t => t == BallType.Solid || t == BallType.Striped).ToList();
                if (legalPotted.Count > 0)
                {
                    return RuleResult.Continue;
                }
            }
            else
            {
                // Re-evaluate positive momentum: Sinking your designated matching subset rewards continuous control.
                if (ctx.PocketedBallTypes.Contains(ctx.PlayerAssignedType.Value))
                {
                    return RuleResult.Continue;
                }
            }

            // No special foul occurred, no favorable legal points scored: Standard yield.
            return RuleResult.TurnLost;
        }
    }
}

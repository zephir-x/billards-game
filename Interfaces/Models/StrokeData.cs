using System.Collections.Generic;

namespace BilliardsGame.Interfaces.Models
{
    /// <summary>
    /// Encapsulates chronological physical events dispatched directly from the Simulation step.
    /// Acts as an objective evidence packet for validating subjective sequence rules.
    /// </summary>
    public class StrokeData
    {
        /// <summary>
        /// Captures the strictly identified sphere ID intersected first by the cue ball ray path.
        /// </summary>
        public int? FirstBallHitId { get; set; }

        /// <summary>
        /// Counter reflecting how many physical cushion walls were impacted *after* the initial collision.
        /// Used strictly in verifying ""No Rail Contact"" foul regulations.
        /// </summary>
        public int RailsHitAfterContact { get; set; }

        /// <summary>
        /// Temporal collection tracking all body identifiers trapped in pocket vectors.
        /// </summary>
        public List<int> SunkBallsIds { get; set; } = new List<int>();

        /// <summary>
        /// Dictionary translating destroyed physical body identifiers to their exact sink (IPocket instance) destination.
        /// Essential for validating Called-Pocket 8-Ball win routes.
        /// </summary>
        public Dictionary<int, IPocket> SunkBallsToPockets { get; set; } = new Dictionary<int, IPocket>();

        /// <summary>
        /// Purges internal temporal states cleanly to reset the container for subsequent shot analyses.
        /// </summary>
        public void Reset()
        {
            FirstBallHitId = null;
            RailsHitAfterContact = 0;
            SunkBallsIds.Clear();
            SunkBallsToPockets.Clear();
        }
    }
}

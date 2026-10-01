using System.Collections.Generic;

namespace BilliardsGame.Interfaces.Models
{
    public class StrokeData
    {
        public int? FirstBallHitId { get; set; }
        public int RailsHitAfterContact { get; set; }
        public List<int> SunkBallsIds { get; set; } = new List<int>();
        public Dictionary<int, IPocket> SunkBallsToPockets { get; set; } = new Dictionary<int, IPocket>();

        public void Reset()
        {
            FirstBallHitId = null;
            RailsHitAfterContact = 0;
            SunkBallsIds = new List<int>();
            SunkBallsToPockets.Clear();
        }
    }
}

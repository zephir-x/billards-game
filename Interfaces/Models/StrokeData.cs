using System.Collections.Generic;

namespace BilliardsGame.Interfaces.Models
{
    public struct StrokeData
    {
        public int? FirstBallHitId { get; set; }
        public int RailsHitAfterContact { get; set; }
        public List<int> SunkBallsIds { get; set; }

        public void Reset()
        {
            FirstBallHitId = null;
            RailsHitAfterContact = 0;
            SunkBallsIds = new List<int>();
        }
    }
}
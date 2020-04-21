using System.Collections.Generic;

namespace IslesOfWar.GameStateProcessing
{
    public class Actions
    {
        public string blockhash { get; set; }
        public string rngseed { get; set; }
        public dynamic admin { get; set; }
        public List<Move> moves { get; set; }
    }
}

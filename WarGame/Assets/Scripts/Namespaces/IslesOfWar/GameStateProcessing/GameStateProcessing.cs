using System.Collections.Generic;

//Probably move this over to MudHero
namespace IslesOfWar.GameStateProcessing
{
    public class Move
    {
        public List<Transaction> inputs { get; set; }
        public dynamic move { get; set; }
        public string name { get; set; }
        public string txid { get; set; }
    }
}
using System.Collections.Generic;

namespace IslesOfWar.Communication
{
    public class PlayerActions
    {
        public string nat;          //Create Or Change Nation
        public IslandBuildOrder bld;//Build
        public List<int> buy;       //Unit Purchase
        public string srch;         //Island Search
        public ResourceOrder pot;   //Resource Pot Submission
        public List<string> dep;    //Depleted Island Submissions
        public MarketOrderAction opn;//Starts a new Market Order
        public string cls;          //Closes/Cancels a Market Order - Order ID
        public string[] acpt;       //Accepts a Market Order and it's terms - Order ID, SellerName
        public BattleCommand attk;  //Attack Plan
        public BattleCommand dfnd;  //Defend Orders
        public SquadWithdrawl rmv;  //Remove Squads from Island.
        public int igBuy;           //In game purchases. Pack count
        public TransferWarbux trns; //Transfer warbux from one player to another.
    }
}

namespace IslesOfWar.Communication
{
    public class AdminCommands
    {
        public int[] ver;          //Set Version, sets all - 3
        public string status;      //Set mode, currently only used for isInMaintenanceMode;
        public decimal packCost;   //Set ResourcePackCost
        public float[] mrktPrcnt;  //Set MarketFeePercent, sets all - 4
        public float[] mrktFee;    //Set MinMarketFee, sets all - 4
        public float[] packAmnt;   //Set ResourcePackAmount, sets all - 4
        public float[] iwCost;     //Set IslandSearchCost, sets all - 4 (only using first right now though) 
        public float atkPerc;      //Set AttackCostPercent
        public float uPerc;        //Set UndiscoveredPercent
        public float repTime;      //Set IslandReplenishTime
        public float sqdHlthLmt;   //Set SquadHealthLimit
        public float[] uCost;      //Set UnitCost first is unitType, next 4 are prices - 5
        public float[] bnkCost;    //Set BunkerCost, same as uCost
        public float[] blkCost;    //Set BlockerCost, same as uCost
        public float[] colCost;    //Set CollectorCost, same as uCost
        public float[] uDmg;       //Set UnitDamages, first is type/index, next is damage - 2
        public float[] uHp;        //Set UnitHealths, same as uDmg
        public float[] uProbs;     //Set UnitOrderProbabilities, same as uDmg
        public float[] ucMods;     //Set UnitCombatModifiers, first is unitType, next 12 is modifiers - 13
        public int[] mmRes;        //Set MinMaxResoruces, first is resourceType, next 2 are min and max respectively - 3
        public float[] eRates;     //Set ExtractRates, sets all - 3
        public float[] fRates;     //Set FreeResourceRates, sets all - 4
        public int ePrd;           //Set ExtractPeriod
        public int fPrd;           //Set FreeResourcePeriod
        public int dayBlk;         //Set AssumedDailyBlocks
        public float[] tProbs;     //Set TileProbabilities, sets all - 3
        public float[] rProbs;     //Set ResourceProbabilities, set all - 3
        public float[] poolPerc;   //Set PurchaseToPoolPercents, first is YType, next 4 are percents per resource - 5
        public int pTimer;         //Set PoolRewardBlocks
        public int wTimer;         //Set WarbucksRewardBlocks
        public string msg;         //Set a message that can be displayed to everyone. Use special characters at start for message types.
        public AirDrop airDrop;    //Add Money to a player's resources.
        public string txid;        //Transaction ID of the command. Xaya Given Info.
    }
}

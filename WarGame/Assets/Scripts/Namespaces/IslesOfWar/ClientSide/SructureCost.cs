namespace IslesOfWar.ClientSide
{
    public struct StructureCost
    {
        public string islandID;
        public int tileIndex, purchaseType;
        public double[] resources;

        public StructureCost(string _islandID, int _tileIndex, int _purchaseType, Constants constants)
        {
            islandID = _islandID;
            tileIndex = _tileIndex;
            purchaseType = _purchaseType;
            resources = new double[] {constants.collectorCosts[purchaseType-1,0], constants.collectorCosts[purchaseType-1, 1],
                constants.collectorCosts[purchaseType-1, 2], constants.collectorCosts[purchaseType-1,3] };
        }
    }
}

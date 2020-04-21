namespace IslesOfWar.Communication
{
    //Class variables shrunk for blockchain.
    public class IslandBuildOrder
    {
        public string id;  //island identification
        public string col; //collectors
        public string def; //defenses

        public IslandBuildOrder() { }

        public IslandBuildOrder(string islandID, string collectors, string defenses)
        {
            id = islandID;
            col = collectors;
            def = defenses;
        }
    }
}
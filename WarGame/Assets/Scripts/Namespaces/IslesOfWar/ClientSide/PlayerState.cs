using System.Collections.Generic;

namespace IslesOfWar.ClientSide
{
    public class PlayerState
    {
        public string nationCode;
        public List<double> units;
        public List<double> resources;
        public List<string> islands;
        public string attackableIsland;

        public PlayerState() { }

        public PlayerState(string nation)
        {
            nationCode = nation;
            units = new List<double>();
            units.AddRange(new double[9]);
            resources = new List<double>();
            resources.AddRange(new double[4]);
            islands = new List<string>();
            attackableIsland = "";
        }

        public PlayerState(string nation, double[] unitCounts, double[] resourceCounts, string[] islandIDs, string _attackableIsland)
        {
            nationCode = nation;

            units = new List<double>();
            resources = new List<double>();
            islands = new List<string>();
                
            units.AddRange(unitCounts);
            resources.AddRange(resourceCounts);
            islands.AddRange(islandIDs);
            attackableIsland = _attackableIsland;
        }
    }
}
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IslesOfWar.ClientSide
{
    public class State
    {
        public Dictionary<string, PlayerState> players;
        public Dictionary<string, Island> islands;
        public Dictionary<string, List<List<double>>> resourceContributions;
        public Dictionary<string, List<string>> depletedContributions;
        public Dictionary<string, List<MarketOrder>> resourceMarket;
        public List<double> resourcePools;
        public double warbucksPool;
        public Constants currentConstants;
        public string debugBlockData = "";

        public State() { }

        public State(Dictionary<string, PlayerState> allPlayers, Dictionary<string, Island> allIslands)
        {
            players = new Dictionary<string, PlayerState>();
            islands = new Dictionary<string, Island>();
            resourceContributions = new Dictionary<string, List<List<double>>>();
            depletedContributions = new Dictionary<string, List<string>>();
            resourceMarket = new Dictionary<string, List<MarketOrder>>();

            players = JsonConvert.DeserializeObject<Dictionary<string, PlayerState>>(JsonConvert.SerializeObject(allPlayers));
            islands = JsonConvert.DeserializeObject<Dictionary<string, Island>>(JsonConvert.SerializeObject(allIslands));
            resourcePools = new List<double> { 0, 0, 0 };
            warbucksPool = 0;
        }

        public State(Dictionary<string, PlayerState> allPlayers, Dictionary<string, Island> allIslands, Dictionary<string, List<List<double>>> resContributions, Dictionary<string, List<string>> depContributions)
        {
            players = new Dictionary<string, PlayerState>();
            islands = new Dictionary<string, Island>();
            resourceContributions = new Dictionary<string, List<List<double>>>();
            depletedContributions = new Dictionary<string, List<string>>();
            resourceMarket = new Dictionary<string, List<MarketOrder>>();

            players = JsonConvert.DeserializeObject<Dictionary<string, PlayerState>>(JsonConvert.SerializeObject(allPlayers));
            islands = JsonConvert.DeserializeObject<Dictionary<string, Island>>(JsonConvert.SerializeObject(allIslands));
            resourceContributions = resContributions;
            depletedContributions = depContributions;
            resourcePools = new List<double> { 0, 0, 0 };
            warbucksPool = 0;
        }

        public State(Dictionary<string, PlayerState> allPlayers, Dictionary<string, Island> allIslands, Dictionary<string, List<List<double>>> resContributions, Dictionary<string, List<string>> depContributions, List<double> resPools, double warPool)
        {
            players = new Dictionary<string, PlayerState>();
            islands = new Dictionary<string, Island>();
            resourceContributions = new Dictionary<string, List<List<double>>>();
            depletedContributions = new Dictionary<string, List<string>>();
            resourceMarket = new Dictionary<string, List<MarketOrder>>();

            players = JsonConvert.DeserializeObject<Dictionary<string, PlayerState>>(JsonConvert.SerializeObject(allPlayers));
            islands = JsonConvert.DeserializeObject<Dictionary<string, Island>>(JsonConvert.SerializeObject(allIslands));
            resourceContributions = resContributions;
            depletedContributions = depContributions;
            resourcePools = resPools;
            warbucksPool = warPool;
        }

        public void Init()
        {
            currentConstants = new Constants();
            players = new Dictionary<string, PlayerState>();
            islands = new Dictionary<string, Island>();
            resourceContributions = new Dictionary<string, List<List<double>>>();
            depletedContributions = new Dictionary<string, List<string>>();
            resourceMarket = new Dictionary<string, List<MarketOrder>>();
            resourcePools = new List<double> { 0, 0, 0 };
            warbucksPool = 0;
        }
    }
}

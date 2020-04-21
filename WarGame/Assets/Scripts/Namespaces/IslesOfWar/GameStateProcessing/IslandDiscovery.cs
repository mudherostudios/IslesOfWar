using UnityEngine;

namespace IslesOfWar.GameStateProcessing
{
    public static class IslandDiscovery
    {
        public static string GetIsland(string[] islands, string txid, ref MudHeroRandom random, float undiscoveredPercent)
        {
            float choice = random.Value();

            if (choice < undiscoveredPercent || islands.Length == 0)
                return txid;
            else
            {
                choice = random.Value() * islands.Length;
                return islands[Mathf.FloorToInt(choice)];
            }
        }
    }
}

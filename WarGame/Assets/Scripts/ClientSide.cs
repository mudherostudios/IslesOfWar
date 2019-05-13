using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClientSide
{
    public struct FakeStateJson
    {
        public PlayerState player;
        public WorldState world;
        public PurchaseTable table;
        public bool success;

        public FakeStateJson(PlayerState p, WorldState w, PurchaseTable t, bool s)
        {
            player = p;
            world = w;
            table = t;
            success = s;
        }
    }

    public struct FakeIslandJson
    {
        public Island[] islands;
        public bool success;

        public FakeIslandJson(Island[] i, bool s)
        {
            islands = i;
            success = s;
        }
    }

    public struct Island
    {
        public string name, features, collectors;

        public Island(string _name, string _features, string _collectors)
        {
            name = _name;
            features = _features;
            collectors = _collectors;
        }

        public int totalTiles
        {
            get { return features.Length; }
        }
    }

    public struct Cost
    {
        public ulong warbucks, oil, metal, concrete;
        public long amount;
        public ulong bigAmount;
        public string type;

        public Cost(ulong _warbucks, ulong _oil, ulong _metal, ulong _concrete, long _amount, string _type)
        {
            warbucks = _warbucks;
            oil = _oil;
            metal = _metal;
            concrete = _concrete;
            amount = _amount;

            if (amount > 0)
                bigAmount = (ulong)_amount;
            else
                bigAmount = 0;

            type = _type;
        }

        public Cost(ulong _warbucks, ulong _oil, ulong _metal, ulong _concrete, ulong _bigAmount, string _type)
        {
            warbucks = _warbucks;
            oil = _oil;
            metal = _metal;
            concrete = _concrete;
            amount = 0;
            bigAmount = _bigAmount;
            type = _type;
        }

        public void ResetAmount()
        {
            amount = 0;
            bigAmount = 0;
        }

        public void SetAmount(long _amount)
        {
            amount = _amount;
            if (_amount >= 0)
                bigAmount = (ulong)_amount;
            else
                bigAmount = 0;
        }
    }

    public class PurchaseTable
    {
        public Cost riflemanCost;
        public Cost machineGunnerCost;
        public Cost bazookamanCost;
        public Cost lightTankCost;
        public Cost mediumTankCost;
        public Cost heavyTankCost;
        public Cost lightFighterCost;
        public Cost mediumFighterCost;
        public Cost bomberCost;
        public Cost troopBunkerCost;
        public Cost tankBunkerCost;
        public Cost aircraftBunkerCost;
        public Cost troopBlockerCost;
        public Cost tankBlockerCost;
        public Cost aircraftBlockerCost;

        public PurchaseTable(Cost[] costs)
        {
            riflemanCost = costs[0];
            machineGunnerCost = costs[1];
            bazookamanCost = costs[2];
            lightTankCost = costs[3];
            mediumTankCost = costs[4];
            heavyTankCost = costs[5];
            lightFighterCost = costs[6];
            mediumFighterCost = costs[7];
            bomberCost = costs[8];
            troopBunkerCost = costs[9];
            tankBunkerCost = costs[10];
            aircraftBunkerCost = costs[11];
            troopBlockerCost = costs[12];
            tankBlockerCost = costs[13];
            aircraftBlockerCost = costs[14];
        }

        public Cost[] Costs
        {
            get
            {
                return new Cost[] {riflemanCost, machineGunnerCost, bazookamanCost, lightTankCost, mediumTankCost, heavyTankCost, lightFighterCost, mediumFighterCost, bomberCost,
                    troopBunkerCost, tankBunkerCost, aircraftBunkerCost, troopBlockerCost, tankBunkerCost, aircraftBunkerCost};
            }
        }
    }

    public class WorldState
    {
        public ulong warbucksPool, oilPool, metalPool, concretePool;
        public ulong warbucksContributed, oilContributed, metalContributed, concreteContributed;
        public ulong warbucksTotalContributions, oilTotalContributions, metalTotalContributions, concreteTotalContributions;
        public float poolTimer, timeRecieved;

        public WorldState(ulong[] pools, ulong[] contributed, ulong[] contributions, float timer)
        {
            warbucksPool = pools[0];
            oilPool = pools[1];
            metalPool = pools[2];
            concretePool = pools[3];

            warbucksContributed = contributed[0];
            oilContributed = contributed[1];
            metalContributed = contributed[2];
            concreteContributed = contributed[3];

            warbucksTotalContributions = contributions[0];
            oilTotalContributions = contributions[1];
            metalTotalContributions = contributions[2];
            concreteTotalContributions = contributions[3];

            poolTimer = timer;
            timeRecieved = Time.time;
        }

        public float GetContributionPercent(string pool, ulong amount)
        {
            float percent = 0;
            double totalContributions = 0;
            ulong contributingAmount = amount;

            if (pool == "warbucks")
            {
                totalContributions = warbucksTotalContributions;
                contributingAmount += warbucksContributed;
            }
            else if (pool == "oil")
            {
                totalContributions = oilTotalContributions;
                contributingAmount += oilContributed;
            }
            else if (pool == "metal")
            {
                totalContributions = metalTotalContributions;
                contributingAmount += metalContributed;
            }
            else if (pool == "concrete")
            {
                totalContributions = concreteTotalContributions;
                contributingAmount += concreteContributed;
            }

            percent = (float)(totalContributions / contributingAmount);

            return percent;
        }
    }

    public class PlayerState 
    {
        public ulong riflemen, machineGunners, bazookamen;
        public ulong lightTanks, mediumTanks, heavyTanks;
        public ulong lightFighters, mediumFighters, bombers;
        public ulong warbucks, oil, metal, concrete;
        public Island[] islands;

        public PlayerState(ulong[] unitCounts, ulong[] resourceCounts, Island[] _islands)
        {
            riflemen = unitCounts[0];
            machineGunners = unitCounts[1];
            bazookamen = unitCounts[2];
            lightTanks = unitCounts[3];
            mediumTanks = unitCounts[4];
            heavyTanks = unitCounts[5];
            lightFighters = unitCounts[6];
            mediumFighters = unitCounts[7];
            bombers = unitCounts[8];

            warbucks = resourceCounts[0];
            oil = resourceCounts[1];
            metal = resourceCounts[2];
            concrete = resourceCounts[3];

            islands = _islands;
        }
    }
}

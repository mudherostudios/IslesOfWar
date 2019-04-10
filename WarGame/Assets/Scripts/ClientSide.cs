using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClientSide
{
    public struct Cost
    {
        public ulong warbucks, oil, metal, concrete;
        public ulong amount;
        public string type;

        public Cost(ulong _warbucks, ulong _oil, ulong _metal, ulong _concrete, ulong _amount, string _type)
        {
            warbucks = _warbucks;
            oil = _oil;
            metal = _metal;
            concrete = _concrete;
            amount = _amount;
            type = _type;
        }
    }

    public class PlayerState 
    {
        public ulong riflemen, machineGunners, bazookas;
        public ulong lightTanks, mediumTanks, heavyTanks;
        public ulong lightFighters, mediumFighters, bombers;
        public ulong warbucks, oil, metal, concrete;
        public uint[] islands;

        public PlayerState(ulong[] unitCounts, ulong[] resourceCounts, uint[] islandCounts)
        {
            riflemen = unitCounts[0];
            machineGunners = unitCounts[1];
            bazookas = unitCounts[2];
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

            islands = islandCounts;
        }
    }
}

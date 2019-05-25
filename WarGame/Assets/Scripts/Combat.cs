using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
    public struct FakeSquadJson
    {
        public Squad squad;
        public bool success;

        public FakeSquadJson(Squad _squad, bool s)
        {
            squad = _squad;
            success = true;
        }
    }

    public struct BattlePlan
    {
        public string[] squadPlans;
        public bool isAttacker;

        public BattlePlan(string[] plans, bool attacker)
        {
            squadPlans = plans;
            isAttacker = attacker;
        }
    }

    public struct Squad
    {
        public long riflemen, machineGunners, bazookamen;
        public long lightTanks, mediumTanks, heavyTanks;
        public long lightFighters, mediumFighters, bombers;
        public long troopBunkers, tankBunkers, antiAircrafts;

        public Squad(long[] squadCounts)
        {
            riflemen = squadCounts[0];
            machineGunners = squadCounts[1];
            bazookamen = squadCounts[2];

            lightTanks = squadCounts[3];
            mediumTanks = squadCounts[4];
            heavyTanks = squadCounts[5];

            lightFighters = squadCounts[6];
            mediumFighters = squadCounts[7];
            bombers = squadCounts[8];

            troopBunkers = squadCounts[9];
            tankBunkers = squadCounts[10];
            antiAircrafts = squadCounts[11];
        }

        public long[] fullSquad
        {
            get
            {
                long[] squad = new long[]
                {
                    riflemen, machineGunners, bazookamen,
                    lightTanks, mediumTanks, heavyTanks,
                    lightFighters, mediumFighters, bombers,
                    troopBunkers, tankBunkers, antiAircrafts
                };

                return squad;
            }
        }

        public void SetUnits(long[] squad)
        {
            riflemen = squad[0];
            machineGunners = squad[1];
            bazookamen = squad[2];

            lightTanks = squad[3];
            mediumTanks = squad[4];
            heavyTanks = squad[5];

            lightFighters = squad[6];
            mediumFighters = squad[7];
            bombers = squad[8];

            troopBunkers = squad[9];
            tankBunkers = squad[10];
            antiAircrafts = squad[11];
        }
    }

    class CombatTables
    {
        //3x4 grid troop, tanks, aircraft, bunkers
        public float[,] damageTable, healthTable, modifierTable, orderProbabilityTable;

        public CombatTables()
        {
            damageTable = new float[,]
            {
                {2.0f, 3.0f, 4.0f},
                {2.5f, 5.0f, 10.0f},
                {8.0f, 12.0f, 14.0f},
                {12.0f, 30.0f, 16.0f}
            };

            healthTable = new float[,]
            {
                {100.0f, 100.0f, 100.0f},
                {125.0f, 250.0f, 500.0f},
                {200.0f, 300.0f, 200.0f},
                {300.0f, 750.0f, 400.0f}
            };

            modifierTable = new float[,]
            {
                //troop, machine, zook, lTank, mTank, hTank, lPlane, mPlane, bomber, troopBunk, tankBunk, aBunk
                {1.0f, 1.0f, 1.0f,     0.1f, 0.1f, 0.1f,    0.1f, 0.1f, 0.1f,     0.1f, 0.1f, 0.1f},
                {1.5f, 1.5f, 1.5f,     0.25f, 0.1f, 0.1f,   0.25f, 0.25f, 0.25f,  0.1f, 0.1f, 0.1f},
                {0.1f, 0.1f, 0.1f,     1.5f, 1.25f, 1.0f,   1.5f, 1.5f, 1.5f,     1.0f, 1.0f, 1.0f},
                {1.0f, 1.0f, 1.0f,     1.0f, 0.5f, 0.25f,   0.3f, 0.15f, 0.1f,    1.5f, 0.5f, 1.5f},
                {0.75f, 0.75f, 0.75f,  0.6f, 0.3f, 0.1f,    1.5f, 1.0f, 0.5f,     1.5f, 0.5f, 1.5f},
                {0.5f, 0.5f, 0.5f,     2.0f, 1.5f, 1.0f,    0.75f, 0.5f, 0.2f,    1.5f, 0.5f, 1.5f},
                {2.0f, 2.0f, 2.0f,     2.0f, 1.5f, 1.0f,    1.0f, 1.5f, 0.75f,    0.5f, 0.5f, 0.5f},
                {1.5f, 1.5f, 1.5f,     2.0f, 1.5f, 1.0f,    0.5f, 1.0f, 1.5f,     1.0f, 1.0f, 1.0f},
                {1.0f, 1.0f, 1.0f,     1.5f, 1.5f, 1.5f,    0.1f, 0.1f, 0.1f,     2.0f, 2.0f, 2.0f},
                {2.0f, 2.0f, 2.0f,     1.5f, 0.25f, 0.25f,  2.25f, 1.5f, 1.5f,    0.0f, 0.0f, 0.0f},
                {1.5f, 1.5f, 1.5f,     4.0f, 3.0f, 2.0f,    0.3f, 0.2f, 0.1f,     0.0f, 0.0f, 0.0f},
                {1.0f, 1.0f, 1.0f,     1.5f, 0.25f, 0.25f,  4.0f, 3.5f, 2.0f,     0.0f, 0.0f, 0.0f,}
            };

            orderProbabilityTable = new float[,]
            {
                {0.5f, 0.4f, 0.3f},
                {10.1f, 0.085f, 0.065f},
                {0.05f, 0.05f, 0.05f},
                {0.01f, 0.01f, 0.01f}
            };
        }
    }

    class Engagement
    {
        public Squad blufor, opfor;
        public double bluforTotalHealth, opforTotalHealth;
        public double blueforTotalDamage, opforTotalDamage;

        public Engagement(Squad _blufor, Squad _opfor)
        {
            blufor = _blufor;
            opfor = _opfor;
        }
    }
}

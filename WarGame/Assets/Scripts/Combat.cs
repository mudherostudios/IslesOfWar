using System;
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

    public struct EngagementHistory
    {
        public long[,] bluforHistory;
        public long[,] opforHistory;

        public string winner;
        public Squad remainingSquad;

        public EngagementHistory(long[,] _bluforHistory, long[,] _opforHistory, string _winner, Squad winningSquad)
        {
            bluforHistory = _bluforHistory;
            opforHistory = _opforHistory;
            winner = _winner;
            remainingSquad = winningSquad;
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
        public double totalHealth, totalDamage;
        public float[] unitProbabilities;
        public int damagedUnit;

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

            totalHealth = 0;
            totalDamage = 0;

            unitProbabilities = new float[squadCounts.Length];

            damagedUnit = -1;
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

    public class CombatTables
    {
        public float[] damageTable, healthTable, orderProbabilityTable;
        public float[,] modifierTable;

        public CombatTables()
        {
            damageTable = new float[]
            {
                2.0f, 3.0f, 4.0f,
                2.5f, 5.0f, 10.0f,
                8.0f, 12.0f, 14.0f,
                12.0f, 30.0f, 16.0f
            };

            healthTable = new float[]
            {
                100.0f, 100.0f, 100.0f,
                125.0f, 250.0f, 500.0f,
                200.0f, 300.0f, 200.0f,
                300.0f, 750.0f, 400.0f
            };

            orderProbabilityTable = new float[]
            {
                0.5f, 0.4f, 0.3f,
                10.1f, 0.085f, 0.065f,
                0.05f, 0.05f, 0.05f,
                0.01f, 0.01f, 0.01f
            };

            modifierTable = new float[,]
            {
                //12x12 grid - troop, machine, zook, lTank, mTank, hTank, lPlane, mPlane, bomber, troopBunk, tankBunk, aBunk
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
        }
    }

    public class Engagement
    {
        public Squad blufor, opfor;
        public CombatTables tables = new CombatTables();

        public Engagement(Squad _blufor, Squad _opfor)
        {
            blufor = _blufor;
            opfor = _opfor;

            blufor.totalHealth = CalculateTotalHealth(blufor);
            opfor.totalHealth = CalculateTotalHealth(opfor);

            blufor.totalDamage = CalculateTotalDamage(blufor);
            opfor.totalDamage = CalculateTotalDamage(opfor);

            blufor.unitProbabilities = CalculateUnitProbabilities(blufor);
            opfor.unitProbabilities = CalculateUnitProbabilities(opfor);
        }

        public EngagementHistory ResolveEngagement()
        {
            List<long[]> bluforHistory = new List<long[]>();
            List<long[]> opforHistory = new List<long[]>();
            bool engagementIsOver = false;
            string winner = "";
            Squad winningSquad = new Squad();

            while (!engagementIsOver)
            {
                long[] bluforRemaining = CalculateCasulaties(opfor, blufor);
                long[] opforRemaining = CalculateCasulaties(blufor, opfor);

                blufor.SetUnits(bluforRemaining);
                opfor.SetUnits(opforRemaining);

                blufor.totalHealth = CalculateTotalHealth(blufor);
                opfor.totalHealth = CalculateTotalHealth(opfor);

                blufor.totalDamage = CalculateTotalDamage(blufor);
                opfor.totalDamage = CalculateTotalDamage(opfor);

                blufor.unitProbabilities = CalculateUnitProbabilities(blufor);
                opfor.unitProbabilities = CalculateUnitProbabilities(opfor);

                bluforHistory.Add(bluforRemaining);
                opforHistory.Add(opforRemaining);

                if (blufor.totalHealth == 0 && opfor.totalHealth != 0)
                {
                    winner = "opfor";
                    winningSquad = opfor;
                    engagementIsOver = true;
                }
                else if (blufor.totalHealth != 0 && opfor.totalHealth == 0)
                {
                    winner = "blufor";
                    winningSquad = blufor;
                    engagementIsOver = true;
                }
                else if (blufor.totalHealth == 0 && opfor.totalHealth == 0)
                {
                    winner = "lolEveryone be ded";
                    engagementIsOver = true;
                }
            }

            return new EngagementHistory(Get2DHistory(bluforHistory), Get2DHistory(opforHistory), winner, winningSquad);
        }

        long[] CalculateCasulaties(Squad attacker, Squad defender)
        {
            long[]units = defender.fullSquad;
            double cumulativeDamage = 0;

            if (defender.totalHealth > attacker.totalDamage)
            {
                System.Random random = new System.Random();

                while (cumulativeDamage < attacker.totalDamage)
                {
                    int deadUnit = defender.damagedUnit;

                    while (deadUnit > -1)
                    {
                        deadUnit = GetUnitByProbability(random, defender.unitProbabilities);
                        if (units[deadUnit] == 0)
                            deadUnit = -1;
                    }

                    cumulativeDamage += tables.healthTable[deadUnit];

                    if (attacker.totalDamage - cumulativeDamage > 0)
                        units[deadUnit]--;
                    else
                        defender.damagedUnit = deadUnit;
                }
            }
            else
            {
                defender.damagedUnit = -1;
                return new long[units.Length];
            }

            return units;
        }

        double CalculateTotalHealth(Squad squad)
        {
            long[] units = squad.fullSquad;
            double totalHealth = 0;

            for (int u = 0; u < units.Length; u++)
            {
                totalHealth += units[u] * tables.healthTable[u];
            }

            return totalHealth;
        }

        double CalculateTotalDamage(Squad squad)
        {
            long[] units = squad.fullSquad;
            double totalAttack = 0;

            for (int u = 0; u < units.Length; u++)
            {
                for (int m = 0; m < units.Length; m++)
                {
                    totalAttack += (units[u] * tables.damageTable[u]) * (units[u] * tables.modifierTable[u, m]);
                }
            }

            return totalAttack;
        }

        float[] CalculateUnitProbabilities(Squad squad)
        {
            long[] units = squad.fullSquad;
            float[] relativeProbabilities = new float[units.Length];
            float[] trueProbabilities = new float[units.Length];
            float totalRelatives = 0;

            for (int r = 0; r < relativeProbabilities.Length; r++)
            {
                relativeProbabilities[r] = (float)(units[r])*tables.orderProbabilityTable[r];
                totalRelatives += relativeProbabilities[r];
            }

            for (int t = 0; t < trueProbabilities.Length; t++)
            {
                trueProbabilities[t] = relativeProbabilities[t] / totalRelatives;
            }

            return trueProbabilities;
        }

        long GetTotalUnitCount(long[] units)
        {
            long total = 0;

            for (int u = 0; u < units.Length; u++)
            {
                total += units[u];
            }

            return total;
        }

        int GetUnitByProbability(System.Random random, float[] probabilities)
        {
            double threshold = random.NextDouble();
            double currentRange = 0.0;

            for (int p = 0; p < probabilities.Length; p++)
            {
                if(probabilities[p] <= threshold)
                {
                    return p;
                }

                currentRange += probabilities[p];
            }

            return -1;
        }

        long[,] Get2DHistory(List<long[]> listHistory)
        {
            long[,] history = new long[listHistory.Count, listHistory[0].Length];

            for (int h = 0; h < history.GetLength(0); h++)
            {
                for (int u = 0; u < history.GetLength(1); u++)
                {
                    history[h, u] = listHistory[h][u];
                }
            }

            return history;
        }
    }
}

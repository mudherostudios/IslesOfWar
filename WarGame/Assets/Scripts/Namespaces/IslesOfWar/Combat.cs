using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace IslesOfWar
{
    namespace Combat
    {
        public struct SquadMessage
        {
            public Squad squad;
            public bool success;

            public SquadMessage(Squad _squad, bool s)
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
            public int[][] squadPositions;
            public long[][] squadCounts;
            public bool[] reactToNewlyAdjacent;

            public bool isAttacker;

            public BattlePlan(int[][] positions, long[][] squads, bool attacker, bool[] react)
            {
                squadPositions = positions;
                squadCounts = squads;
                isAttacker = attacker;
                reactToNewlyAdjacent = react;

            }
        }

        public class AdjacencyMatrix
        {
            //https://en.wikipedia.org/wiki/Adjacency_matrix
            private int[,] adjacencyMatrix = new int[,]
            {
            {2, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0},
            {1, 2, 1, 0, 1, 1, 0, 0, 0, 0, 0, 0},
            {0, 1, 2, 1, 0, 1, 1, 0, 0, 0, 0, 0},
            {0, 0, 1, 2, 0, 0, 1, 1, 0, 0, 0, 0},
            {1, 1, 0, 0, 2, 1, 0, 0, 1, 1, 0, 0},
            {0, 1, 1, 0, 1, 2, 1, 0, 0, 1, 1, 0},
            {0, 0, 1, 1, 0, 1, 2, 1, 0, 0, 1, 1},
            {0, 0, 0, 1, 0, 0, 1, 2, 0, 0, 0, 1},
            {0, 0, 0, 0, 1, 0, 0, 0, 2, 1, 0, 0},
            {0, 0, 0, 0, 1, 1, 0, 0, 1, 2, 1, 0},
            {0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 2, 1},
            {0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 2},
            };

            public bool IsAdjacent(int currentPosition, int destination)
            {
                bool adjacent = false;

                if (currentPosition >= 0 && currentPosition < adjacencyMatrix.Length && destination >= 0 && destination < adjacencyMatrix.Length)
                {
                    if (adjacencyMatrix[currentPosition, destination] > 0)
                        adjacent = true;
                }

                return adjacent;
            }

            public int mapSize
            {
                get
                {
                    return adjacencyMatrix.Length;
                }
            }

            public int[] GetAllAdjacentIndices(int position, bool excludeSelf)
            {
                List<int> allAdjacentIndices = new List<int>();

                for (int p = 0; p < adjacencyMatrix.GetLength(0); p++)
                {
                    if (adjacencyMatrix[position, p] == 1)
                        allAdjacentIndices.Add(p);
                    else if (adjacencyMatrix[position, p] == 2 && !excludeSelf)
                        allAdjacentIndices.Add(p);
                }

                return allAdjacentIndices.ToArray();
            }
        }

        public class AttackPlanner
        {
            private long[][] squadCounts = new long[3][];
            int[][] squadMoves = new int[3][];
            private int[] lastMoveIndex;
            private AdjacencyMatrix adjacencyMatrix = new AdjacencyMatrix();

            public AttackPlanner(Squad[] squads)
            {
                lastMoveIndex = new int[squads.Length];

                for (int s = 0; s < squads.Length; s++)
                {
                    AddSquad(s, squads[s].fullSquad);
                }
            }

            public void AddSquad(int squad, long[] squadCount)
            {
                squadCounts[squad] = squadCount;
                squadMoves[squad] = new int[6] { -1, -1, -1, -1, -1, -1 };
            }

            public bool AddMove(int squad, int position)
            {
                bool canMove = false;
                int lastIndex = lastMoveIndex[squad];

                if (lastIndex == 0)
                    canMove = true;
                else if (adjacencyMatrix.IsAdjacent(squadMoves[squad][lastIndex - 1], position) && lastIndex < 6)
                    canMove = true;

                if (canMove)
                {
                    squadMoves[squad][lastIndex] = position;
                    lastMoveIndex[squad]++;
                }

                return canMove;
            }

            public void RemoveLastMove(int squad)
            {
                if (lastMoveIndex[squad] > 0)
                {
                    squadMoves[squad][lastMoveIndex[squad]] = -1;
                    lastMoveIndex[squad]--;
                }
            }

            public BattlePlan attackPlan
            {
                get
                {
                    return new BattlePlan(squadMoves, squadCounts, true, null);
                }
            }
        }

        public class DefensePlanner
        {
            private long[][] squadCounts = new long[4][];
            private List<List<int>> defensePositions = new List<List<int>>();
            private bool[] reactToNewlyAdjacents = new bool[4];
            private AdjacencyMatrix adjacencyMatrix = new AdjacencyMatrix();

            public DefensePlanner(Squad[] squads)
            {
                for (int s = 0; s < squads.Length; s++)
                {
                    AddSquad(s, squads[s].fullSquad);
                }
            }

            public void AddSquad(int squad, long[] squadCount)
            {
                squadCounts[squad] = squadCount;
                defensePositions.Add(new List<int>());
                reactToNewlyAdjacents[squad] = true;
            }

            public void SetSquadPosition(int squad, int position)
            {
                defensePositions[squad].Add(position);
                defensePositions[squad].AddRange(adjacencyMatrix.GetAllAdjacentIndices(position, true));
            }

            public void ToggleSquadReactCommand(int squad)
            {
                reactToNewlyAdjacents[squad] = !reactToNewlyAdjacents[squad];
            }

            public void ToggleDefenseZone(int squad, int position)
            {
                if (defensePositions.Count != 0 && defensePositions[squad][0] != position)
                {
                    if (adjacencyMatrix.IsAdjacent(defensePositions[squad][0], position))
                    {
                        if (defensePositions[squad].Contains(position))
                            defensePositions[squad].Remove(position);
                        else
                            defensePositions[squad].Add(position);
                    }
                }
            }

            public bool GetStateOfPosition(int squad, int position)
            {
                return defensePositions[squad].Contains(position);
            }

            public BattlePlan defensePlan
            {
                get
                {
                    List<int[]> tempDefenses = new List<int[]>();

                    for (int t = 0; t < defensePositions.Count; t++)
                    {
                        tempDefenses.Add(defensePositions[t].ToArray());
                    }

                    return new BattlePlan(tempDefenses.ToArray(), squadCounts, false, reactToNewlyAdjacents);
                }
            }
        }

        public class Squad
        {
            public long riflemen, machineGunners, bazookamen;
            public long lightTanks, mediumTanks, heavyTanks;
            public long lightFighters, mediumFighters, bombers;
            public long troopBunkers, tankBunkers, antiAircrafts;
            public double totalHealth, totalDamage;
            public float[] unitProbabilities;
            public int damagedUnit;
            public double remainingHealth;

            public Squad()
            {
            }

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

                unitProbabilities = new float[12];

                damagedUnit = -1;
                remainingHealth = 0.0;
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

            public void CalculateCasualties(Squad attacker, CombatTables tables)
            {
                long[] units = fullSquad;
                double cumulativeDamage = 0;
                long unitCount = GetTotalUnitCount();

                if (totalHealth > attacker.totalDamage)
                {
                    System.Random random = new System.Random();

                    if (damagedUnit != -1)
                    {
                        if (remainingHealth <= attacker.totalDamage)
                        {
                            cumulativeDamage += remainingHealth;
                            remainingHealth = 0;
                            units[damagedUnit]--;
                            unitCount--;
                            damagedUnit = -1;
                        }
                        else
                        {
                            remainingHealth -= attacker.totalDamage;
                            cumulativeDamage = attacker.totalDamage;
                        }

                        CalculateUnitProbabilities(tables, units);
                    }

                    while (cumulativeDamage < attacker.totalDamage && unitCount > 0)
                    {
                        int deadUnit = GetUnitByProbability(random);

                        cumulativeDamage += tables.healthTable[deadUnit];

                        if (cumulativeDamage < attacker.totalDamage)
                        {
                            units[deadUnit]--;
                            unitCount--;
                            CalculateUnitProbabilities(tables, units);
                        }
                        else
                        {
                            remainingHealth = cumulativeDamage - attacker.totalDamage;
                            damagedUnit = deadUnit;
                            cumulativeDamage = attacker.totalDamage;
                        }
                    }
                }
                else
                {
                    remainingHealth = 0;
                    damagedUnit = -1;
                    units = new long[12];
                }

                SetUnits(units);
            }

            int GetUnitByProbability(System.Random random)
            {
                double threshold = random.NextDouble();
                double currentRange = 0.0;

                for (int p = 0; p < unitProbabilities.Length; p++)
                {
                    if (threshold <= currentRange + unitProbabilities[p])
                    {
                        return p;
                    }

                    currentRange += unitProbabilities[p];
                }

                return -1;
            }

            public void CalculateUnitProbabilities(CombatTables tables)
            {
                CalculateUnitProbabilities(tables, fullSquad);
            }

            public void CalculateUnitProbabilities(CombatTables tables, long[] units)
            {
                float[] relativeProbabilities = new float[units.Length];
                float[] trueProbabilities = new float[units.Length];
                float totalRelatives = 0;

                for (int r = 0; r < relativeProbabilities.Length; r++)
                {
                    relativeProbabilities[r] = (float)(units[r]) * tables.orderProbabilityTable[r];
                    totalRelatives += relativeProbabilities[r];
                }

                for (int t = 0; t < trueProbabilities.Length; t++)
                {
                    trueProbabilities[t] = relativeProbabilities[t] / totalRelatives;
                }

                unitProbabilities = trueProbabilities;
            }

            long GetTotalUnitCount()
            {
                long total = 0;
                long[] units = fullSquad;

                for (int u = 0; u < units.Length; u++)
                {
                    total += units[u];
                }

                return total;
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
                //12x12 grid - troop, machine, zook, lTank, mTank, hTank, lPlane, mPlane, bomber, troopBunk, tankBunk, airBunk
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
                {1.0f, 1.0f, 1.0f,     1.5f, 0.25f, 0.25f,  4.0f, 3.5f, 2.0f,     0.0f, 0.0f, 0.0f}
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

                double[] healths = CalculateTotalHealth(blufor, opfor);
                blufor.totalHealth = healths[0];
                opfor.totalHealth = healths[1];

                double[] damages = CalculateTotalDamage(blufor, opfor);
                blufor.totalDamage = damages[0];
                opfor.totalDamage = damages[1];

                blufor.CalculateUnitProbabilities(tables);
                opfor.CalculateUnitProbabilities(tables);
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
                    blufor.CalculateCasualties(opfor, tables);
                    opfor.CalculateCasualties(blufor, tables);

                    double[] healths = CalculateTotalHealth(blufor, opfor);
                    blufor.totalHealth = healths[0];
                    opfor.totalHealth = healths[1];

                    double[] damages = CalculateTotalDamage(blufor, opfor);
                    blufor.totalDamage = damages[0];
                    opfor.totalDamage = damages[1];

                    bluforHistory.Add(blufor.fullSquad);
                    opforHistory.Add(opfor.fullSquad);

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
                        winner = "none";
                        engagementIsOver = true;
                    }
                }

                return new EngagementHistory(Get2DHistory(bluforHistory), Get2DHistory(opforHistory), winner, winningSquad);
            }

            double[] CalculateTotalHealth(Squad squadA, Squad squadB)
            {
                long[] unitsA = squadA.fullSquad;
                long[] unitsB = squadB.fullSquad;
                double totalHealthA = 0;
                double totalHealthB = 0;

                for (int u = 0; u < unitsA.Length; u++)
                {
                    totalHealthA += unitsA[u] * tables.healthTable[u];
                    totalHealthB += unitsB[u] * tables.healthTable[u];
                }

                return new double[] { totalHealthA, totalHealthB };
            }

            double[] CalculateTotalDamage(Squad squadA, Squad squadB)
            {
                long[] unitsA = squadA.fullSquad;
                long[] unitsB = squadB.fullSquad;
                double totalAttackA = 0;
                double totalAttackB = 0;
                long unitCountA = 0;
                long unitCountB = 0;

                for (int u = 0; u < unitsA.Length; u++)
                {

                    unitCountA += unitsA[u];
                    unitCountB += unitsB[u];

                    for (int m = 0; m < unitsB.Length; m++)
                    {
                        totalAttackA += (unitsA[u] * tables.damageTable[u]) * (unitsB[m] * tables.modifierTable[u, m]);
                        totalAttackB += (unitsB[u] * tables.damageTable[u]) * (unitsA[m] * tables.modifierTable[u, m]);
                    }
                }

                return new double[] { totalAttackA / unitCountB, totalAttackB / unitCountA };
            }

            int GetUnitByProbability(System.Random random, float[] probabilities)
            {
                double threshold = random.NextDouble();
                double currentRange = 0.0;

                for (int p = 0; p < probabilities.Length; p++)
                {
                    if (threshold <= currentRange + probabilities[p])
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
}
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
            public double[,] bluforHistory;
            public double[,] opforHistory;

            public string winner;
            public Squad remainingSquad;

            public EngagementHistory(double[,] _bluforHistory, double[,] _opforHistory, string _winner, Squad winningSquad)
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
            public double[][] squadCounts;

            public bool isAttacker;

            public BattlePlan(int[][] positions, double[][] squads, bool attacker)
            {
                squadPositions = positions;
                squadCounts = squads;
                isAttacker = attacker;

            }
        }

        public static class AdjacencyMatrix
        {
            //https://en.wikipedia.org/wiki/Adjacency_matrix
            private static int[,] adjacencyMatrix = new int[,]
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

            public static bool IsAdjacent(int currentPosition, int destination)
            {
                bool adjacent = false;

                if (currentPosition >= 0 && currentPosition < adjacencyMatrix.Length && destination >= 0 && destination < adjacencyMatrix.Length)
                {
                    if (adjacencyMatrix[currentPosition, destination] > 0)
                        adjacent = true;
                }

                return adjacent;
            }

            public static int mapSize
            {
                get
                {
                    return adjacencyMatrix.Length;
                }
            }

            public static int[] GetAllAdjacentIndices(int position, bool excludeSelf)
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
            private double[][] squadCounts = new double[3][];
            int[][] squadMoves = new int[3][];
            private int[] lastMoveIndex;

            public AttackPlanner(Squad[] squads)
            {
                lastMoveIndex = new int[squads.Length];

                for (int s = 0; s < squads.Length; s++)
                {
                    AddSquad(s, squads[s].fullSquad);
                }
            }

            public void AddSquad(int squad, double[] squadCount)
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
                else if (AdjacencyMatrix.IsAdjacent(squadMoves[squad][lastIndex - 1], position) && lastIndex < 6)
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
                    return new BattlePlan(squadMoves, squadCounts, true);
                }
            }
        }

        public class DefensePlanner
        {
            private double[][] squadCounts = new double[4][];
            private List<List<int>> defensePositions = new List<List<int>>();
            private bool[] reactToNewlyAdjacents = new bool[4];

            public DefensePlanner(Squad[] squads)
            {
                for (int s = 0; s < squads.Length; s++)
                {
                    AddSquad(s, squads[s].fullSquad);
                }
            }

            public void AddSquad(int squad, double[] squadCount)
            {
                squadCounts[squad] = squadCount;
                defensePositions.Add(new List<int>());
                reactToNewlyAdjacents[squad] = true;
            }

            public void SetSquadPosition(int squad, int position)
            {
                defensePositions[squad].Add(position);
                defensePositions[squad].AddRange(AdjacencyMatrix.GetAllAdjacentIndices(position, true));
            }

            public void ToggleSquadReactCommand(int squad)
            {
                reactToNewlyAdjacents[squad] = !reactToNewlyAdjacents[squad];
            }

            public void ToggleDefenseZone(int squad, int position)
            {
                if (defensePositions.Count != 0 && defensePositions[squad][0] != position)
                {
                    if (AdjacencyMatrix.IsAdjacent(defensePositions[squad][0], position))
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

                    return new BattlePlan(tempDefenses.ToArray(), squadCounts, false);
                }
            }
        }

        //Eventually remove all of the named variables and convert it to just a double array
        public class Squad
        {
            public double riflemen, machineGunners, bazookamen;
            public double lightTanks, mediumTanks, heavyTanks;
            public double lightFighters, mediumFighters, bombers;
            public double troopBunkers, tankBunkers, antiAircrafts;
            public double totalHealth, totalDamage;
            public double[] unitProbabilities;
            public int damagedUnit;
            public double remainingHealth;

            public Squad()
            {
               
            }

            public Squad(int[] squadCounts)
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

                if (squadCounts.Length > 9)
                {
                    troopBunkers = squadCounts[9];
                    tankBunkers = squadCounts[10];
                    antiAircrafts = squadCounts[11];
                }
                else
                {
                    troopBunkers = 0;
                    tankBunkers = 0;
                    antiAircrafts = 0;
                }

                totalHealth = 0;
                totalDamage = 0;

                unitProbabilities = new double[12];

                damagedUnit = -1;
                remainingHealth = 0.0;
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

                if (squadCounts.Length > 9)
                {
                    troopBunkers = squadCounts[9];
                    tankBunkers = squadCounts[10];
                    antiAircrafts = squadCounts[11];
                }
                else
                {
                    troopBunkers = 0;
                    tankBunkers = 0;
                    antiAircrafts = 0;
                }

                totalHealth = 0;
                totalDamage = 0;

                unitProbabilities = new double[12];

                damagedUnit = -1;
                remainingHealth = 0.0;
            }

            public Squad(double[] squadCounts)
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

                if (squadCounts.Length > 9)
                {
                    troopBunkers = squadCounts[9];
                    tankBunkers = squadCounts[10];
                    antiAircrafts = squadCounts[11];
                }
                else
                {
                    troopBunkers = 0;
                    tankBunkers = 0;
                    antiAircrafts = 0;
                }

                totalHealth = 0;
                totalDamage = 0;

                unitProbabilities = new double[12];

                damagedUnit = -1;
                remainingHealth = 0.0;
            }

            public double[] fullSquad
            {
                get
                {
                    double[] squad = new double[]
                    {
                    riflemen, machineGunners, bazookamen,
                    lightTanks, mediumTanks, heavyTanks,
                    lightFighters, mediumFighters, bombers,
                    troopBunkers, tankBunkers, antiAircrafts
                    };

                    return squad;
                }
            }

            public double[] onlyUnits
            {
                get
                {
                    double[] squad = new double[]
                    {
                    riflemen, machineGunners, bazookamen,
                    lightTanks, mediumTanks, heavyTanks,
                    lightFighters, mediumFighters, bombers
                    };

                    return squad;
                }
            }

            public int[] bunkers
            {
                get { return new int[] {(int)troopBunkers * 1, (int)tankBunkers * 2, (int)antiAircrafts * 3 }; }
            }

            public void AddBunkers(int[] bunkers)
            {
                if (bunkers[0] > 0)
                    troopBunkers = 1;
                if (bunkers[1] > 0)
                    tankBunkers = 1;
                if (bunkers[2] > 0)
                    antiAircrafts = 1;
            }

            public void CalculateCasualties(Squad attacker, ref MudHeroRandom random, float[] unitHealths, float[] unitOrderProbabilities)
            {
                double[] units = fullSquad;
                double cumulativeDamage = 0;
                double unitCount = GetTotalUnitCount();

                if (totalHealth > attacker.totalDamage)
                {
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

                        CalculateUnitProbabilities(units, unitOrderProbabilities);
                    }

                    while (cumulativeDamage < attacker.totalDamage && unitCount > 0)
                    {
                        int deadUnit = GetUnitByProbability(ref random);

                        cumulativeDamage += unitHealths[deadUnit];

                        if (cumulativeDamage < attacker.totalDamage)
                        {
                            units[deadUnit]--;
                            unitCount--;
                            CalculateUnitProbabilities(units, unitOrderProbabilities);
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
                    units = new double[12];
                }

                SetUnits(units);
            }

            int GetUnitByProbability(ref MudHeroRandom random)
            {
                double threshold = random.Value();
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

            public void CalculateUnitProbabilities(float[] unitOrderProbabilities)
            {
                CalculateUnitProbabilities(fullSquad, unitOrderProbabilities);
            }

            public void CalculateUnitProbabilities(double[] units, float[] unitOrderProbabilities)
            {
                double[] relativeProbabilities = new double[units.Length];
                double[] trueProbabilities = new double[units.Length];
                double totalRelatives = 0;

                for (int r = 0; r < relativeProbabilities.Length; r++)
                {
                    relativeProbabilities[r] = (float)(units[r]) * unitOrderProbabilities[r];
                    totalRelatives += relativeProbabilities[r];
                }

                for (int t = 0; t < trueProbabilities.Length; t++)
                {
                    trueProbabilities[t] = relativeProbabilities[t] / totalRelatives;
                }

                unitProbabilities = trueProbabilities;
            }

            public double GetTotalUnitCount()
            {
                double total = 0;
                double[] units = fullSquad;

                for (int u = 0; u < units.Length; u++)
                {
                    total += units[u];
                }

                return total;
            }

            public void SetUnits(double[] squad)
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

        public class Engagement
        {
            public Squad blufor, opfor;

            public Engagement(Squad _blufor, Squad _opfor, Constants constants)
            {
                blufor = _blufor;
                opfor = _opfor;

                double[] healths = CalculateTotalHealth(blufor, opfor, constants.unitHealths);
                blufor.totalHealth = healths[0];
                opfor.totalHealth = healths[1];

                double[] damages = CalculateTotalDamage(blufor, opfor, constants.unitDamages, constants.unitCombatModifiers);
                blufor.totalDamage = damages[0];
                opfor.totalDamage = damages[1];

                blufor.CalculateUnitProbabilities(constants.unitOrderProbabilities);
                opfor.CalculateUnitProbabilities(constants.unitOrderProbabilities);
            }

            public EngagementHistory ResolveEngagement(ref MudHeroRandom random, Constants constants)
            {
                List<double[]> bluforHistory = new List<double[]>();
                List<double[]> opforHistory = new List<double[]>();
                bool engagementIsOver = false;
                string winner = "";
                Squad winningSquad = new Squad();

                while (!engagementIsOver)
                {
                    blufor.CalculateCasualties(opfor, ref random, constants.unitHealths, constants.unitOrderProbabilities);
                    opfor.CalculateCasualties(blufor, ref random, constants.unitHealths, constants.unitOrderProbabilities);

                    double[] healths = CalculateTotalHealth(blufor, opfor, constants.unitHealths);
                    blufor.totalHealth = healths[0];
                    opfor.totalHealth = healths[1];

                    double[] damages = CalculateTotalDamage(blufor, opfor, constants.unitDamages, constants.unitCombatModifiers);
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

            double[] CalculateTotalHealth(Squad squadA, Squad squadB, float[] unitHealths)
            {
                double[] unitsA = squadA.fullSquad;
                double[] unitsB = squadB.fullSquad;
                double totalHealthA = 0;
                double totalHealthB = 0;

                for (int u = 0; u < unitsA.Length; u++)
                {
                    totalHealthA += unitsA[u] * unitHealths[u];
                    totalHealthB += unitsB[u] * unitHealths[u];
                }

                return new double[] { totalHealthA, totalHealthB };
            }

            double[] CalculateTotalDamage(Squad squadA, Squad squadB, float[] unitDamages, float[,] unitCombatModifiers)
            {
                double[] unitsA = squadA.fullSquad;
                double[] unitsB = squadB.fullSquad;
                double totalAttackA = 0;
                double totalAttackB = 0;
                double unitCountA = 0;
                double unitCountB = 0;

                for (int u = 0; u < unitsA.Length; u++)
                {

                    unitCountA += unitsA[u];
                    unitCountB += unitsB[u];

                    for (int m = 0; m < unitsB.Length; m++)
                    {
                        totalAttackA += (unitsA[u] * unitDamages[u]) * (unitsB[m] * unitCombatModifiers[u, m]);
                        totalAttackB += (unitsB[u] * unitDamages[u]) * (unitsA[m] * unitCombatModifiers[u, m]);
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

            double[,] Get2DHistory(List<double[]> listHistory)
            {
                double[,] history = new double[listHistory.Count, listHistory[0].Length];

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
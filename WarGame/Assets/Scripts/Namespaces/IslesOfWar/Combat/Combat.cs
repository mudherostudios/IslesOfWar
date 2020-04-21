using System.Collections.Generic;

namespace IslesOfWar.Combat
{
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
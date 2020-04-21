using System.Collections.Generic;

namespace IslesOfWar.Combat
{
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
}

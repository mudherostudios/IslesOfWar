namespace IslesOfWar.Combat
{
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
}

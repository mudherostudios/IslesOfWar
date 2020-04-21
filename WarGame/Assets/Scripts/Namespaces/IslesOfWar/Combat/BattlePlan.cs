namespace IslesOfWar.Combat
{
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
}

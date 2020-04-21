namespace IslesOfWar.Combat
{
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
}

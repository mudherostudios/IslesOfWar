namespace IslesOfWar.Communication
{
    public class SquadWithdrawl
    {
        public string id;
        public int[] sqds;

        public SquadWithdrawl() { }

        public SquadWithdrawl(string _id)
        {
            id = _id;
            sqds = new int[] { 0, 1, 2, 3 };
        }

        public SquadWithdrawl(string _id, int squad)
        {
            id = _id;
            sqds = new int[] { squad };
        }

        public SquadWithdrawl(string _id, int[] squads)
        {
            id = _id;
            sqds = squads;
        }
    }
}

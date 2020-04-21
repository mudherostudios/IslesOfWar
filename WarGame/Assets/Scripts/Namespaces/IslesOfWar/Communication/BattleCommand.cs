using System.Collections.Generic;

namespace IslesOfWar.Communication
{
    public class BattleCommand
    {
        public string id;
        public List<List<int>> pln;
        public List<List<int>> sqd;

        public BattleCommand() { }

        public BattleCommand(string _id)
        {
            id = _id;
            pln = new List<List<int>>();
            sqd = new List<List<int>>();
        }

        public BattleCommand(string _id, int[][] plan, int[][] squad)
        {
            id = _id;
            pln = new List<List<int>>();
            sqd = new List<List<int>>();

            if (_id != null && plan != null && squad != null)
            {
                for (int s = 0; s < plan.Length; s++)
                {
                    pln.Add(new List<int>(plan[s]));
                }

                for (int s = 0; s < squad.Length; s++)
                {
                    sqd.Add(new List<int>(squad[s]));
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IslesOfWar.Communication
{
    public class AirDrop
    {
        public string[] players; //Name as it is in the state dictionaries as keys or values.
        public double[] amount;  //Add to state.players[player].resources - 4
        public string reason; //Just a reason that can be displayed but also doubles as transparency for community.
    }
}

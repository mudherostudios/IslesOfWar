using System.Collections.Generic;

namespace IslesOfWar.Communication
{
    public class ResourceOrder
    {
        public int rsrc;
        public List<double> amnt;

        public ResourceOrder() { }

        public ResourceOrder(int resourceType, List<double> amounts)
        {
            rsrc = resourceType;
            amnt = amounts;
        }
    }
}

using System;

namespace IslesOfWar.ClientSide
{
    public class ChiOrderData
    {
        public Guid id;
        public string owner;
        public decimal price;
        public double warbux;

        public string ID { get { return id.ToString(); } }

        public ChiOrderData() { id = new Guid(); owner = ""; }
        public ChiOrderData(Guid _id, string _owner, decimal _price, double _warbux)
        {
            id = _id;
            owner = _owner;
            price = _price;
            warbux = _warbux;
        }
    }
}

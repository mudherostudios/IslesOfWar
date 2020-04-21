namespace IslesOfWar.ClientSide
{
    //Phase out
    public struct Cost
    {
        public double amount;
        public double[] costs;
        public string type;

        public Cost(double[,] _costs, int row, double _amount)
        {
            costs = new double[] { _costs[row, 0], _costs[row, 1], _costs[row, 2], _costs[row, 3] };
            amount = _amount;
            type = row.ToString();
        }

        public Cost(double[] _costs, double _amount, string _type)
        {
            costs = _costs;
            amount = _amount;
            type = _type;
        }

    }
}

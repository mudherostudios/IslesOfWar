using MudHero;
namespace IslesOfWar.ClientSide
{
    public class MarketOrder
    {
        public double[] selling; //4 doubles - one for every resource
        public double[] buying;  //4 doubles - one for every resource
        public string orderID;   //8 characters from TXID of action

        public MarketOrder()
        {
            selling = new double[4];
            buying = new double[4];
            orderID = "";
        }

        public MarketOrder(double[] sell, double[] buy, string ID)
        {
            selling = Deep.CopyObject<double[]>(sell);
            buying = Deep.CopyObject<double[]>(buy);
            orderID = ID;
        }
    }
}

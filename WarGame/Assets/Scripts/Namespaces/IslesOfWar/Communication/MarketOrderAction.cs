using MudHero;

namespace IslesOfWar.Communication
{
    public class MarketOrderAction
    {
        public double[] sell;
        public double[] buy;

        public MarketOrderAction(double[] resourcesToSell, double[] resourcesToBuy)
        {
            sell = Deep.CopyObject<double[]>(resourcesToSell);
            buy = Deep.CopyObject<double[]>(resourcesToBuy);
        }
    }
}

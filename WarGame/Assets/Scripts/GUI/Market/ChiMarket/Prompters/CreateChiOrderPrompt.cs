public class CreateChiOrderPrompt : ChiPrompt
{
    public ChiOrderFormation former;
    public ChiMarket market;

    public void Ok()
    {
        former.CreateChiOrder();
        StartCoroutine(market.Wait(4));
        market.Refresh();
        Close();
    }

    public void Cancel()
    {
        former.CancelChiOffer();
        Close();
    }
}

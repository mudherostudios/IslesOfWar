public class CreateChiOrderPrompt : ChiPrompt
{
    public ChiOrderFormation former;

    public void Ok()
    {
        former.CreateChiOrder();
        Close();
    }

    public void Cancel()
    {
        former.CancelChiOffer();
        Close();
    }
}

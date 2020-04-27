public class GotoChiMarket : MarketMenuButton
{
    public void GotoChiMarketMenu()
    {
        MainMenu.SetActive(false);
        ResourceMenu.SetActive(false);
        ChiMenu.SetActive(true);
        BackButton.AtMainMenu = false;
    }
}

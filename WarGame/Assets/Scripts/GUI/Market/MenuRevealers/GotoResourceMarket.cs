public class GotoResourceMarket : MarketMenuButton
{
    public void GotoResourceMarketMenu()
    {
        MainMenu.SetActive(false);
        ChiMenu.SetActive(false);
        ResourceMenu.SetActive(true);
        BackButton.AtMainMenu = false;
    }
}

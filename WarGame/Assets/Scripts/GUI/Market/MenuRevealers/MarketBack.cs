using UnityEngine;

public class MarketBack : MarketMenuButton
{
    public int MainSceneIndex;
    public bool AtMainMenu;

    public void Back()
    {
        if (!AtMainMenu)
        {
            ResourceMenu.SetActive(false);
            ChiMenu.SetActive(false);
            MainMenu.SetActive(true);
            AtMainMenu = true;
        }
        else
        {
            Debug.Log("Pretending to load IslandMenu...");
        }
    }
}

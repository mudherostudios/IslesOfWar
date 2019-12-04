using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IslesOfWar;

public class PurchasePackMenu : MonoBehaviour
{
    public WorldNavigator nav;
    public CommunicationInterface comScript;
    public Constants constants;
    public Text warbucksAmount, oilAmount, metalAmount, concreteAmount, chiAmount;
    public InputField packCount;
    int packs = 0;

    public void Start()
    {
        nav = GameObject.FindGameObjectWithTag("GameMaster").GetComponent<WorldNavigator>();
        constants = nav.clientInterface.chainState.currentConstants;
        comScript = nav.communicationScript;
    }

    public void PurchasePacks()
    {
        nav.clientInterface.BuyResourcePack(packs);
        nav.commandScript.CloseMenus();
        nav.ShowActions();
    }

    public void UpdateCounts()
    {
        int oldPacks = packs;
        bool parsed = int.TryParse(packCount.text, out packs) && packs > 0;

        if (!parsed)
            packs = oldPacks;
        else
            CheckMax(oldPacks);

        if (parsed)
        {
            warbucksAmount.text = (constants.resourcePackAmount[0] * packs).ToString();
            oilAmount.text = (constants.resourcePackAmount[1] * packs).ToString();
            metalAmount.text = (constants.resourcePackAmount[2] * packs).ToString();
            concreteAmount.text = (constants.resourcePackAmount[3] * packs).ToString();
            chiAmount.text = (constants.resourcePackCost * packs).ToString("G8", CultureInfo.InvariantCulture);
        }
    }

    void CheckMax(int oldPacks)
    {
        if (!comScript.HasSufficientFunds(packs * constants.resourcePackCost))
        {
            packs = oldPacks;
            packCount.text = packs.ToString();
        }
    }
    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionIdentifier : MonoBehaviour
{
    public ClientInterface client;
    public GameObject identifier;
    public GameObject invalidIdentifier;
    public int actionType = -1;
    public int subType = -1;
    bool isValid = true;

    public void Hide()
    {
        gameObject.SetActive(false);
        identifier.SetActive(false);
        invalidIdentifier.SetActive(false);
        isValid = true;
    }

    public void Show(bool valid) { isValid = valid; }
    public void Show()
    {
        gameObject.SetActive(true);
        identifier.SetActive(isValid);
        invalidIdentifier.SetActive(!isValid);
    }

    void ShowOnConditionIsTrue(bool condition)
    {
        if (condition)
            Show();
    }

    public void ChangeValidityTo(bool valid)
    {
        isValid = valid;
        identifier.SetActive(isValid);
        invalidIdentifier.SetActive(!isValid);
    }

    public void CancelAction()
    {
        switch (actionType)
        {
            case 0: //Troops
                client.CancelUnitPurchase(actionType);
                break;
            case 1: //Tanks
                client.CancelUnitPurchase(actionType);
                break;
            case 2: //Aircraft
                client.CancelUnitPurchase(actionType);
                break;
            case 3:
                client.CancelResourceDeposit();
                break;
            case 4:
                client.CancelWarbucksContribution();
                break;
            case 5:
                client.CancelIslandSearch();
                break;
            case 6: //DefensePlan
                client.CancelPlan(false);
                break;
            case 7: //AttackPlan
                client.CancelPlan(true);
                break;
            case 8:
                client.CancelIslandDevelopment();
                break;
            case 9:
                client.CancelBuyResourcePack();
                break;
            case 10:
                client.CancelNationChange();
                break;
            default:
                client.notificationSystem.PushNotification(0, 1, "Not a valid action type that you can remove.");
                break;
        }

        Hide();
    }

    public void ShowIfQueued()
    {
        switch (actionType)
        {
            case 0: //Troops
                if (client.queuedActions.buy != null)
                    ShowOnConditionIsTrue(client.queuedActions.buy[0] + client.queuedActions.buy[1] + client.queuedActions.buy[2] > 0);
                break;
            case 1: //Tanks
                if (client.queuedActions.buy != null)
                    ShowOnConditionIsTrue(client.queuedActions.buy[3] + client.queuedActions.buy[4] + client.queuedActions.buy[5] > 0);
                break;
            case 2: //Aircraft
                if (client.queuedActions.buy != null)
                    ShowOnConditionIsTrue(client.queuedActions.buy[6] + client.queuedActions.buy[7] + client.queuedActions.buy[8] > 0);
                break;
            case 3:
                if (client.queuedActions.pot != null && subType == client.queuedActions.pot.rsrc)
                    Show();
                break;
            case 4:
                if (client.queuedActions.dep != null)
                    Show();
                break;
            case 5:
                if (client.queuedActions.srch != null)
                    Show();
                break;
            case 6: //DefensePlan
                if (client.queuedActions.dfnd != null)
                    Show();
                break;
            case 7: //AttackPlan
                if (client.queuedActions.attk != null)
                    Show();
                break;
            case 8:
                if (client.queuedActions.bld != null)
                    Show();
                break;
            case 9:
                if (client.queuedActions.igBuy != 0)
                    Show();
                break;
            case 10:
                if (client.queuedActions.nat != null && client.isPlaying)
                    Show();
                break;
            default:
                client.notificationSystem.PushNotification(0, 1, string.Format("This action identifier has no valid type - {0}", gameObject.name));
                break;
        }
    }
    
}

using System.Collections.Generic;
using System.Linq;
using IslesOfWar.ClientSide;
using IslesOfWar.Communication;
using UnityEngine;
using Newtonsoft.Json;

public class BaseClient : MonoBehaviour
{
    protected State gameState;
    protected CommunicationInterface commsInterface;
    protected PlayerActions queuedActions;
    public string player;
    protected JsonSerializerSettings settings;
    
    public State State { get { return gameState; } }
    public string Player { get { return player; } }

    protected double[] queuedExpenditures;

    public double[] PlayerResources
    {
        get
        {
            if (gameState != null) return gameState.players[player].resources.ToArray();
            else return new double[] { -1.0, -1.0, -1.0, -1.0 };
        }
    }

    private void Start()
    {
        queuedExpenditures = new double[4];
        settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }; 
        //commsInterface = GameObject.FindGameObjectWithTag("CommunicationInterface").GetComponent<CommunicationInterface>();
    }

    protected void CleanEmptyPlans()
    {
        CleanLists(queuedActions.dfnd.pln);
        CleanLists(queuedActions.attk.pln);
    }

    private void CleanLists(List<List<int>> plans)
    {
        if (plans != null)
        {
            List<List<int>> cleanedPlans = new List<List<int>>();

            for (int p = 0; p < plans.Count; p++)
                if (plans[p].Count > 0) cleanedPlans.Add(plans[p].ToArray().ToList());

            if (cleanedPlans.Count > 0)
            {
                plans.Clear();
                plans = cleanedPlans;
            }
            else plans = null;
        }
    }

    protected void SpendResources(double[] resources)
    {
        queuedExpenditures[0] += resources[0];
        queuedExpenditures[1] += resources[1];
        queuedExpenditures[2] += resources[2];
        queuedExpenditures[3] += resources[3];
    }

    protected void UnspendResources(double[] resources)
    {
        queuedExpenditures[0] -= resources[0];
        queuedExpenditures[1] -= resources[1];
        queuedExpenditures[2] -= resources[2];
        queuedExpenditures[3] -= resources[3];
    }

    protected void SendOrderToBlockchain()
    {
        string action = JsonConvert.SerializeObject(queuedActions, Formatting.None, settings);
        Debug.Log(action);
        //string command = string.Format("{{\"g\":{{\"iow\":{0}}}}}", action);
        //commsInterface.SendCommand(command);
    }

    protected void ClearQueuedActions() { queuedActions = new PlayerActions(); }

    
}

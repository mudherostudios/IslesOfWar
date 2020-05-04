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
    protected PlayerActions queuedActions = new PlayerActions();
    protected string player;
    protected JsonSerializerSettings settings;
    
    public State State { get { return gameState; } }
    public string Player { get { return player; } }

    private double[] queuedExpenditures = new double[4];

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
        settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }; 
        commsInterface = GameObject.FindGameObjectWithTag("CommunicationInterface").GetComponent<CommunicationInterface>();
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
        for (int r = 0; r < resources[r]; r++)
            queuedExpenditures[r] += resources[r];
    }

    protected void UnspendResources(double[] resources)
    {
        for(int r = 0; r < resources[r]; r++)
            queuedExpenditures[r] -= resources[r];
    }

    public double[] GetSubtractedResources()
    {
        double[] subtracted = PlayerResources;

        for (int q = 0; q < queuedExpenditures[q]; q++)
            subtracted[q] -= queuedExpenditures[q];

        return subtracted;
    }

    public void SendOrderToBlockchain()
    {
        string action = JsonConvert.SerializeObject(queuedActions, Formatting.None, settings);
        string command = string.Format("{{\"g\":{{\"iow\":{0}}}}}", action);
        Debug.Log(command);
        commsInterface.SendCommand(command);
        queuedActions = new PlayerActions();
    }

    public bool HasComms { get { return commsInterface != null; } }
    public int Progress { get { return commsInterface.blockProgress; } }

    protected void ClearQueuedActions() { queuedActions = new PlayerActions(); }
    public void FindCommsInterface() { commsInterface = GameObject.FindGameObjectWithTag("CommunicationInterface").GetComponent<CommunicationInterface>(); }
    public void SetPlayer() { if(HasComms) player = commsInterface.player; }
    public void UpdateState() { if (HasComms) gameState = JsonConvert.DeserializeObject<State>(JsonConvert.SerializeObject(commsInterface.state)); }
}

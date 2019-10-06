using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SearchIslands : MonoBehaviour
{
    public Text warbuxCost;
    public Text oilCost;
    public Text metalCost;
    public Text concreteCost;
    public CommandIslandInteraction commandScript;

    private double[] cost;
    
    public void UpdateCost()
    {
        cost = commandScript.GetIslandSearchCost();

        string formatW = "";
        string formatO = "";
        string formatM = "";
        string formatC = "";

        if (cost[0] > 999999999)
            formatW = "G2";
        if (cost[1] > 999999999)
            formatO = "G2";
        if (cost[2] > 999999999)
            formatM = "G2";
        if (cost[3] > 999999999)
            formatC = "G2";

        warbuxCost.text = cost[0].ToString(formatW);
        oilCost.text = cost[1].ToString(formatO);
        metalCost.text = cost[2].ToString(formatM);
        concreteCost.text = cost[3].ToString(formatC);
    }

    public void SearchForIslands()
    {
        commandScript.SearchForIslands();
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        UpdateCost();
    }
}

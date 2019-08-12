using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TestNameUpdates & TestActionParsing
using IslesOfWar.Communication;
using IslesOfWar.GameStateProcessing;

//TestActionParsing
using Newtonsoft.Json;

public class Tests : MonoBehaviour
{
    public string blockData; //should have valid block data but not valid move data
    public string validBlockData; //should have valid block data and move data
    public string invalidGameMoves;
    public string validGameMoves;
    public string databasePath = "/database/";
    public string dbName = "";
    public bool DEBUG = true;

    private void Start()
    {
        Random.InitState(1337);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            TestNameUpdates();
        if (Input.GetKeyDown(KeyCode.Alpha2))
            TestActionParsing();
        if (Input.GetKeyDown(KeyCode.Alpha3))
            Debug.Log(Random.value);
        
    }

    void TestNameUpdates()
    {
        IslandBuildOrder buildOrder = new IslandBuildOrder();
        buildOrder.id = "0";
        buildOrder.col = "0123abcdABCD";
        buildOrder.def = "0123abcdABCD";

        List<int> purchaseOrder = new List<int> { 100, 0, 0, 10, 0, 0, 10, 0, 0 };

        ResourceOrder order = new ResourceOrder();
        order.rsrc = 0;
        order.amnt = new List<uint> { 0, 1000, 1000 };

        List<string> depletedIslands = new List<string> { "0", "A3", "BB", "1" };

        BattleCommand command = new BattleCommand();
        command.id = "0";
        command.pln = new List<List<int>> { new List<int>(), new List<int>() };
        command.pln[0] = new List<int> { 0, 1, 2, 3, 4, 5 };
        command.pln[1] = new List<int>{ 0, 1, 2, 3, 4, 5 };
        command.sqd = new List<List<int>> { new List<int>(), new List<int>() };
        command.sqd[0] = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        command.sqd[1] = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8 };

        BattleCommand defend = new BattleCommand();
        defend.id = "0";
        defend.pln = command.pln;
        defend.sqd = command.sqd;

        PlayerActions actions = new PlayerActions();

        actions.nat = "US";
        actions.bld = buildOrder;
        actions.buy = purchaseOrder;
        actions.srch = "norm";
        actions.pot = order;
        actions.dep = depletedIslands;
        actions.attk = command;
        actions.dfnd = defend;

        Debug.Log(CommandUtility.GetSerializedCommand(actions));
    }

    void TestActionParsing()
    {
        Actions actions = JsonConvert.DeserializeObject<Actions>(blockData);
        Actions validActions = JsonConvert.DeserializeObject<Actions>(validBlockData);
        PlayerActions playerActions = JsonConvert.DeserializeObject <PlayerActions>(JsonConvert.SerializeObject(validActions.moves[0].move));

        Debug.Log(actions.moves.Count);
        Debug.Log(actions.rngseed);

        for (int i = 0; i < actions.moves[0].inputs.Count; i++)
        {
            Debug.Log(actions.moves[0].inputs[i].txid);
            Debug.Log(actions.moves[0].inputs[i].vout);
        }

        Debug.Log(JsonConvert.SerializeObject(actions.moves[0].move));
        Debug.Log(actions.moves[0].name);
        Debug.Log(actions.moves[0].txid);
        Debug.Log(actions.admin);

        if (playerActions.nat != null)
        {
            string islandIDs = "";
            foreach (string id in playerActions.dep)
            {
                islandIDs += "ID: " + id + "\n";
            }

            Debug.Log
            (
                "Nation: " + playerActions.nat + "\n\n" +
                "-Build Order-" + "\n" +
                "ID: " + playerActions.bld.id + "\n" +
                "Collectors: " + playerActions.bld.col + "\n" +
                "Defenses: " + playerActions.bld.def + "\n" + "------\n\n" +
                "Unit Purchase: " + playerActions.buy[0] + "\n" +
                "Search Islands: " + playerActions.srch + "\n\n" +
                "-Resource Submissions-" + "\n" +
                "Resource Type: " + playerActions.pot.rsrc + "\n" +
                "Amounts: " + playerActions.pot.amnt[1] + "\n" + "------\n\n" +
                "-Depleted Islands-" + "\n" +
                islandIDs + "--------\n\n" +
                "Attack Plan Island: " + playerActions.attk.id + "\n" +
                "Defend Order Island: " + playerActions.dfnd.id + "\n\n"
            );
        }
        else
        {
            Debug.Log("Failed to Parse Player Actions");
        }
    }
}

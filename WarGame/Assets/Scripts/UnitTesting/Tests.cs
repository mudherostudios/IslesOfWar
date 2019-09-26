using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TestNameUpdates & TestActionParsing
using IslesOfWar.ClientSide;
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
        UnityEngine.Random.InitState(1337);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            TestNameUpdates();
        if (Input.GetKeyDown(KeyCode.Alpha2))
            TestActionParsing();
        if (Input.GetKeyDown(KeyCode.Alpha3))
            Debug.Log(UnityEngine.Random.value);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            PlayState();
        
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
        order.amnt = new List<double> { 0, 1000, 1000 };

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

    public struct Move
    {
        public string m;
    }

    //Passes the moves data to the processor and tracks undo data.
    public void PlayState()
    {
        string blockData = "{\"block\": {\"hash\": \"dda7eccde4857742e5000bd66cf72154ce26c22876582654bc8b8d78dadbce8c\",\"height\": 558369,\"parent\": \"18f72c91c7b9223e9c7d0525216277e4016d748a2c81be4ba9d4a2b30eaed92d\",\"rngseed\": \"b36747498ce183b9da32b3ab6e0d72f2a17aa06859c08cf1d1e91907cb09dddc\",\"timestamp\": 1549056526},\"moves\": [{\"move\": {\"nat\": \"US\"},\"name\": \"ALICE\",\"out\": {\"CMBPmRos5QADg2T8kvkQhMaMV5WzpzfedR\": 3443.7832612},\"txid\": \"edd0d7a7662a1b5f8ded16e333f114eb5bea343a432e6c72dfdbdcfef6bf4d44\"}],\"reqtoken\": \"1fba0f4f9e76a65b1f09f3ea40a59af8\"}";
        string updatedState = "";
        string undoData = Callback.PlayState("", blockData, "", out updatedState);

        Debug.Log(updatedState);
    }
}

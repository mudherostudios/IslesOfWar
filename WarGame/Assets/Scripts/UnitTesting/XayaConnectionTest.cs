using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IslesOfWar.Communication;
using IslesOfWar.GameStateProcessing;
using MudHero.XayaProcessing;
using Newtonsoft.Json;
public class XayaConnectionTest : MonoBehaviour
{
    public string blockData; //should have valid block data but not valid move data
    public string validBlockData; //should have valid block data and move data

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
            Test();
    }

    void Test()
    {
        IslandBuildOrder buildOrder = new IslandBuildOrder();
        buildOrder.id = "0";
        buildOrder.col = "0123abcdABCD";
        buildOrder.def = "0123abcdABCD";

        UnitPurchaseOrder purchaseOrder = new UnitPurchaseOrder();
        purchaseOrder.unitCounts = new uint[] { 100, 0, 0, 10, 0, 0, 10, 0, 0 };

        ResourceOrder order = new ResourceOrder();
        order.rsrc = 0;
        order.amnt = new uint[] { 0, 1000, 1000 };

        string[] depletedIslands = new string[] { "0", "A3", "BB", "1" };

        BattleCommand command = new BattleCommand();
        command.id = "0";
        command.pln = new int[2][];
        command.pln[0] = new int[] { 0, 1, 2, 3, 4, 5 };
        command.pln[1] = new int[] { 0, 1, 2, 3, 4, 5 };
        command.sqd = new uint[2][];
        command.sqd[0] = new uint[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        command.sqd[1] = new uint[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };

        BattleCommand defend = new BattleCommand();
        defend.id = "0";
        defend.pln = command.pln;
        defend.sqd = command.sqd;
        defend.flw = new int[] { 0, 1 };

        Actions actions = ActionParser.JsonToActions(blockData);
        Actions validActions = ActionParser.JsonToActions(validBlockData);
        PlayerActions playerActions = ActionParser.ParseMove(JsonConvert.SerializeObject(actions.moves[0].move));
        
        string data = CommandUtility.UpdateNation("ZW");
        Debug.Log(data);

        data = CommandUtility.CompleteIslandBuildCommands(buildOrder);
        Debug.Log(data);

        data = CommandUtility.CompleteUnitPurchaseOrder(purchaseOrder);
        Debug.Log(data);

        data = CommandUtility.SearchForIslands();
        Debug.Log(data);

        data = CommandUtility.SubmitToResourcePot(order);
        Debug.Log(data);

        data = CommandUtility.SubmitToCryptoPot(depletedIslands);
        Debug.Log(data);

        data = CommandUtility.AttackIsland(command);
        Debug.Log(data);

        data = CommandUtility.DefendIsland(defend);
        Debug.Log(data);
        
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
            Debug.Log("      Debugging Move Parsing      ");
            string islandIDs = "";
            foreach (string id in playerActions.dep)
            {
                islandIDs += "ID: " + id + "\n";
            }

            Debug.Log
            (
                "Nation: " + playerActions.nat + "\n" +
                "-Build Order-" + "\n" +
                "ID: " + playerActions.bld.id + "\n" +
                "Collectors: " + playerActions.bld.col + "\n" +
                "Defenses: " + playerActions.bld.def + "\n" + "------\n" +
                "Unit Purchase: " + playerActions.buy[0] + "\n" +
                "Search Islands: " + playerActions.srch + "\n" +
                "-Resource Submissions-" + "\n" +
                "Resource Type: " + playerActions.pot.rsrc + "\n" +
                "Amounts: " + playerActions.pot.amnt[1] + "\n" + "------\n" +
                "-Depleted Islands-" + "\n" +
                islandIDs + "--------\n" +
                "Attack Plan Island: " + playerActions.attk.id + "\n" +
                "Defend Order Island: " + playerActions.dfnd.id
            );
        }
        else
        {
            Debug.Log("Failed to Parse Player Actions");
        }
    }
}

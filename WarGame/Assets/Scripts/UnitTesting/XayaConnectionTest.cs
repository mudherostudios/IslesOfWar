using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IslesOfWar.Communication;
using IslesOfWar.Callbacks;

public class XayaConnectionTest : MonoBehaviour
{
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

        DefendCommand defend = new DefendCommand();
        defend.id = "0";
        defend.pln = command.pln;
        defend.sqd = command.sqd;
        defend.flw = new int[] { 0, 1 };

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
    }
}

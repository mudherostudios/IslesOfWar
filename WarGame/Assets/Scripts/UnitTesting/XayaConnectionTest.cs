using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TestNameUpdates & TestActionParsing
using IslesOfWar.Communication;
using IslesOfWar.GameStateProcessing;

//TestActionParsing
using MudHero.XayaProcessing;
using Newtonsoft.Json;

//TestSQLiteDB
using Mono.Data.Sqlite;
using System.Data;
using System.IO;

public class XayaConnectionTest : MonoBehaviour
{
    public string blockData; //should have valid block data but not valid move data
    public string validBlockData; //should have valid block data and move data
    public string databasePath = "/database/";
    public string dbName = "";
    public bool DEBUG = true;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            TestNameUpdates();
        if (Input.GetKeyDown(KeyCode.Alpha2))
            TestActionParsing();
        if (Input.GetKeyDown(KeyCode.Alpha3))
            TestSQLiteDB();
    }

    void TestNameUpdates()
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

        string nation = CommandUtility.UpdateNation("ZW");
        string build = CommandUtility.CompleteIslandBuildCommands(buildOrder);
        string purchase = CommandUtility.CompleteUnitPurchaseOrder(purchaseOrder);
        string search = CommandUtility.SearchForIslands();
        string pot = CommandUtility.SubmitToResourcePot(order);
        string crypto = CommandUtility.SubmitToCryptoPot(depletedIslands);
        string atk = CommandUtility.AttackIsland(command);
        string def = CommandUtility.DefendIsland(defend);

        Debug.Log(string.Format("{0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}\n{7}\n", nation, build, purchase, search, pot, crypto, atk, def));
    }

    void TestActionParsing()
    {
        Actions actions = ActionParser.JsonToActions(blockData);
        Actions validActions = ActionParser.JsonToActions(validBlockData);
        PlayerActions playerActions = ActionParser.ParseMove(JsonConvert.SerializeObject(validActions.moves[0].move));

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

    void TestSQLiteDB()
    {
        IDbCommand dbCommand;
        IDbCommand otherCommand;
        IDbCommand readCommand;
        IDataReader reader;
        
        string folderPath = string.Format("{0}{1}", Application.persistentDataPath, databasePath);
        string connection = string.Format("URI=file:{0}{1}",folderPath,dbName);

        if (!Directory.Exists(folderPath))
        {
            if(DEBUG)
                Debug.Log(string.Format("Creating Folder at {0}.", folderPath));
            Directory.CreateDirectory(folderPath);
        }
        else if(Directory.Exists(folderPath) && DEBUG)
        {
            
            Debug.Log("Folder Already Exists. Not Recreating.");
        }

        if(DEBUG)
            Debug.Log(string.Format("Attempting database connection with {0}.", connection));

        IDbConnection dbConnection = new SqliteConnection(connection);
        dbConnection.Open();

        if (DEBUG)
            Debug.Log("Creating Table.");

        dbCommand = dbConnection.CreateCommand();
        string sqlCreateTable = "CREATE TABLE IF NOT EXISTS my_table (id INTEGER PRIMARY KEY, val INTEGER )";
        dbCommand.CommandText = sqlCreateTable;
        reader = dbCommand.ExecuteReader();

        if (DEBUG)
            Debug.Log("Updating Table.");

        otherCommand = dbConnection.CreateCommand();
        otherCommand.CommandText = "INSERT OR REPLACE INTO my_table (id, val) VALUES (0, 5)";
        otherCommand.ExecuteNonQuery();

        if (DEBUG)
            Debug.Log("Reading Database");

        readCommand = dbConnection.CreateCommand();
        string dbQuery = "SELECT * FROM my_table";
        readCommand.CommandText = dbQuery;
        reader = readCommand.ExecuteReader();

        while (reader.Read())
        {
            Debug.Log(string.Format("id: {0}\nval: {1}\n", reader[0].ToString(), reader[1].ToString()));
        }

        reader.Close();
        dbConnection.Close();
    }
}

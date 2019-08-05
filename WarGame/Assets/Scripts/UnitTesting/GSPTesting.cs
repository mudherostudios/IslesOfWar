using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IslesOfWar.ClientSide;
using IslesOfWar.GameStateProcessing;

public class GSPTesting : MonoBehaviour
{
    StateProcessor processor;

    //Nation Update Test Data
    string playerOne = "cairo";
    string playerTwo = "caz";
    string failNation = "USA";
    string passNation = "US";
    string updateFailNation = "zw";
    string updatePassNation = "ZW";

    //Search Island Data (SI)
    Dictionary<string, PlayerState> playersSI;
    Dictionary<string, Island> islandsSI;
    string[] IIDs = new string[] {"a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o" };

    //Test Results
    string nationResults = "";
    string searchIslandResults = "";
    string[] label;
    string[] results;

    void Start()
    {
        processor = new StateProcessor();

        playersSI = new Dictionary<string, PlayerState>
        {
            {"cairo", new PlayerState("US", new long[9], new long[] {3000, 7500, 0, 0}, new string[] {IIDs[0], IIDs[1], IIDs[2], IIDs[3]}, "") },
            {"pimpMacD", new PlayerState("US", new long[9], new long[] {1000, 2500, 0, 0}, new string[] { IIDs[4], IIDs[5], IIDs[6], IIDs[7], IIDs[8]}, "") },
            {"nox", new PlayerState("MX", new long[9], new long[] {0, 0, 0, 0}, new string[] { IIDs[9], IIDs[10], IIDs[11], IIDs[12], IIDs[13], IIDs[14]}, "") }
        };

        islandsSI = new Dictionary<string, Island>
        {
            {IIDs[0], IslandGenerator.Generate("cairo")},
            {IIDs[1], IslandGenerator.Generate("cairo")},
            {IIDs[2], IslandGenerator.Generate("cairo")},
            {IIDs[3], IslandGenerator.Generate("cairo")},
            {IIDs[4], IslandGenerator.Generate("pimpMacD")},
            {IIDs[5], IslandGenerator.Generate("pimpMacD")},
            {IIDs[6], IslandGenerator.Generate("pimpMacD")},
            {IIDs[7], IslandGenerator.Generate("pimpMacD")},
            {IIDs[8], IslandGenerator.Generate("pimpMacD")},
            {IIDs[9], IslandGenerator.Generate("nox")},
            {IIDs[10], IslandGenerator.Generate("nox")},
            {IIDs[11], IslandGenerator.Generate("nox")},
            {IIDs[12], IslandGenerator.Generate("nox")},
            {IIDs[13], IslandGenerator.Generate("nox")},
            {IIDs[14], IslandGenerator.Generate("nox")}
        };
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            TestAll();
    }

    void TestAll()
    {
        NationUpdateTests();
        SearchIslandsTest();
        Debug.Log(GetTestResultStrings());
    }

    void NationUpdateTests()
    {
        nationResults = "";

        //Adding and Updating One Player
        processor.state = new State();
        processor.AddPlayerOrUpdateNation(playerOne, failNation);
        bool passedFirst = processor.state.players.Count == 0 && !processor.state.players.ContainsKey(playerOne);

        if (passedFirst)
            nationResults += "pass ";
        else
            nationResults += "fail ";

        processor.state = new State();
        processor.AddPlayerOrUpdateNation(playerOne, passNation);
        bool passedSecond = processor.state.players.Count == 1 && processor.state.players.ContainsKey(playerOne) && processor.state.players[playerOne].nationCode == passNation;

        if (passedSecond)
            nationResults += "pass ";
        else
            nationResults += "fail ";

        processor.AddPlayerOrUpdateNation(playerOne, updateFailNation);
        bool passedThird = processor.state.players.Count == 1 && processor.state.players.ContainsKey(playerOne) && processor.state.players[playerOne].nationCode == passNation;

        if (passedThird)
            nationResults += "pass ";
        else
            nationResults += "fail ";

        processor.AddPlayerOrUpdateNation(playerOne, updatePassNation);
        bool passedFourth = processor.state.players.Count == 1 && processor.state.players.ContainsKey(playerOne) && processor.state.players[playerOne].nationCode == updatePassNation;

        if (passedFourth)
            nationResults += "pass ";
        else
            nationResults += "fail ";
        
        //Adding and Updating a Second Player
        State savedState = processor.state;
        processor.AddPlayerOrUpdateNation(playerTwo, failNation);
        bool passedFifth = processor.state.players.Count == 1 && !processor.state.players.ContainsKey(playerTwo) 
        && processor.state.players.ContainsKey(playerOne) && processor.state.players[playerOne].nationCode == updatePassNation;

        if (passedFifth)
            nationResults += "pass ";
        else
            nationResults += "fail ";

        processor.state = savedState;
        processor.AddPlayerOrUpdateNation(playerTwo, passNation);
        bool passedSixth = processor.state.players.Count == 2 && processor.state.players.ContainsKey(playerOne) 
        && processor.state.players.ContainsKey(playerTwo) && processor.state.players[playerOne].nationCode == updatePassNation
        && processor.state.players[playerTwo].nationCode == passNation;

        if (passedSixth)
            nationResults += "pass ";
        else
            nationResults += "fail ";

        processor.AddPlayerOrUpdateNation(playerTwo, updateFailNation);
        bool passedSeventh = processor.state.players.Count == 2 && processor.state.players.ContainsKey(playerOne) 
        && processor.state.players.ContainsKey(playerTwo) && processor.state.players[playerOne].nationCode == updatePassNation
        && processor.state.players[playerTwo].nationCode == passNation;

        if (passedSeventh)
            nationResults += "pass ";
        else
            nationResults += "fail ";

        processor.AddPlayerOrUpdateNation(playerTwo, updatePassNation);
        bool passedEighth = processor.state.players.Count == 2 && processor.state.players.ContainsKey(playerOne)
        && processor.state.players.ContainsKey(playerTwo) && processor.state.players[playerOne].nationCode == updatePassNation
        && processor.state.players[playerTwo].nationCode == updatePassNation;

        if (passedEighth)
            nationResults += "pass ";
        else
            nationResults += "fail ";
    }

    void SearchIslandsTest()
    {
        searchIslandResults = "";
        processor.state = new State(playersSI, islandsSI, new Dictionary<string, ResourceContribution>(), new Dictionary<string, string>());
        Random.InitState(1337);

        //Got lucky with seed 1337, allowed me to try all of the combinations first shot with minimum calls, so don't change that.
        //Don't test any random non existent names because that is checked before it passes to this.
        //Fail to find island because found self
        processor.DiscoverOrScoutIsland("cairo", "norm", "p");
        bool passedFirst = processor.state.players["cairo"].islands.Count == 4 && processor.state.players["cairo"].attackableIsland == ""
        && processor.state.players["cairo"].resources[0] == 2000 && processor.state.players["cairo"].resources[1] == 5000
        && processor.state.players["cairo"].resources[2] == 0 && processor.state.players["cairo"].resources[3] == 0 
        && processor.state.players["pimpMacD"].islands.Count == 5 && processor.state.players["pimpMacD"].attackableIsland == ""
        && processor.state.players["pimpMacD"].resources[0] == 1000 && processor.state.players["pimpMacD"].resources[1] == 2500
        && processor.state.players["pimpMacD"].resources[2] == 0 && processor.state.players["pimpMacD"].resources[3] == 0
        && processor.state.players["nox"].islands.Count == 6 && processor.state.players["nox"].attackableIsland == ""
        && processor.state.players["nox"].resources[0] == 0 && processor.state.players["nox"].resources[1] == 0
        && processor.state.players["nox"].resources[2] == 0 && processor.state.players["nox"].resources[3] == 0
        && processor.state.islands.Count == 15 && !processor.state.islands.ContainsKey("p");

        if (passedFirst)
            searchIslandResults += "pass ";
        else
            searchIslandResults += "fail ";

        //Find attackable island
        processor.DiscoverOrScoutIsland("cairo", "norm", "p");
        bool passedSecond = processor.state.players["cairo"].islands.Count == 4 && processor.state.players["cairo"].attackableIsland == "h"
        && processor.state.players["cairo"].resources[0] == 1000 && processor.state.players["cairo"].resources[1] == 2500
        && processor.state.players["cairo"].resources[2] == 0 && processor.state.players["cairo"].resources[3] == 0
        && processor.state.players["pimpMacD"].islands.Count == 5 && processor.state.players["pimpMacD"].attackableIsland == ""
        && processor.state.players["pimpMacD"].resources[0] == 1000 && processor.state.players["pimpMacD"].resources[1] == 2500
        && processor.state.players["pimpMacD"].resources[2] == 0 && processor.state.players["pimpMacD"].resources[3] == 0
        && processor.state.players["nox"].islands.Count == 6 && processor.state.players["nox"].attackableIsland == ""
        && processor.state.players["nox"].resources[0] == 0 && processor.state.players["nox"].resources[1] == 0
        && processor.state.players["nox"].resources[2] == 0 && processor.state.players["nox"].resources[3] == 0
        && processor.state.islands.Count == 15 && !processor.state.islands.ContainsKey("p");

        if (passedSecond)
            searchIslandResults += "pass ";
        else
            searchIslandResults += "fail ";
        
        //Find new island
        processor.DiscoverOrScoutIsland("pimpMacD", "norm", "p");
        bool passedThird = processor.state.players["cairo"].islands.Count == 4 && processor.state.players["cairo"].attackableIsland == "h"
        && processor.state.players["cairo"].resources[0] == 1000 && processor.state.players["cairo"].resources[1] == 2500
        && processor.state.players["cairo"].resources[2] == 0 && processor.state.players["cairo"].resources[3] == 0
        && processor.state.players["pimpMacD"].islands.Count == 6 && processor.state.players["pimpMacD"].attackableIsland == ""
        && processor.state.players["pimpMacD"].resources[0] == 0 && processor.state.players["pimpMacD"].resources[1] == 0
        && processor.state.players["pimpMacD"].resources[2] == 0 && processor.state.players["pimpMacD"].resources[3] == 0
        && processor.state.players["pimpMacD"].islands.Contains("p") && processor.state.players["nox"].islands.Count == 6 
        && processor.state.players["nox"].attackableIsland == "" && processor.state.players["nox"].resources[0] == 0 
        && processor.state.players["nox"].resources[1] == 0 && processor.state.players["nox"].resources[2] == 0 
        && processor.state.players["nox"].resources[3] == 0 && processor.state.islands.Count == 16 && processor.state.islands.ContainsKey("p");

        if (passedThird)
            searchIslandResults += "pass ";
        else
            searchIslandResults += "fail ";

        //Fail to find island because you a broke ass
        processor.DiscoverOrScoutIsland("nox", "norm", "q");
        bool passedFourth = processor.state.players["cairo"].islands.Count == 4 && processor.state.players["cairo"].attackableIsland == "h"
        && processor.state.players["cairo"].resources[0] == 1000 && processor.state.players["cairo"].resources[1] == 2500
        && processor.state.players["cairo"].resources[2] == 0 && processor.state.players["cairo"].resources[3] == 0
        && processor.state.players["pimpMacD"].islands.Count == 6 && processor.state.players["pimpMacD"].attackableIsland == ""
        && processor.state.players["pimpMacD"].resources[0] == 0 && processor.state.players["pimpMacD"].resources[1] == 0
        && processor.state.players["pimpMacD"].resources[2] == 0 && processor.state.players["pimpMacD"].resources[3] == 0
        && processor.state.players["nox"].islands.Count == 6 && processor.state.players["nox"].attackableIsland == "" 
        && processor.state.players["nox"].resources[0] == 0 && processor.state.players["nox"].resources[1] == 0 
        && processor.state.players["nox"].resources[2] == 0 && processor.state.players["nox"].resources[3] == 0 
        && processor.state.islands.Count == 16 && !processor.state.islands.ContainsKey("q");

        if (passedFourth)
            searchIslandResults += "pass ";
        else
            searchIslandResults += "fail ";

        //Fail to find island because of bad search command
        processor.DiscoverOrScoutIsland("cairo", "non-norm", "q");
        bool passedFifth = processor.state.players["cairo"].islands.Count == 4 && processor.state.players["cairo"].attackableIsland == "h"
        && processor.state.players["cairo"].resources[0] == 1000 && processor.state.players["cairo"].resources[1] == 2500
        && processor.state.players["cairo"].resources[2] == 0 && processor.state.players["cairo"].resources[3] == 0
        && processor.state.players["pimpMacD"].islands.Count == 6 && processor.state.players["pimpMacD"].attackableIsland == ""
        && processor.state.players["pimpMacD"].resources[0] == 0 && processor.state.players["pimpMacD"].resources[1] == 0
        && processor.state.players["pimpMacD"].resources[2] == 0 && processor.state.players["pimpMacD"].resources[3] == 0
        && processor.state.players["nox"].islands.Count == 6 && processor.state.players["nox"].attackableIsland == ""
        && processor.state.players["nox"].resources[0] == 0 && processor.state.players["nox"].resources[1] == 0
        && processor.state.players["nox"].resources[2] == 0 && processor.state.players["nox"].resources[3] == 0
        && processor.state.islands.Count == 16 && !processor.state.islands.ContainsKey("q");

        if (passedFifth)
            searchIslandResults += "pass ";
        else
            searchIslandResults += "fail ";

        //Failed to find island because of already existing txid (1/1.15e+77 chances but just incase)
        processor.DiscoverOrScoutIsland("cairo", "norm", "a");
        bool passedSixth = processor.state.players["cairo"].islands.Count == 4 && processor.state.players["cairo"].attackableIsland == "h"
        && processor.state.players["cairo"].resources[0] == 1000 && processor.state.players["cairo"].resources[1] == 2500
        && processor.state.players["cairo"].resources[2] == 0 && processor.state.players["cairo"].resources[3] == 0
        && processor.state.players["pimpMacD"].islands.Count == 6 && processor.state.players["pimpMacD"].attackableIsland == ""
        && processor.state.players["pimpMacD"].resources[0] == 0 && processor.state.players["pimpMacD"].resources[1] == 0
        && processor.state.players["pimpMacD"].resources[2] == 0 && processor.state.players["pimpMacD"].resources[3] == 0
        && processor.state.players["nox"].islands.Count == 6 && processor.state.players["nox"].attackableIsland == ""
        && processor.state.players["nox"].resources[0] == 0 && processor.state.players["nox"].resources[1] == 0
        && processor.state.players["nox"].resources[2] == 0 && processor.state.players["nox"].resources[3] == 0
        && processor.state.islands.Count == 16;

        if (passedSixth)
            searchIslandResults += "pass ";
        else
            searchIslandResults += "fail ";
    }

    string GetTestResultStrings()
    {
        string entireResults = "";

        label = new string[] { "NationUpdateTests: ", "SearchIslandsTest: " };
        results = new string[] { nationResults, searchIslandResults };
        
        for (int r = 0; r < results.Length; r++)
        {
            entireResults += label[r] + results[r] + "\n";
        }

        return entireResults;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IslesOfWar.ClientSide;
using IslesOfWar.Communication;
using IslesOfWar.GameStateProcessing;
using Newtonsoft.Json;

public class GSPTesting : MonoBehaviour
{
    StateProcessor processor;

    Dictionary<string, PlayerState> players;
    Dictionary<string, Island> islands;
    string[] IIDs = new string[] {"a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o" };

    //Test Results
    string nationResults = "";
    string searchIslandResults = "";
    string purchaseUnitResults = "";
    string purchaseCollectorsResults = "";
    string[] label;
    string[] results;

    void Start()
    {
        ResetTestData();
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
        PurchaseUnitTest();
        PurchaseCollectorsTest();
        Debug.Log(GetTestResultStrings());
    }

    void NationUpdateTests()
    {
        nationResults = "";

        //Adding and Updating One Player
        //Country Code Add Fail
        processor.state = new State();
        processor.AddPlayerOrUpdateNation("cairo", "USA");
        bool passedFirst = processor.state.players.Count == 0 && !processor.state.players.ContainsKey("cairo");
        nationResults += GetPassOrFail(passedFirst);

        //Country Code Add Pass
        processor.state = new State();
        processor.AddPlayerOrUpdateNation("cairo", "US");
        bool passedSecond = processor.state.players.Count == 1 && processor.state.players.ContainsKey("cairo") && processor.state.players["cairo"].nationCode == "US";
        nationResults += GetPassOrFail(passedSecond);

        //Country Code Update Fail
        processor.AddPlayerOrUpdateNation("cairo", "zw");
        bool passedThird = processor.state.players.Count == 1 && processor.state.players.ContainsKey("cairo") && processor.state.players["cairo"].nationCode == "US";
        nationResults += GetPassOrFail(passedThird);

        //Country Code Update Pass
        processor.AddPlayerOrUpdateNation("cairo", "ZW");
        bool passedFourth = processor.state.players.Count == 1 && processor.state.players.ContainsKey("cairo") && processor.state.players["cairo"].nationCode == "ZW";
        nationResults += GetPassOrFail(passedFourth);

        //Adding and Updating a Second Player
        //Country Code Fail
        State savedState = processor.state;
        processor.AddPlayerOrUpdateNation("nox", "Mexico");
        bool passedFifth = processor.state.players.Count == 1 && !processor.state.players.ContainsKey("caz") 
        && processor.state.players.ContainsKey("cairo") && processor.state.players["cairo"].nationCode == "ZW";
        nationResults += GetPassOrFail(passedFifth);

        //Country Code Pass
        processor.state = savedState;
        processor.AddPlayerOrUpdateNation("nox", "MX");
        bool passedSixth = processor.state.players.Count == 2 && processor.state.players.ContainsKey("cairo") 
        && processor.state.players.ContainsKey("nox") && processor.state.players["cairo"].nationCode == "ZW"
        && processor.state.players["nox"].nationCode == "MX";
        nationResults += GetPassOrFail(passedSixth);

        processor.AddPlayerOrUpdateNation("nox", "Murica");
        bool passedSeventh = processor.state.players.Count == 2 && processor.state.players.ContainsKey("cairo") 
        && processor.state.players.ContainsKey("nox") && processor.state.players["cairo"].nationCode == "ZW"
        && processor.state.players["nox"].nationCode == "MX";
        nationResults += GetPassOrFail(passedSeventh);

        processor.AddPlayerOrUpdateNation("nox", "US");
        bool passedEighth = processor.state.players.Count == 2 && processor.state.players.ContainsKey("cairo")
        && processor.state.players.ContainsKey("nox") && processor.state.players["cairo"].nationCode == "ZW"
        && processor.state.players["nox"].nationCode == "US";
        nationResults += GetPassOrFail(passedEighth);
    }

    void SearchIslandsTest()
    {
        ResetTestData();
        searchIslandResults = "";
        processor.state = new State(players, islands, new Dictionary<string, ResourceContribution>(), new Dictionary<string, string>());
        UnityEngine.Random.InitState(1337);

        //Got lucky with seed 1337, allowed me to try all of the combinations first shot with minimum calls, so don't change that.
        //Don't test any random non existent names because that is checked before it passes to this.
        //Fail to find island because found self
        processor.DiscoverOrScoutIsland("cairo", "norm", "p");
        bool passedFirst = processor.state.players["cairo"].islands.Count == 4 && processor.state.players["cairo"].attackableIsland == ""
        && processor.state.players["cairo"].resources[0] == 2000 && processor.state.players["cairo"].resources[1] == 5000
        && processor.state.players["cairo"].resources[2] == 6000 && processor.state.players["cairo"].resources[3] == 1500 
        && processor.state.players["pimpMacD"].islands.Count == 5 && processor.state.players["pimpMacD"].attackableIsland == ""
        && processor.state.players["pimpMacD"].resources[0] == 1000 && processor.state.players["pimpMacD"].resources[1] == 2500
        && processor.state.players["pimpMacD"].resources[2] == 2000 && processor.state.players["pimpMacD"].resources[3] == 500
        && processor.state.players["nox"].islands.Count == 6 && processor.state.players["nox"].attackableIsland == ""
        && processor.state.players["nox"].resources[0] == 0 && processor.state.players["nox"].resources[1] == 0
        && processor.state.players["nox"].resources[2] == 0 && processor.state.players["nox"].resources[3] == 0
        && processor.state.islands.Count == 15 && !processor.state.islands.ContainsKey("p");
        searchIslandResults += GetPassOrFail(passedFirst);

        //Find attackable island
        processor.DiscoverOrScoutIsland("cairo", "norm", "p");
        bool passedSecond = processor.state.players["cairo"].islands.Count == 4 && processor.state.players["cairo"].attackableIsland == "h"
        && processor.state.players["cairo"].resources[0] == 1000 && processor.state.players["cairo"].resources[1] == 2500
        && processor.state.players["cairo"].resources[2] == 6000 && processor.state.players["cairo"].resources[3] == 1500
        && processor.state.players["pimpMacD"].islands.Count == 5 && processor.state.players["pimpMacD"].attackableIsland == ""
        && processor.state.players["pimpMacD"].resources[0] == 1000 && processor.state.players["pimpMacD"].resources[1] == 2500
        && processor.state.players["pimpMacD"].resources[2] == 2000 && processor.state.players["pimpMacD"].resources[3] == 500
        && processor.state.players["nox"].islands.Count == 6 && processor.state.players["nox"].attackableIsland == ""
        && processor.state.players["nox"].resources[0] == 0 && processor.state.players["nox"].resources[1] == 0
        && processor.state.players["nox"].resources[2] == 0 && processor.state.players["nox"].resources[3] == 0
        && processor.state.islands.Count == 15 && !processor.state.islands.ContainsKey("p");
        searchIslandResults += GetPassOrFail(passedSecond);

        //Find new island
        processor.DiscoverOrScoutIsland("pimpMacD", "norm", "p");
        bool passedThird = processor.state.players["cairo"].islands.Count == 4 && processor.state.players["cairo"].attackableIsland == "h"
        && processor.state.players["cairo"].resources[0] == 1000 && processor.state.players["cairo"].resources[1] == 2500
        && processor.state.players["cairo"].resources[2] == 6000 && processor.state.players["cairo"].resources[3] == 1500
        && processor.state.players["pimpMacD"].islands.Count == 6 && processor.state.players["pimpMacD"].attackableIsland == ""
        && processor.state.players["pimpMacD"].resources[0] == 0 && processor.state.players["pimpMacD"].resources[1] == 0
        && processor.state.players["pimpMacD"].resources[2] == 2000 && processor.state.players["pimpMacD"].resources[3] == 500
        && processor.state.players["pimpMacD"].islands.Contains("p") && processor.state.players["nox"].islands.Count == 6 
        && processor.state.players["nox"].attackableIsland == "" && processor.state.players["nox"].resources[0] == 0 
        && processor.state.players["nox"].resources[1] == 0 && processor.state.players["nox"].resources[2] == 0 
        && processor.state.players["nox"].resources[3] == 0 && processor.state.islands.Count == 16 && processor.state.islands.ContainsKey("p");
        searchIslandResults += GetPassOrFail(passedThird);

        //Fail to find island because you a broke ass
        processor.DiscoverOrScoutIsland("nox", "norm", "q");
        bool passedFourth = processor.state.players["cairo"].islands.Count == 4 && processor.state.players["cairo"].attackableIsland == "h"
        && processor.state.players["cairo"].resources[0] == 1000 && processor.state.players["cairo"].resources[1] == 2500
        && processor.state.players["cairo"].resources[2] == 6000 && processor.state.players["cairo"].resources[3] == 1500
        && processor.state.players["pimpMacD"].islands.Count == 6 && processor.state.players["pimpMacD"].attackableIsland == ""
        && processor.state.players["pimpMacD"].resources[0] == 0 && processor.state.players["pimpMacD"].resources[1] == 0
        && processor.state.players["pimpMacD"].resources[2] == 2000 && processor.state.players["pimpMacD"].resources[3] == 500
        && processor.state.players["nox"].islands.Count == 6 && processor.state.players["nox"].attackableIsland == "" 
        && processor.state.players["nox"].resources[0] == 0 && processor.state.players["nox"].resources[1] == 0 
        && processor.state.players["nox"].resources[2] == 0 && processor.state.players["nox"].resources[3] == 0 
        && processor.state.islands.Count == 16 && !processor.state.islands.ContainsKey("q");
        searchIslandResults += GetPassOrFail(passedFourth);

        //Fail to find island because of bad search command
        processor.DiscoverOrScoutIsland("cairo", "non-norm", "q");
        bool passedFifth = processor.state.players["cairo"].islands.Count == 4 && processor.state.players["cairo"].attackableIsland == "h"
        && processor.state.players["cairo"].resources[0] == 1000 && processor.state.players["cairo"].resources[1] == 2500
        && processor.state.players["cairo"].resources[2] == 6000 && processor.state.players["cairo"].resources[3] == 1500
        && processor.state.players["pimpMacD"].islands.Count == 6 && processor.state.players["pimpMacD"].attackableIsland == ""
        && processor.state.players["pimpMacD"].resources[0] == 0 && processor.state.players["pimpMacD"].resources[1] == 0
        && processor.state.players["pimpMacD"].resources[2] == 2000 && processor.state.players["pimpMacD"].resources[3] == 500
        && processor.state.players["nox"].islands.Count == 6 && processor.state.players["nox"].attackableIsland == ""
        && processor.state.players["nox"].resources[0] == 0 && processor.state.players["nox"].resources[1] == 0
        && processor.state.players["nox"].resources[2] == 0 && processor.state.players["nox"].resources[3] == 0
        && processor.state.islands.Count == 16 && !processor.state.islands.ContainsKey("q");
        searchIslandResults += GetPassOrFail(passedFifth);

        //Failed to find island because of already existing txid (1/1.15e+77 chances but just incase)
        processor.DiscoverOrScoutIsland("cairo", "norm", "a");
        bool passedSixth = processor.state.players["cairo"].islands.Count == 4 && processor.state.players["cairo"].attackableIsland == "h"
        && processor.state.players["cairo"].resources[0] == 1000 && processor.state.players["cairo"].resources[1] == 2500
        && processor.state.players["cairo"].resources[2] == 6000 && processor.state.players["cairo"].resources[3] == 1500
        && processor.state.players["pimpMacD"].islands.Count == 6 && processor.state.players["pimpMacD"].attackableIsland == ""
        && processor.state.players["pimpMacD"].resources[0] == 0 && processor.state.players["pimpMacD"].resources[1] == 0
        && processor.state.players["pimpMacD"].resources[2] == 2000 && processor.state.players["pimpMacD"].resources[3] == 500
        && processor.state.players["nox"].islands.Count == 6 && processor.state.players["nox"].attackableIsland == ""
        && processor.state.players["nox"].resources[0] == 0 && processor.state.players["nox"].resources[1] == 0
        && processor.state.players["nox"].resources[2] == 0 && processor.state.players["nox"].resources[3] == 0
        && processor.state.islands.Count == 16;
        searchIslandResults += GetPassOrFail(passedSixth); 
    }

    void PurchaseUnitTest()
    {
        ResetTestData();
        purchaseUnitResults = "";
        processor.state = new State(players, islands, new Dictionary<string, ResourceContribution>(), new Dictionary<string, string>());
        long[][] playerResources = new long[][] { new long[4], new long[4], new long[4] };
        Array.Copy(processor.state.players["cairo"].allResources, playerResources[0], 4);
        Array.Copy(processor.state.players["pimpMacD"].allResources, playerResources[1], 4);
        Array.Copy(processor.state.players["nox"].allResources, playerResources[2], 4);
        long[][] playerUnits = new long[][] {new long[9], new long[9], new long[9] };
        Array.Copy(processor.state.players["cairo"].allUnits, playerUnits[0], 9);
        Array.Copy(processor.state.players["pimpMacD"].allUnits, playerUnits[1], 9);
        Array.Copy(processor.state.players["nox"].allUnits, playerUnits[2], 9);

        //Fail to purchase because array size.
        processor.PurchaseUnits("cairo", new List<int>() { 1, 1, 1 });
        bool passedFirst = IsEqual(processor.state.players["cairo"].allUnits, playerUnits[0])
        && IsEqual(processor.state.players["pimpMacD"].allUnits, playerUnits[1])
        && IsEqual(processor.state.players["nox"].allUnits, playerUnits[2])
        && IsEqual(processor.state.players["cairo"].allResources, playerResources[0])
        && IsEqual(processor.state.players["pimpMacD"].allResources, playerResources[1])
        && IsEqual(processor.state.players["nox"].allResources, playerResources[2]);
        purchaseUnitResults += GetPassOrFail(passedFirst);

        //Fail to purchase because not enough of each resource.
        processor.PurchaseUnits("nox", new List<int>() { 1, 1, 1, 1, 1, 1, 1, 1, 1 });
        bool passedSecond = IsEqual(processor.state.players["cairo"].allUnits, playerUnits[0])
        && IsEqual(processor.state.players["pimpMacD"].allUnits, playerUnits[1])
        && IsEqual(processor.state.players["nox"].allUnits, playerUnits[2])
        && IsEqual(processor.state.players["cairo"].allResources, playerResources[0])
        && IsEqual(processor.state.players["pimpMacD"].allResources, playerResources[1])
        && IsEqual(processor.state.players["nox"].allResources, playerResources[2]);
        purchaseUnitResults += GetPassOrFail(passedSecond);

        //Fail to purchase because not enough of one resource.
        processor.PurchaseUnits("pimpMacD", new List<int>() { 101, 0, 0, 0, 0, 0, 0, 0, 0 });
        bool passedThird = IsEqual(processor.state.players["cairo"].allUnits, playerUnits[0])
        && IsEqual(processor.state.players["pimpMacD"].allUnits, playerUnits[1])
        && IsEqual(processor.state.players["nox"].allUnits, playerUnits[2])
        && IsEqual(processor.state.players["cairo"].allResources, playerResources[0])
        && IsEqual(processor.state.players["pimpMacD"].allResources, playerResources[1])
        && IsEqual(processor.state.players["nox"].allResources, playerResources[2]);
        purchaseUnitResults += GetPassOrFail(passedThird);

        //Successful purchase of one unit type.
        processor.PurchaseUnits("pimpMacD", new List<int>() { 100, 0, 0, 0, 0, 0, 0, 0, 0 });
        bool passedFourth = IsEqual(processor.state.players["cairo"].allUnits, playerUnits[0])
        && IsEqual(processor.state.players["pimpMacD"].allUnits, new long[] { 150, 25, 12, 5, 2, 1, 5, 2, 1 })
        && IsEqual(processor.state.players["nox"].allUnits, playerUnits[2])
        && IsEqual(processor.state.players["cairo"].allResources, playerResources[0])
        && IsEqual(processor.state.players["pimpMacD"].allResources, new long[] {0, 2500, 1000, 500 })
        && IsEqual(processor.state.players["nox"].allResources, playerResources[2]);
        purchaseUnitResults += GetPassOrFail(passedFourth);

        //Save state again
        Array.Copy(processor.state.players["cairo"].allUnits, playerUnits[0], 9);
        Array.Copy(processor.state.players["pimpMacD"].allUnits, playerUnits[1], 9);
        Array.Copy(processor.state.players["nox"].allUnits, playerUnits[2], 9);
        Array.Copy(processor.state.players["cairo"].allResources, playerResources[0], 4);
        Array.Copy(processor.state.players["pimpMacD"].allResources, playerResources[1], 4);
        Array.Copy(processor.state.players["nox"].allResources, playerResources[2], 4);

        //Successful purchase of multiple units
        //Total purchase based on current unit price should be 2710, 1035, 1175, 0
        processor.PurchaseUnits("cairo", new List<int>() { 1, 1, 1, 1, 1, 1, 1, 1, 1 });
        bool passedFifth = IsEqual(processor.state.players["cairo"].allUnits, new long[] { 1, 1, 1, 1, 1, 1, 1, 1, 1 })
        && IsEqual(processor.state.players["pimpMacD"].allUnits, playerUnits[1])
        && IsEqual(processor.state.players["nox"].allUnits, playerUnits[2])
        && IsEqual(processor.state.players["cairo"].allResources, new long[] { 290, 6465, 4825, 1500 })
        && IsEqual(processor.state.players["pimpMacD"].allResources, playerResources[1])
        && IsEqual(processor.state.players["nox"].allResources, playerResources[2]);
        purchaseUnitResults += GetPassOrFail(passedFifth);

    }

    //Has some fail testing of defenses in the beginning of this function because I didn't realize how big the DevelopIsland function testing
    //was going to be. So I just left them and stopped at the end of the collector testing. The remainder of defense testing will be in a 
    //different function.
    void PurchaseCollectorsTest()
    {
        ResetTestData();
        purchaseCollectorsResults = "";
        processor.state = new State(players, islands, new Dictionary<string, ResourceContribution>(), new Dictionary<string, string>());

        long[][] playerResources = new long[][] { new long[4], new long[4], new long[4] };
        Array.Copy(processor.state.players["cairo"].allResources, playerResources[0], 4);
        Array.Copy(processor.state.players["pimpMacD"].allResources, playerResources[1], 4);
        Array.Copy(processor.state.players["nox"].allResources, playerResources[2], 4);
        Dictionary<string, Island> savedIslands = JsonConvert.DeserializeObject<Dictionary<string,Island>>(JsonConvert.SerializeObject(processor.state.islands));

        //No need to test if IslandBuildOrder is null or player exists
        //because the callback checks that before it pushes data to this function.
        //Fail because no ID.
        processor.DevelopIsland("cairo", new IslandBuildOrder(null, "100000000000", "))))))))))))"));
        bool passedFirst = IsEqual(processor.state.players["cairo"].allResources, playerResources[0]) 
        && IslandsAreEqual(savedIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands);
        purchaseCollectorsResults += GetPassOrFail(passedFirst);

        //Fail because non-existent ID
        processor.DevelopIsland("cairo", new IslandBuildOrder("z", "100000000000", "))))))))))))"));
        bool passedSecond = IsEqual(processor.state.players["cairo"].allResources, playerResources[0]) 
        && IslandsAreEqual(savedIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands);
        purchaseCollectorsResults += GetPassOrFail(passedSecond);

        //Fail because defenses and collectors are null
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, null));
        bool passedThird = IsEqual(processor.state.players["cairo"].allResources, playerResources[0]) 
        && IslandsAreEqual(savedIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands);
        purchaseCollectorsResults += GetPassOrFail(passedThird);

        //Fail because island is not owned by player
        processor.DevelopIsland("cairo", new IslandBuildOrder("o", null, null));
        bool passedFourth = IsEqual(processor.state.players["cairo"].allResources, playerResources[0]) 
        && IslandsAreEqual(savedIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands);
        purchaseCollectorsResults += GetPassOrFail(passedFourth);

        //Fail because collectors are not long enough. #1
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "10", null));
        bool passedFifth = IsEqual(processor.state.players["cairo"].allResources, playerResources[0]) 
        && IslandsAreEqual(savedIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands);
        purchaseCollectorsResults += GetPassOrFail(passedFifth);

        //Fail because defenses are not long enough. #1
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null , "!))"));
        bool passedSixth = IsEqual(processor.state.players["cairo"].allResources, playerResources[0]) 
        && IslandsAreEqual(savedIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands);
        purchaseCollectorsResults += GetPassOrFail(passedSixth);

        //Fail because collectors are not long enough. #2
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "10", "))))))))))))"));
        bool passedSeventh = IsEqual(processor.state.players["cairo"].allResources, playerResources[0]) 
        && IslandsAreEqual(savedIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands);
        purchaseCollectorsResults += GetPassOrFail(passedSeventh);

        //Fail because defenses are not long enough. #2
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "000000000000", "!))"));
        bool passedEighth = IsEqual(processor.state.players["cairo"].allResources, playerResources[0]) 
        && IslandsAreEqual(savedIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands);
        purchaseCollectorsResults += GetPassOrFail(passedEighth);

        //Succeed in development of oil collector on first tile with null defense
        Dictionary<string, Island> alteredIslands = JsonConvert.DeserializeObject<Dictionary<string, Island>>(JsonConvert.SerializeObject(savedIslands));
        alteredIslands["a"].collectors = "100000000000";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "100000000000", null));
        bool passedNinth = IsEqual(processor.state.players["cairo"].allResources, new long[] {1500, 7000, 5000, 500 })
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands);
        purchaseCollectorsResults += GetPassOrFail(passedNinth);

        //Succeed in development of metal collector on first tile with null defense
        ResetTestData();
        processor.state = new State(players, islands, new Dictionary<string, ResourceContribution>(), new Dictionary<string, string>());
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "200000000000", null));
        alteredIslands["a"].collectors = "200000000000";
        bool passedTenth = IsEqual(processor.state.players["cairo"].allResources, new long[] { 1500, 6500, 5500, 500 })
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands);
        purchaseCollectorsResults += GetPassOrFail(passedTenth);

        //Succeed in development of limestone collector on first tile with null defense
        ResetTestData();
        processor.state = new State(players, islands, new Dictionary<string, ResourceContribution>(), new Dictionary<string, string>());
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "300000000000", null));
        alteredIslands["a"].collectors = "300000000000";
        bool passedEleventh = IsEqual(processor.state.players["cairo"].allResources, new long[] { 1500, 6500, 5000, 1000 })
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands);
        purchaseCollectorsResults += GetPassOrFail(passedEleventh);

        //Succeed in development of oil and metal collectors on first tile with null defense
        ResetTestData();
        processor.state = new State(players, islands, new Dictionary<string, ResourceContribution>(), new Dictionary<string, string>());
        processor.state.players["cairo"].resources[3] = 2000;
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "400000000000", null));
        alteredIslands["a"].collectors = "400000000000";
        bool passedTwelfth = IsEqual(processor.state.players["cairo"].allResources, new long[] { 0, 6000, 4500, 0 })
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands);
        purchaseCollectorsResults += GetPassOrFail(passedTwelfth);

        //Succeed in development of oil and limestone collectors on first tile with null defense
        ResetTestData();
        processor.state = new State(players, islands, new Dictionary<string, ResourceContribution>(), new Dictionary<string, string>());
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "500000000000", null));
        alteredIslands["a"].collectors = "500000000000";
        bool passedThirteenth = IsEqual(processor.state.players["cairo"].allResources, new long[] { 0, 6000, 4000, 0 })
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands);
        purchaseCollectorsResults += GetPassOrFail(passedThirteenth);

        //Succeed in development of metal and limestone collectors on first tile with null defense
        ResetTestData();
        processor.state = new State(players, islands, new Dictionary<string, ResourceContribution>(), new Dictionary<string, string>());
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "600000000000", null));
        alteredIslands["a"].collectors = "600000000000";
        bool passedFourteenth = IsEqual(processor.state.players["cairo"].allResources, new long[] { 0, 5500, 4500, 0 })
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands);
        purchaseCollectorsResults += GetPassOrFail(passedFourteenth);

        //Succeed in development of all collectors on first tile with null defense
        ResetTestData();
        processor.state = new State(players, islands, new Dictionary<string, ResourceContribution>(), new Dictionary<string, string>());
        processor.state.players["cairo"].resources = new List<long>() { 4500, 2500, 2500, 2500 };
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "700000000000", null));
        alteredIslands["a"].collectors = "700000000000";
        bool passedFifteenth = IsEqual(processor.state.players["cairo"].allResources, new long[] { 0, 0, 0, 0 })
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands);
        purchaseCollectorsResults += GetPassOrFail(passedFifteenth);

        //Succeed in development of single collectors on multiple different tiles of different types.
        ResetTestData();
        processor.state = new State(players, islands, new Dictionary<string, ResourceContribution>(), new Dictionary<string, string>());
        processor.state.players["cairo"].resources = new List<long>() { 4500, 2500, 2000, 3000 };
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "100000000022", null));
        alteredIslands["a"].collectors = "100000000022";
        bool passedSixteenth = IsEqual(processor.state.players["cairo"].allResources, new long[] { 0, 0, 0, 0 })
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands);
        purchaseCollectorsResults += GetPassOrFail(passedSixteenth);
    }

    void ResetTestData()
    {
        //Initialize Random Seed to get the same islands everytime.
        //Changing this seed will break a lot of the tests.
        UnityEngine.Random.InitState(1337);
        processor = new StateProcessor();

        players = new Dictionary<string, PlayerState>
        {
            {"cairo", new PlayerState("US", new long[9], new long[] {3000, 7500, 6000, 1500}, new string[] {IIDs[0], IIDs[1], IIDs[2], IIDs[3]}, "") },
            {"pimpMacD", new PlayerState("US", new long[] {50, 25, 12, 5, 2, 1, 5, 2, 1 }, new long[] {1000, 2500, 2000, 500}, new string[] { IIDs[4], IIDs[5], IIDs[6], IIDs[7], IIDs[8]}, "") },
            {"nox", new PlayerState("MX", new long[] {100, 50, 25, 10, 5, 2, 10, 5, 1 }, new long[] {0, 0, 0, 0}, new string[] { IIDs[9], IIDs[10], IIDs[11], IIDs[12], IIDs[13], IIDs[14]}, "") }
        };

        islands = new Dictionary<string, Island>
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

    string GetPassOrFail(bool pass)
    {
        if (pass)
            return "O ";
        else
            return "X ";
    }

    bool IslandFeaturesWereNotAltered(Dictionary<string, Island> a, Dictionary<string, Island> b)
    {
        bool notAltered = true;

        foreach (KeyValuePair<string, Island> pair in b)
        {
            notAltered = notAltered && pair.Value.features == a[pair.Key].features;
        }

        return notAltered;
    }

    bool IslandsAreEqual(Dictionary<string, Island> a, Dictionary<string, Island> b)
    {
        bool equal = a.Count == b.Count;

        foreach (KeyValuePair<string, Island> pair in b)
        {
            equal = equal && a.ContainsKey(pair.Key);

            if (equal)
                equal = equal && a[pair.Key].collectors == b[pair.Key].collectors
                && a[pair.Key].defenses == b[pair.Key].defenses;
        }

        return equal;
    }

    bool IsEqual(long[] a, long[] b)
    {
        bool equal = a.Length == b.Length;

        for (int i = 0; i < a.Length && equal; i++)
        {
            equal = a[i] == b[i];
        }

        return equal;
    }

    string GetTestResultStrings()
    {
        string entireResults = "-Test Results -        O=Pass X=Fail \n";

        label = new string[] { "NationUpdateTests: ", "SearchIslandsTest: ", "PurchaseUnitsTest: ", "CollectorPurchase: " };
        results = new string[] { nationResults, searchIslandResults, purchaseUnitResults, purchaseCollectorsResults };
        
        for (int r = 0; r < results.Length; r++)
        {
            entireResults += label[r] + results[r] + "\n";
        }

        return entireResults;
    }
}

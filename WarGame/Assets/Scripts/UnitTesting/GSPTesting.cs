using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IslesOfWar;
using IslesOfWar.ClientSide;
using IslesOfWar.Communication;
using IslesOfWar.GameStateProcessing;
using Newtonsoft.Json;

public class GSPTesting : MonoBehaviour
{
    StateProcessor processor;

    Dictionary<string, PlayerState> players;
    Dictionary<string, Island> islands;
    string[] IIDs = new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o" };

    //Test Results
    string nationResults = "";
    string searchIslandResults = "";
    string purchaseUnitResults = "";
    string purchaseCollectorsResults = "";
    string purchaseBunkerResults = "";
    string purchaseBlockerResults = "";
    string purchaseBlockerAndBunkerResults = "";
    string defenseUpdateResults = "";
    string[] label;
    string[] results;
    int[,] colCost = Constants.collectorCosts;

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
        PurchaseBunkersTests();
        BlockerPurchaseTest();
        BunkerAndBlockerPurchaseTest();
        UpdateDefensesTests();
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
        long[][] playerResources = new long[][] { new long[4], new long[4], new long[4] };
        Array.Copy(processor.state.players["cairo"].allResources, playerResources[0], 4);
        Array.Copy(processor.state.players["pimpMacD"].allResources, playerResources[1], 4);
        Array.Copy(processor.state.players["nox"].allResources, playerResources[2], 4);
        long[][] playerUnits = new long[][] { new long[9], new long[9], new long[9] };
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
        && IsEqual(processor.state.players["pimpMacD"].allResources, new long[] { 0, 2500, 1000, 500 })
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

        long[][] playerResources = new long[][] { new long[4], new long[4], new long[4] };
        Array.Copy(processor.state.players["cairo"].allResources, playerResources[0], 4);
        Array.Copy(processor.state.players["pimpMacD"].allResources, playerResources[1], 4);
        Array.Copy(processor.state.players["nox"].allResources, playerResources[2], 4);
        Dictionary<string, Island> savedIslands = JsonConvert.DeserializeObject<Dictionary<string, Island>>(JsonConvert.SerializeObject(processor.state.islands));

        //No need to test if IslandBuildOrder is null or player exists
        //because the callback checks that before it pushes data to this function.
        //Fail because no ID.
        processor.DevelopIsland("cairo", new IslandBuildOrder(null, "100000000000", "))))))))))))"));
        bool passedFirst = IsEqual(processor.state.players["cairo"].allResources, playerResources[0])
        && IslandsAreEqual(savedIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands)
        && PlayersAreEqualExcept("", players, processor.state.players);
        purchaseCollectorsResults += GetPassOrFail(passedFirst);

        //Fail because non-existent ID
        processor.DevelopIsland("cairo", new IslandBuildOrder("z", "100000000000", "))))))))))))"));
        bool passedSecond = IsEqual(processor.state.players["cairo"].allResources, playerResources[0])
        && IslandsAreEqual(savedIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands)
        && PlayersAreEqualExcept("", players, processor.state.players);
        purchaseCollectorsResults += GetPassOrFail(passedSecond);

        //Fail because defenses and collectors are null
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, null));
        bool passedThird = IsEqual(processor.state.players["cairo"].allResources, playerResources[0])
        && IslandsAreEqual(savedIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands)
        && PlayersAreEqualExcept("", players, processor.state.players);
        purchaseCollectorsResults += GetPassOrFail(passedThird);

        //Fail because island is not owned by player
        processor.DevelopIsland("cairo", new IslandBuildOrder("o", "100000000000", null));
        bool passedFourth = IsEqual(processor.state.players["cairo"].allResources, playerResources[0])
        && IslandsAreEqual(savedIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands)
        && PlayersAreEqualExcept("", players, processor.state.players);
        purchaseCollectorsResults += GetPassOrFail(passedFourth);

        //Fail because collectors are not long enough. #1
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "10", null));
        bool passedFifth = IsEqual(processor.state.players["cairo"].allResources, playerResources[0])
        && IslandsAreEqual(savedIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands)
        && PlayersAreEqualExcept("", players, processor.state.players);
        purchaseCollectorsResults += GetPassOrFail(passedFifth);

        //Sixth Moved To Defenses as First

        //Fail because collectors are not long enough. #2
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "10", "))))))))))))"));
        bool passedSeventh = IsEqual(processor.state.players["cairo"].allResources, playerResources[0])
        && IslandsAreEqual(savedIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands)
        && PlayersAreEqualExcept("", players, processor.state.players);
        purchaseCollectorsResults += GetPassOrFail(passedSeventh);

        //Eight Moved to Defenses as Second

        //Succeed in development of oil collector on first tile with null defense
        Dictionary<string, Island> alteredIslands = JsonConvert.DeserializeObject<Dictionary<string, Island>>(JsonConvert.SerializeObject(savedIslands));
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfCollectors(new int[] { 1, 0, 0 }));
        alteredIslands["a"].collectors = "100000000000";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "100000000000", null));
        bool passedNinth = IsEqual(processor.state.players["cairo"].allResources, new long[] { 0, 0, 0, 0 })
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseCollectorsResults += GetPassOrFail(passedNinth);

        //Succeed in development of metal collector on first tile with null defense
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfCollectors(new int[] { 0, 1, 0 }));
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "200000000000", null));
        alteredIslands["a"].collectors = "200000000000";
        bool passedTenth = IsEqual(processor.state.players["cairo"].allResources, new long[] { 0, 0, 0, 0 })
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands);
        purchaseCollectorsResults += GetPassOrFail(passedTenth);

        //Succeed in development of limestone collector on first tile with null defense
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfCollectors(new int[] { 0, 0, 1 }));
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "300000000000", null));
        alteredIslands["a"].collectors = "300000000000";
        bool passedEleventh = IsEqual(processor.state.players["cairo"].allResources, new long[] { 0, 0, 0, 0 })
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseCollectorsResults += GetPassOrFail(passedEleventh);

        //Succeed in development of oil and metal collectors on first tile with null defense
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfCollectors(new int[] { 1, 1, 0 }));
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "400000000000", null));
        alteredIslands["a"].collectors = "400000000000";
        bool passedTwelfth = IsEqual(processor.state.players["cairo"].allResources, new long[] { 0, 0, 0, 0 })
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseCollectorsResults += GetPassOrFail(passedTwelfth);

        //Succeed in development of oil and limestone collectors on first tile with null defense
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfCollectors(new int[] { 1, 0, 1 }));
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "500000000000", null));
        alteredIslands["a"].collectors = "500000000000";
        bool passedThirteenth = IsEqual(processor.state.players["cairo"].allResources, new long[] { 0, 0, 0, 0 })
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseCollectorsResults += GetPassOrFail(passedThirteenth);

        //Succeed in development of metal and limestone collectors on first tile with null defense
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfCollectors(new int[] { 0, 1, 1 }));
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "600000000000", null));
        alteredIslands["a"].collectors = "600000000000";
        bool passedFourteenth = IsEqual(processor.state.players["cairo"].allResources, new long[] { 0, 0, 0, 0 })
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseCollectorsResults += GetPassOrFail(passedFourteenth);

        //Succeed in development of all collectors on first tile with null defense
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfCollectors(new int[] { 1, 1, 1 }));
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "700000000000", null));
        alteredIslands["a"].collectors = "700000000000";
        bool passedFifteenth = IsEqual(processor.state.players["cairo"].allResources, new long[] { 0, 0, 0, 0 })
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseCollectorsResults += GetPassOrFail(passedFifteenth);

        //Succeed in development of single collectors on multiple different tiles of different types.
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfCollectors(new int[] {2,1,0 }));
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "100000000012", null));
        alteredIslands["a"].collectors = "100000000012";
        bool passedSixteenth = IsEqual(processor.state.players["cairo"].allResources, new long[] { 0, 0, 0, 0 })
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseCollectorsResults += GetPassOrFail(passedSixteenth);

        //Fail in development with collectors on multiple tiles where at least one is incorrect. (Sneaky Bastard Almost Got Through)
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfCollectors(new int[] { 1, 2, 0 }));
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "100000000022", null));
        bool passedSeventeenth = IsEqual(processor.state.players["cairo"].allResources, GetCostOfCollectors(new int[] { 1, 2, 0 }))
        && IslandsAreEqual(savedIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseCollectorsResults += GetPassOrFail(passedSeventeenth);

        //Succeed in development on lower case letter. 
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfCollectors(new int[] { 0, 1, 1 }));
        processor.DevelopIsland("cairo", new IslandBuildOrder("d", "000000000060", null));
        alteredIslands["a"].collectors = "000000000000";
        alteredIslands["d"].collectors = "000000000060";
        bool passedEighteenth = IsEqual(processor.state.players["cairo"].allResources, new long[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseCollectorsResults += GetPassOrFail(passedEighteenth);

        //Fail in development with collectors on multiple tiles where at least one is invalid but invalid case is multi collector request.
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfCollectors(new int[] { 1, 2, 0 }));
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "100000000042", null));
        bool passedNineteenth = IsEqual(processor.state.players["cairo"].allResources, GetCostOfCollectors(new int[] { 1, 2, 0 }))
        && IslandsAreEqual(savedIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseCollectorsResults += GetPassOrFail(passedNineteenth);

        //Succeed in development via overwriting with additional collector of different type.
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfCollectors(new int[] { 0, 1, 0 }));
        processor.state.islands["a"].collectors = "100000000000";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "200000000000", null));
        alteredIslands["a"].collectors = "400000000000";
        alteredIslands["d"].collectors = "000000000000";
        bool passedTwentieth = IsEqual(processor.state.players["cairo"].allResources, new long[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseCollectorsResults += GetPassOrFail(passedTwentieth);

        //Fail in development because collector already exists.
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfCollectors(new int[] { 1, 0, 0 }));
        processor.state.islands["a"].collectors = "100000000000";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "100000000000", null));
        alteredIslands["a"].collectors = "100000000000";
        bool passedTwentyFirst = IsEqual(processor.state.players["cairo"].allResources, GetCostOfCollectors(new int[] { 1, 0, 0 }))
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseCollectorsResults += GetPassOrFail(passedTwentyFirst);

        //Fail in development with a multi collector request where one collector of a requested type exists already.
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfCollectors(new int[] { 1, 0, 0 }));
        processor.state.islands["a"].collectors = "100000000000";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "400000000000", null));
        bool passedTwentySecond = IsEqual(processor.state.players["cairo"].allResources, GetCostOfCollectors(new int[] { 1, 0, 0 }))
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseCollectorsResults += GetPassOrFail(passedTwentySecond);

        //Fail because malformed data
        ResetTestData();
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "0000000|0000", null));
        bool passedTwentyThird = IsEqual(processor.state.players["cairo"].allResources, playerResources[0])
        && IslandsAreEqual(savedIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands)
        && PlayersAreEqualExcept("", players, processor.state.players);
        purchaseCollectorsResults += GetPassOrFail(passedTwentyThird);

        //Fail because malformed data but some are correct
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfCollectors(new int[] { 1, 0, 0 }));
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "1000000|0000", null));
        bool passedTwentyFourth = IsEqual(processor.state.players["cairo"].allResources, GetCostOfCollectors(new int[] { 1, 0, 0 }))
        && IslandsAreEqual(savedIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseCollectorsResults += GetPassOrFail(passedTwentyFourth);
    }

    void PurchaseBunkersTests()
    {
        ResetTestData();
        purchaseBunkerResults = "";

        long[][] playerResources = new long[][] { new long[4], new long[4], new long[4] };
        Array.Copy(processor.state.players["cairo"].allResources, playerResources[0], 4);
        Array.Copy(processor.state.players["pimpMacD"].allResources, playerResources[1], 4);
        Array.Copy(processor.state.players["nox"].allResources, playerResources[2], 4);
        Dictionary<string, Island> savedIslands = JsonConvert.DeserializeObject<Dictionary<string, Island>>(JsonConvert.SerializeObject(processor.state.islands));
        Dictionary<string, Island> alteredIslands = JsonConvert.DeserializeObject<Dictionary<string, Island>>(JsonConvert.SerializeObject(processor.state.islands));

        //Fail because defenses are not long enough. #1
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "!))"));
        bool passedFirst = IslandsAreEqual(savedIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands)
        && PlayersAreEqualExcept("", players, processor.state.players);
        purchaseBunkerResults += GetPassOrFail(passedFirst);

        //Fail because defenses are not long enough. #2
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "000000000000", "!))"));
        bool passedSecond = IslandsAreEqual(savedIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands)
        && PlayersAreEqualExcept("", players, processor.state.players);
        purchaseBunkerResults += GetPassOrFail(passedSecond);

        //Succeed in development of troop bunker.
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfBunkers(new int[] { 1, 0, 0 }));
        alteredIslands["a"].defenses = "!)))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "!)))))))))))"));
        bool passedThird = IsEqual(processor.state.players["cairo"].allResources, new long[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBunkerResults += GetPassOrFail(passedThird);

        //Succeed in development of tank bunker.
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfBunkers(new int[] { 0, 1, 0 }));
        alteredIslands["a"].defenses = "@)))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "@)))))))))))"));
        bool passedFourth = IsEqual(processor.state.players["cairo"].allResources, new long[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBunkerResults += GetPassOrFail(passedThird);

        //Succeed in development of air bunker.
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfBunkers(new int[] { 0, 0, 1 }));
        alteredIslands["a"].defenses = "#)))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "#)))))))))))"));
        bool passedFifth = IsEqual(processor.state.players["cairo"].allResources, new long[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBunkerResults += GetPassOrFail(passedFifth);

        //Succeed in development of troop and tank bunker.
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfBunkers(new int[] { 1, 1, 0 }));
        alteredIslands["a"].defenses = "$)))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "$)))))))))))"));
        bool passedSixth = IsEqual(processor.state.players["cairo"].allResources, new long[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBunkerResults += GetPassOrFail(passedSixth);

        //Succeed in development of troop and air bunker.
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfBunkers( new int[] { 1, 0, 1 }));
        alteredIslands["a"].defenses = "%)))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "%)))))))))))"));
        bool passedSeventh = IsEqual(processor.state.players["cairo"].allResources, new long[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBunkerResults += GetPassOrFail(passedSeventh);

        //Succeed in development of tank and air bunker.
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfBunkers(new int[] { 0, 1, 1 }));
        alteredIslands["a"].defenses = "^)))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "^)))))))))))"));
        bool passedEigth = IsEqual(processor.state.players["cairo"].allResources, new long[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBunkerResults += GetPassOrFail(passedEigth);

        //Fail in development of troop, tank and air bunker.
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfBunkers( new int[] { 1, 1, 1 }));
        alteredIslands["a"].defenses = "))))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "&)))))))))))"));
        bool passedNinth = IsEqual(processor.state.players["cairo"].allResources, GetCostOfBunkers(new int[] { 1, 1, 1 }))
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBunkerResults += GetPassOrFail(passedNinth);

        //Succeed in development of multiple single defenses on multiple tiles.
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfBunkers(new int[] { 1, 1, 1 }));
        alteredIslands["a"].defenses = "!))))@))))#)";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "!))))@))))#)"));
        bool passedTenth = IsEqual(processor.state.players["cairo"].allResources, new long[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBunkerResults += GetPassOrFail(passedTenth);

        //Succeed in development of multiple single and multi defenses on multiple tiles.
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfBunkers(new int[] { 3, 1, 1 }));
        alteredIslands["a"].defenses = "!))))$))))%)";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "!))))$))))%)"));
        bool passedEleventh = IsEqual(processor.state.players["cairo"].allResources, new long[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBunkerResults += GetPassOrFail(passedEleventh);

        //Fail in development of multiple single defenses on multiple tiles because one exists already on a tile.
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfBunkers(new int[] { 2, 0, 1 }));
        alteredIslands["a"].defenses = ")))))!))))))";
        processor.state.islands["a"].defenses = ")))))!))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "!))))!))))#)"));
        bool passedTwelfth = IsEqual(processor.state.players["cairo"].allResources, GetCostOfBunkers(new int[] { 2, 0, 1 }))
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBunkerResults += GetPassOrFail(passedTwelfth);

        //Fail in development of multiple single and multi defenses on multiple tiles because one exists already on a tile.
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfBunkers(new int[] { 3, 1, 1 }));
        alteredIslands["a"].defenses = ")))))!))))))";
        processor.state.islands["a"].defenses = ")))))!))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "!))))$))))%)"));
        bool passedThirteenth = IsEqual(processor.state.players["cairo"].allResources, GetCostOfBunkers(new int[] { 3, 1, 1 }))
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBunkerResults += GetPassOrFail(passedThirteenth);
    }

    void BlockerPurchaseTest()
    {
        ResetTestData();
        purchaseBlockerResults = "";
        Dictionary<string, Island> savedIslands = JsonConvert.DeserializeObject<Dictionary<string, Island>>(JsonConvert.SerializeObject(processor.state.islands));
        Dictionary<string, Island> alteredIslands = JsonConvert.DeserializeObject<Dictionary<string, Island>>(JsonConvert.SerializeObject(processor.state.islands));

        //Fail in development of blocker because incorrect length. #1
        alteredIslands["a"].defenses = "))))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "0))"));
        bool passedFirst = IslandsAreEqual(savedIslands, processor.state.islands)
        && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands) && PlayersAreEqualExcept("", players, processor.state.players);
        purchaseBlockerResults += GetPassOrFail(passedFirst);

        //Fail in development of blocker because incorrect length. #2
        ResetTestData();
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "000000000000", "0))"));
        bool passedSecond = IslandsAreEqual(savedIslands, processor.state.islands)
        && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands) && PlayersAreEqualExcept("", players, processor.state.players);
        purchaseBlockerResults += GetPassOrFail(passedSecond);

        //Succeed in development of troop blocker
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfBlockers(new int[] { 1, 0, 0 }));
        alteredIslands["a"].defenses = "0)))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "0)))))))))))"));
        bool passedThird = IsEqual(processor.state.players["cairo"].allResources, new long[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerResults += GetPassOrFail(passedThird);

        //Succeed in development of tank blocker
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfBlockers(new int[] { 0, 1, 0 }));
        alteredIslands["a"].defenses = "a)))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "a)))))))))))"));
        bool passedFourth = IsEqual(processor.state.players["cairo"].allResources, new long[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerResults += GetPassOrFail(passedFourth);

        //Succeed in development of air blocker
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfBlockers( new int[] { 0, 0, 1 }));
        alteredIslands["a"].defenses = "A)))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "A)))))))))))"));
        bool passedFifth = IsEqual(processor.state.players["cairo"].allResources, new long[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerResults += GetPassOrFail(passedFifth);

        //Succeed in development of multiple blockers on multiple tiles
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfBlockers(new int[] { 1, 1, 2 }));
        alteredIslands["a"].defenses = "A)))0)))a))A";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "A)))0)))a))A"));
        bool passedSixth = IsEqual(processor.state.players["cairo"].allResources, new long[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerResults += GetPassOrFail(passedSixth);

        //Fail in development of multiple blockers because not enough resources
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfBlockers(new int[] { 0, 0, 1 }));
        alteredIslands["a"].defenses = "))))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "A))))))))0aA"));
        bool passedSeventh = IsEqual(processor.state.players["cairo"].allResources, GetCostOfBlockers(new int[] { 0, 0, 1 }))
        && IslandsAreEqual(savedIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerResults += GetPassOrFail(passedSeventh);

        //Fail in development of tank blocker because troop blocker already exists.
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfBlockers(new int[] { 0, 1, 0 }));
        processor.state.islands["a"].defenses = "0)))))))))))";
        alteredIslands["a"].defenses = "0)))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "a)))))))))))"));
        bool passedEighth = IsEqual(processor.state.players["cairo"].allResources, GetCostOfBlockers(new int[] { 0, 1, 0 }))
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerResults += GetPassOrFail(passedEighth);

        //Fail in development of air blocker because troop blocker already exists.
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfBlockers(new int[] { 0, 0, 1 }));
        processor.state.islands["a"].defenses = "0)))))))))))";
        alteredIslands["a"].defenses = "0)))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "A)))))))))))"));
        bool passedNinth = IsEqual(processor.state.players["cairo"].allResources, GetCostOfBlockers(new int[] { 0, 0, 1 }))
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerResults += GetPassOrFail(passedNinth);

        //Fail in development of troop blocker because air blocker already exists.
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfBlockers(new int[] { 1, 0, 0 }));
        processor.state.islands["a"].defenses = "A)))))))))))";
        alteredIslands["a"].defenses = "A)))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "0)))))))))))"));
        bool passedTenth = IsEqual(processor.state.players["cairo"].allResources, GetCostOfBlockers(new int[] { 1, 0, 0}))
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerResults += GetPassOrFail(passedTenth);
    }

    void BunkerAndBlockerPurchaseTest()
    {
        ResetTestData();
        purchaseBlockerAndBunkerResults = "";

        Dictionary<string, Island> savedIslands = JsonConvert.DeserializeObject<Dictionary<string, Island>>(JsonConvert.SerializeObject(processor.state.islands));
        Dictionary<string, Island> alteredIslands = JsonConvert.DeserializeObject<Dictionary<string, Island>>(JsonConvert.SerializeObject(processor.state.islands));
        
        //Succeed building troop blocker and troop bunker
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(Add(GetCostOfBlockers(new int[] { 1, 0, 0 }), GetCostOfBunkers(new int[] { 1, 0, 0 })));
        alteredIslands["a"].defenses = "1)))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "1)))))))))))"));
        bool passedFirst = IsEqual(processor.state.players["cairo"].allResources, new long[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerAndBunkerResults += GetPassOrFail(passedFirst);

        //Succeed building troop blocker and tank bunker
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(Add(GetCostOfBlockers(new int[] { 1, 0, 0 }), GetCostOfBunkers(new int[] { 0, 1, 0 })));
        alteredIslands["a"].defenses = "2)))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "2)))))))))))"));
        bool passedSecond = IsEqual(processor.state.players["cairo"].allResources, new long[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerAndBunkerResults += GetPassOrFail(passedSecond);

        //Succeed building troop blocker and air bunker
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(Add(GetCostOfBlockers(new int[] { 1, 0, 0 }), GetCostOfBunkers(new int[] { 0, 0, 1 })));
        alteredIslands["a"].defenses = "3)))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "3)))))))))))"));
        bool passedThird = IsEqual(processor.state.players["cairo"].allResources, new long[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerAndBunkerResults += GetPassOrFail(passedThird);

        //Succeed building troop blocker and troop tank bunker combo
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(Add(GetCostOfBlockers(new int[] { 1, 0, 0 }), GetCostOfBunkers(new int[] { 1, 1, 0 })));
        alteredIslands["a"].defenses = "4)))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "4)))))))))))"));
        bool passedFourth = IsEqual(processor.state.players["cairo"].allResources, new long[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerAndBunkerResults += GetPassOrFail(passedFourth);

        //Succeed building troop blocker and troop air bunker combo
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(Add(GetCostOfBlockers(new int[] { 1, 0, 0 }), GetCostOfBunkers(new int[] { 1, 0, 1 })));
        alteredIslands["a"].defenses = "5)))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "5)))))))))))"));
        bool passedFifth = IsEqual(processor.state.players["cairo"].allResources, new long[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerAndBunkerResults += GetPassOrFail(passedFifth);

        //Succeed building troop blocker and tank air bunker combo
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(Add(GetCostOfBlockers(new int[] { 1, 0, 0 }), GetCostOfBunkers(new int[] { 0, 1, 1 })));
        alteredIslands["a"].defenses = "6)))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "6)))))))))))"));
        bool passedSixth = IsEqual(processor.state.players["cairo"].allResources, new long[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerAndBunkerResults += GetPassOrFail(passedSixth);

        //Succeed building tank blocker and tank air bunker combo
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(Add(GetCostOfBlockers(new int[] { 0, 1, 0 }), GetCostOfBunkers(new int[] { 0, 1, 1 })));
        alteredIslands["a"].defenses = "g)))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "g)))))))))))"));
        bool passedSeventh = IsEqual(processor.state.players["cairo"].allResources, new long[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerAndBunkerResults += GetPassOrFail(passedSeventh);

        //Succeed building air blocker and tank air bunker combo
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(Add(GetCostOfBlockers(new int[] { 0, 0, 1 }), GetCostOfBunkers(new int[] { 0, 1, 1 })));
        alteredIslands["a"].defenses = "G)))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "G)))))))))))"));
        bool passedEigth = IsEqual(processor.state.players["cairo"].allResources, new long[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerAndBunkerResults += GetPassOrFail(passedEigth);

        //Succeed building multiple bunkers and blockers on multiple tiles
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(Add(GetCostOfBlockers(new int[] { 3, 3, 3 }), GetCostOfBunkers(new int[] { 4, 4, 4 })));
        alteredIslands["a"].defenses = ")01bcDE5gG))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, ")01bcDE5gG))"));
        bool passedNinth = IsEqual(processor.state.players["cairo"].allResources, new long[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerAndBunkerResults += GetPassOrFail(passedNinth);

        //Fail building multiple bunkers and blockers on multiple tiles because not enough resources
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfBlockers(new int[] { 1, 0, 0 }));
        alteredIslands["a"].defenses = "))))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "0))))@))))))"));
        bool passedTenth = IsEqual(processor.state.players["cairo"].allResources, GetCostOfBlockers(new int[] { 1, 0, 0 }))
        && IslandsAreEqual(savedIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerAndBunkerResults += GetPassOrFail(passedTenth);

        //Fail building multiple bunkers and blockers on multiple tiles because bad data
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(Add(GetCostOfBlockers(new int[] { 2, 0, 0 }), GetCostOfBunkers(new int[] { 1, 0, 0 })));
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "0))))@))7)))"));
        bool passedEleventh = IsEqual(processor.state.players["cairo"].allResources, Add(GetCostOfBlockers(new int[] { 2, 0, 0 }), GetCostOfBunkers(new int[] { 1, 0, 0 })))
        && IslandsAreEqual(savedIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerAndBunkerResults += GetPassOrFail(passedEleventh);

        //Succeed building new bunkers on tiles with blocker on it.
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange( GetCostOfBunkers(new int[] { 1, 1, 1 }));
        processor.state.islands["a"].defenses = "0))))A)))))a";
        alteredIslands["a"].defenses = "1))))C)))))d";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "!))))@)))))#"));
        bool passedTwelfth = IsEqual(processor.state.players["cairo"].allResources, new long[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerAndBunkerResults += GetPassOrFail(passedTwelfth);

        //Succeed building new blocker on tiles with bunkers on it.
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfBlockers(new int[] { 1, 1, 1 }));
        processor.state.islands["a"].defenses = "!))))@)))))#";
        alteredIslands["a"].defenses = "1))))C)))))d";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "0))))A)))))a"));
        bool passedThriteenth = IsEqual(processor.state.players["cairo"].allResources, new long[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerAndBunkerResults += GetPassOrFail(passedThriteenth);

        //Fail building new bunkers on tiles with blocker on it because not enough resources
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfBunkers(new int[] { 1, 1, 0 }));
        processor.state.islands["a"].defenses = "0))))A)))))a";
        alteredIslands["a"].defenses = "0))))A)))))a";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "!))))@)))))#"));
        bool passedFourteenth = IsEqual(processor.state.players["cairo"].allResources, GetCostOfBunkers(new int[] { 1, 1, 0 }))
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerAndBunkerResults += GetPassOrFail(passedFourteenth);

        //Fail building new bunkers on tiles with blocker on it because bunker already there
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfBunkers(new int[] { 1, 1, 1 }));
        processor.state.islands["a"].defenses = "1))))A)))))a";
        alteredIslands["a"].defenses = "1))))A)))))a";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "!))))@)))))#"));
        bool passedFifteenth = IsEqual(processor.state.players["cairo"].allResources, GetCostOfBunkers(new int[] { 1, 1, 1 }))
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerAndBunkerResults += GetPassOrFail(passedFifteenth);

        //Fail building new blocker on tiles with bunkers on it because not enough resources
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfBlockers(new int[] { 1, 1, 0 }));
        processor.state.islands["a"].defenses = "!))))@)))))#";
        alteredIslands["a"].defenses = "!))))@)))))#";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "0))))A)))))a"));
        bool passedSixteenth = IsEqual(processor.state.players["cairo"].allResources, GetCostOfBlockers(new int[] { 1, 1, 0 }))
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerAndBunkerResults += GetPassOrFail(passedSixteenth);

        //Fail building new blocker on tiles with bunkers on it because blocker exists
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfBlockers(new int[] { 1, 1, 1}));
        processor.state.islands["a"].defenses = "0))))@)))))#";
        alteredIslands["a"].defenses = "0))))@)))))#";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "0))))A)))))a"));
        bool passedSeventeenth = IsEqual(processor.state.players["cairo"].allResources, GetCostOfBlockers(new int[] { 1, 1, 1 }))
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerAndBunkerResults += GetPassOrFail(passedSeventeenth);
    }

    void UpdateDefensesTests()
    {
        ResetTestData();
        defenseUpdateResults = "";

        processor.state.players["cairo"].units = new List<long>() { 0, 2, 4, 6, 8, 10, 12, 14, 16 };
        players["cairo"].units = new List<long>() { 0, 2, 4, 6, 8, 10, 12, 14, 16 };
        Dictionary<string, Island> savedIslands = JsonConvert.DeserializeObject<Dictionary<string, Island>>(JsonConvert.SerializeObject(processor.state.islands));
        int[][] pln = new int[][] { new int[] { 0, 1, 4 }, new int[] { 3, 5, 6, 7 } };
        int[][] sqd = new int[][] { new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 } };
        int[][] bigPlan = new int[][] { new int[] { 0, 1, 4 }, new int[] { 3, 2, 6, 7 }, new int[] { 8, 4, 9 }, new int[] { 11, 10, 6, 7 }, new int[] { 5, 1, 2, 6, 10, 9, 4 } };
        int[][] bigSquad = new int[][] { new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 } };


        //Fail because id is null
        processor.UpdateDefensePlan(null, new BattleCommand("cairo", pln, sqd));
        bool passedFirst = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(savedIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedFirst);

        //Fail because pln is null
        processor.UpdateDefensePlan("a", new BattleCommand("cairo", null, sqd));
        bool passedSecond = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(savedIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedSecond);

        //Fail because sqd is null
        processor.UpdateDefensePlan("a", new BattleCommand("cairo", pln, null));
        bool passedThird = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(savedIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedThird);

        //Fail because id does not exist
        processor.UpdateDefensePlan("z", new BattleCommand("cairo", pln, sqd));
        bool passedFourth = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(savedIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedFourth);

        //Fail because id does not belong to player
        processor.UpdateDefensePlan("o", new BattleCommand("cairo", pln, sqd));
        bool passedFifth = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(savedIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedFifth);

        //Fail because pln length != sqd length
        processor.UpdateDefensePlan("a", new BattleCommand("cairo", new int[][] { new int[] { 0, 1, 4 } }, sqd));
        bool passedSixth = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(savedIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedSixth);

        //Fail because pln length > 4
        processor.state.players["cairo"].units = new List<long>() { 0, 5, 10, 15, 20, 25, 30, 35, 40 };
        players["cairo"].units = new List<long>() { 0, 5, 10, 15, 20, 25, 30, 35, 40 };
        processor.UpdateDefensePlan("a", new BattleCommand("cairo", bigPlan, bigSquad));
        bool passedSeventh = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(savedIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedSeventh);

        //Fail because sqd length != 9
        processor.state.players["cairo"].units = new List<long>() { 0, 2, 4, 6, 8, 10, 12, 14, 16 };
        players["cairo"].units = new List<long>() { 0, 2, 4, 6, 8, 10, 12, 14, 16 };
        int[][] oddSquad = new int[][] { new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 } };
        processor.UpdateDefensePlan("a", new BattleCommand("cairo", pln, oddSquad));
        bool passedEigth = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(savedIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedEigth);



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

        processor.state = new State(players, islands, new Dictionary<string, ResourceContribution>(), new Dictionary<string, string>());
    }

    long[] Add(int[] a, int[] b)
    {
        long[] c = new long[a.Length];

        for (int i = 0; i < c.Length; i++)
        {
            c[i] = a[i] + b[i];
        }

        return c;
    }

    long[] Add(long[] a, long[] b)
    {
        long[] c = new long[a.Length];

        for (int i = 0; i < c.Length; i++)
        {
            c[i] = a[i] + b[i];
        }

        return c;
    }

    long[] GetCostOfBunkers(int[] bunkerCounts)
    {
        long[] cost = new long[4];

        for (int b = 0; b < bunkerCounts.Length; b++)
        {
            cost[0] += bunkerCounts[b] * Constants.bunkerCosts[b, 0];
            cost[1] += bunkerCounts[b] * Constants.bunkerCosts[b, 1];
            cost[2] += bunkerCounts[b] * Constants.bunkerCosts[b, 2];
            cost[3] += bunkerCounts[b] * Constants.bunkerCosts[b, 3];
        }

        return cost;
    }

    long[] GetCostOfBlockers(int[] blockerCounts)
    {
        long[] cost = new long[4];

        for (int b = 0; b < blockerCounts.Length; b++)
        {
            cost[0] += blockerCounts[b] * Constants.blockerCosts[b, 0];
            cost[1] += blockerCounts[b] * Constants.blockerCosts[b, 1];
            cost[2] += blockerCounts[b] * Constants.blockerCosts[b, 2];
            cost[3] += blockerCounts[b] * Constants.blockerCosts[b, 3];
        }

        return cost;
    }

    long[] GetCostOfCollectors(int[] typeCount)
    {
        long[] finalPrice = new long[4];

        for (int i = 0; i < typeCount.Length; i++)
        {
            finalPrice[0] += typeCount[i] * Constants.collectorCosts[i, 0];
            finalPrice[1] += typeCount[i] * Constants.collectorCosts[i, 1];
            finalPrice[2] += typeCount[i] * Constants.collectorCosts[i, 2];
            finalPrice[3] += typeCount[i] * Constants.collectorCosts[i, 3];
        }

        return finalPrice;
    }

    string GetPassOrFail(bool pass)
    {
        if (pass)
            return "O ";
        else
            return "X ";
    }

    //Need this just in case adding collectors to an island modified the features and IslandsAreEqual is excluding a player.
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
                && a[pair.Key].defenses == b[pair.Key].defenses && pair.Value.features == a[pair.Key].features;
        }

        return equal;
    }

    bool PlayersAreEqualExcept(string player, Dictionary<string, PlayerState> affected, Dictionary<string, PlayerState> saved)
    {
        bool areEqual = true;

        foreach (KeyValuePair<string, PlayerState> pair in affected)
        {
            if (pair.Key != player)
            {
                areEqual = areEqual && IsEqual(pair.Value.allResources, saved[pair.Key].allResources);
                areEqual = areEqual && IsEqual(pair.Value.allUnits, saved[pair.Key].allUnits);
                areEqual = areEqual && IsEqual(pair.Value.allIslands, saved[pair.Key].allIslands);
            }
        }

        return areEqual;
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

    bool IsEqual(string[] a, string[] b)
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
                                                                                                                           
        label = new string[] { "NationUpdate      : ", "SearchIslands     : ", "PurchaseUnits     : ", "BuildCollectors    : ", "BuildBunkers      : ",
        "BuildBlocker       : ", "BuildBlonkers     : ", "UpdateDefenders: " };
        results = new string[] { nationResults, searchIslandResults, purchaseUnitResults, purchaseCollectorsResults, purchaseBunkerResults,
        purchaseBlockerResults, purchaseBlockerAndBunkerResults, defenseUpdateResults };
        
        for (int r = 0; r < results.Length; r++)
        {
            entireResults += label[r] + results[r] + "\n";
        }

        return entireResults;
    }
}

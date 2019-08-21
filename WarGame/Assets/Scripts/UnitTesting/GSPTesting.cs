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
    string resourceUpdateResults = "";
    string depletedIslandResults = "";
    string resourcePoolResults = "";
    string transferResourcesResults = "";
    string rewardDepletedResults = "";
    string rewardResourcePoolResults = "";
    string attackTestingResults = "";
    string terrainTestingResults = "";
    string[] label;
    string[] results;
    double[,] colCost = Constants.collectorCosts;
    int passCount;
    int failCount;

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
        passCount = 0;
        failCount = 0;

        //If changing this order messes up tests, it's likely the random state that needs to be reset.
        NationUpdateTests();
        SearchIslandsTest();
        PurchaseUnitTest();
        PurchaseCollectorsTest();
        PurchaseBunkersTests();
        BlockerPurchaseTest();
        BunkerAndBlockerPurchaseTest();
        UpdateDefensesTests();
        IncrementResourcesTest();
        TestDepletedIslandSubmissions();
        TestReourcePoolSubmission();
        ResourceTransferTest();
        AttackCycleTesting();
        MovementBlockTesting();
        Debug.Log(GetTestResultStrings());
    }

    void NationUpdateTests()
    {
        nationResults = "";

        //Adding and Updating One Player
        //Country Code Add Fail
        processor.state = new State();
        processor.state.Init();
        processor.AddPlayerOrUpdateNation("cairo", "USA");
        bool passedFirst = processor.state.players.Count == 0 && !processor.state.players.ContainsKey("cairo");
        nationResults += GetPassOrFail(passedFirst);

        //Country Code Add Pass
        processor.state = new State();
        processor.state.Init();
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
        double[][] playerResources = new double[][] { new double[4], new double[4], new double[4] };
        Array.Copy(processor.state.players["cairo"].allResources, playerResources[0], 4);
        Array.Copy(processor.state.players["pimpMacD"].allResources, playerResources[1], 4);
        Array.Copy(processor.state.players["nox"].allResources, playerResources[2], 4);
        double[][] playerUnits = new double[][] { new double[9], new double[9], new double[9] };
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
        && IsEqual(processor.state.players["pimpMacD"].allUnits, new double[] { 150, 25, 12, 5, 2, 1, 5, 2, 1 })
        && IsEqual(processor.state.players["nox"].allUnits, playerUnits[2])
        && IsEqual(processor.state.players["cairo"].allResources, playerResources[0])
        && IsEqual(processor.state.players["pimpMacD"].allResources, new double[] { 0, 2500, 1000, 500 })
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
        bool passedFifth = IsEqual(processor.state.players["cairo"].allUnits, new double[] { 1, 1, 1, 1, 1, 1, 1, 1, 1 })
        && IsEqual(processor.state.players["pimpMacD"].allUnits, playerUnits[1])
        && IsEqual(processor.state.players["nox"].allUnits, playerUnits[2])
        && IsEqual(processor.state.players["cairo"].allResources, new double[] { 290, 6465, 4825, 1500 })
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

        double[][] playerResources = new double[][] { new double[4], new double[4], new double[4] };
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
        bool passedNinth = IsEqual(processor.state.players["cairo"].allResources, new double[] { 0, 0, 0, 0 })
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseCollectorsResults += GetPassOrFail(passedNinth);

        //Succeed in development of metal collector on first tile with null defense
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfCollectors(new int[] { 0, 1, 0 }));
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "200000000000", null));
        alteredIslands["a"].collectors = "200000000000";
        bool passedTenth = IsEqual(processor.state.players["cairo"].allResources, new double[] { 0, 0, 0, 0 })
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands);
        purchaseCollectorsResults += GetPassOrFail(passedTenth);

        //Succeed in development of limestone collector on first tile with null defense
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfCollectors(new int[] { 0, 0, 1 }));
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "300000000000", null));
        alteredIslands["a"].collectors = "300000000000";
        bool passedEleventh = IsEqual(processor.state.players["cairo"].allResources, new double[] { 0, 0, 0, 0 })
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseCollectorsResults += GetPassOrFail(passedEleventh);

        //Succeed in development of oil and metal collectors on first tile with null defense
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfCollectors(new int[] { 1, 1, 0 }));
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "400000000000", null));
        alteredIslands["a"].collectors = "400000000000";
        bool passedTwelfth = IsEqual(processor.state.players["cairo"].allResources, new double[] { 0, 0, 0, 0 })
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseCollectorsResults += GetPassOrFail(passedTwelfth);

        //Succeed in development of oil and limestone collectors on first tile with null defense
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfCollectors(new int[] { 1, 0, 1 }));
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "500000000000", null));
        alteredIslands["a"].collectors = "500000000000";
        bool passedThirteenth = IsEqual(processor.state.players["cairo"].allResources, new double[] { 0, 0, 0, 0 })
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseCollectorsResults += GetPassOrFail(passedThirteenth);

        //Succeed in development of metal and limestone collectors on first tile with null defense
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfCollectors(new int[] { 0, 1, 1 }));
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "600000000000", null));
        alteredIslands["a"].collectors = "600000000000";
        bool passedFourteenth = IsEqual(processor.state.players["cairo"].allResources, new double[] { 0, 0, 0, 0 })
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseCollectorsResults += GetPassOrFail(passedFourteenth);

        //Succeed in development of all collectors on first tile with null defense
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfCollectors(new int[] { 1, 1, 1 }));
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "700000000000", null));
        alteredIslands["a"].collectors = "700000000000";
        bool passedFifteenth = IsEqual(processor.state.players["cairo"].allResources, new double[] { 0, 0, 0, 0 })
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(savedIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseCollectorsResults += GetPassOrFail(passedFifteenth);

        //Succeed in development of single collectors on multiple different tiles of different types.
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfCollectors(new int[] {2,1,0 }));
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "100000000012", null));
        alteredIslands["a"].collectors = "100000000012";
        bool passedSixteenth = IsEqual(processor.state.players["cairo"].allResources, new double[] { 0, 0, 0, 0 })
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
        bool passedEighteenth = IsEqual(processor.state.players["cairo"].allResources, new double[4])
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
        bool passedTwentieth = IsEqual(processor.state.players["cairo"].allResources, new double[4])
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

        double[][] playerResources = new double[][] { new double[4], new double[4], new double[4] };
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
        bool passedThird = IsEqual(processor.state.players["cairo"].allResources, new double[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBunkerResults += GetPassOrFail(passedThird);

        //Succeed in development of tank bunker.
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfBunkers(new int[] { 0, 1, 0 }));
        alteredIslands["a"].defenses = "@)))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "@)))))))))))"));
        bool passedFourth = IsEqual(processor.state.players["cairo"].allResources, new double[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBunkerResults += GetPassOrFail(passedThird);

        //Succeed in development of air bunker.
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfBunkers(new int[] { 0, 0, 1 }));
        alteredIslands["a"].defenses = "#)))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "#)))))))))))"));
        bool passedFifth = IsEqual(processor.state.players["cairo"].allResources, new double[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBunkerResults += GetPassOrFail(passedFifth);

        //Succeed in development of troop and tank bunker.
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfBunkers(new int[] { 1, 1, 0 }));
        alteredIslands["a"].defenses = "$)))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "$)))))))))))"));
        bool passedSixth = IsEqual(processor.state.players["cairo"].allResources, new double[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBunkerResults += GetPassOrFail(passedSixth);

        //Succeed in development of troop and air bunker.
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfBunkers( new int[] { 1, 0, 1 }));
        alteredIslands["a"].defenses = "%)))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "%)))))))))))"));
        bool passedSeventh = IsEqual(processor.state.players["cairo"].allResources, new double[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBunkerResults += GetPassOrFail(passedSeventh);

        //Succeed in development of tank and air bunker.
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfBunkers(new int[] { 0, 1, 1 }));
        alteredIslands["a"].defenses = "^)))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "^)))))))))))"));
        bool passedEigth = IsEqual(processor.state.players["cairo"].allResources, new double[4])
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
        bool passedTenth = IsEqual(processor.state.players["cairo"].allResources, new double[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBunkerResults += GetPassOrFail(passedTenth);

        //Succeed in development of multiple single and multi defenses on multiple tiles.
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfBunkers(new int[] { 3, 1, 1 }));
        alteredIslands["a"].defenses = "!))))$))))%)";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "!))))$))))%)"));
        bool passedEleventh = IsEqual(processor.state.players["cairo"].allResources, new double[4])
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
        bool passedThird = IsEqual(processor.state.players["cairo"].allResources, new double[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerResults += GetPassOrFail(passedThird);

        //Succeed in development of tank blocker
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfBlockers(new int[] { 0, 1, 0 }));
        alteredIslands["a"].defenses = "a)))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "a)))))))))))"));
        bool passedFourth = IsEqual(processor.state.players["cairo"].allResources, new double[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerResults += GetPassOrFail(passedFourth);

        //Succeed in development of air blocker
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfBlockers( new int[] { 0, 0, 1 }));
        alteredIslands["a"].defenses = "A)))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "A)))))))))))"));
        bool passedFifth = IsEqual(processor.state.players["cairo"].allResources, new double[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerResults += GetPassOrFail(passedFifth);

        //Succeed in development of multiple blockers on multiple tiles
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(GetCostOfBlockers(new int[] { 1, 1, 2 }));
        alteredIslands["a"].defenses = "A)))0)))a))A";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "A)))0)))a))A"));
        bool passedSixth = IsEqual(processor.state.players["cairo"].allResources, new double[4])
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
        bool passedFirst = IsEqual(processor.state.players["cairo"].allResources, new double[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerAndBunkerResults += GetPassOrFail(passedFirst);

        //Succeed building troop blocker and tank bunker
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(Add(GetCostOfBlockers(new int[] { 1, 0, 0 }), GetCostOfBunkers(new int[] { 0, 1, 0 })));
        alteredIslands["a"].defenses = "2)))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "2)))))))))))"));
        bool passedSecond = IsEqual(processor.state.players["cairo"].allResources, new double[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerAndBunkerResults += GetPassOrFail(passedSecond);

        //Succeed building troop blocker and air bunker
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(Add(GetCostOfBlockers(new int[] { 1, 0, 0 }), GetCostOfBunkers(new int[] { 0, 0, 1 })));
        alteredIslands["a"].defenses = "3)))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "3)))))))))))"));
        bool passedThird = IsEqual(processor.state.players["cairo"].allResources, new double[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerAndBunkerResults += GetPassOrFail(passedThird);

        //Succeed building troop blocker and troop tank bunker combo
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(Add(GetCostOfBlockers(new int[] { 1, 0, 0 }), GetCostOfBunkers(new int[] { 1, 1, 0 })));
        alteredIslands["a"].defenses = "4)))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "4)))))))))))"));
        bool passedFourth = IsEqual(processor.state.players["cairo"].allResources, new double[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerAndBunkerResults += GetPassOrFail(passedFourth);

        //Succeed building troop blocker and troop air bunker combo
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(Add(GetCostOfBlockers(new int[] { 1, 0, 0 }), GetCostOfBunkers(new int[] { 1, 0, 1 })));
        alteredIslands["a"].defenses = "5)))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "5)))))))))))"));
        bool passedFifth = IsEqual(processor.state.players["cairo"].allResources, new double[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerAndBunkerResults += GetPassOrFail(passedFifth);

        //Succeed building troop blocker and tank air bunker combo
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(Add(GetCostOfBlockers(new int[] { 1, 0, 0 }), GetCostOfBunkers(new int[] { 0, 1, 1 })));
        alteredIslands["a"].defenses = "6)))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "6)))))))))))"));
        bool passedSixth = IsEqual(processor.state.players["cairo"].allResources, new double[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerAndBunkerResults += GetPassOrFail(passedSixth);

        //Succeed building tank blocker and tank air bunker combo
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(Add(GetCostOfBlockers(new int[] { 0, 1, 0 }), GetCostOfBunkers(new int[] { 0, 1, 1 })));
        alteredIslands["a"].defenses = "g)))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "g)))))))))))"));
        bool passedSeventh = IsEqual(processor.state.players["cairo"].allResources, new double[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerAndBunkerResults += GetPassOrFail(passedSeventh);

        //Succeed building air blocker and tank air bunker combo
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(Add(GetCostOfBlockers(new int[] { 0, 0, 1 }), GetCostOfBunkers(new int[] { 0, 1, 1 })));
        alteredIslands["a"].defenses = "G)))))))))))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, "G)))))))))))"));
        bool passedEigth = IsEqual(processor.state.players["cairo"].allResources, new double[4])
        && IslandsAreEqual(alteredIslands, processor.state.islands) && IslandFeaturesWereNotAltered(alteredIslands, processor.state.islands)
        && PlayersAreEqualExcept("cairo", players, processor.state.players);
        purchaseBlockerAndBunkerResults += GetPassOrFail(passedEigth);

        //Succeed building multiple bunkers and blockers on multiple tiles
        ResetTestData();
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["cairo"].resources.AddRange(Add(GetCostOfBlockers(new int[] { 3, 3, 3 }), GetCostOfBunkers(new int[] { 4, 4, 4 })));
        alteredIslands["a"].defenses = ")01bcDE5gG))";
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", null, ")01bcDE5gG))"));
        bool passedNinth = IsEqual(processor.state.players["cairo"].allResources, new double[4])
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
        bool passedTwelfth = IsEqual(processor.state.players["cairo"].allResources, new double[4])
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
        bool passedThriteenth = IsEqual(processor.state.players["cairo"].allResources, new double[4])
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

        processor.state.players["cairo"].units = new List<double>() { 0, 2, 4, 6, 8, 10, 12, 14, 16 };
        players["cairo"].units = new List<double>() { 0, 2, 4, 6, 8, 10, 12, 14, 16 };
        Dictionary<string, Island> savedIslands = JsonConvert.DeserializeObject<Dictionary<string, Island>>(JsonConvert.SerializeObject(processor.state.islands));
        Dictionary<string, Island> alteredIslands = JsonConvert.DeserializeObject<Dictionary<string, Island>>(JsonConvert.SerializeObject(processor.state.islands));
        int[][] pln = new int[][] { new int[] { 0, 1, 4 }, new int[] { 3, 2, 6, 7 } };
        int[][] sqd = new int[][] { new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 } };
        int[][] maxPlan = new int[][] { new int[] { 0, 1, 4 }, new int[] { 3, 2, 6, 7 }, new int[] { 8, 4, 9 }, new int[] { 11, 10, 6, 7 } };
        int[][] maxSquad = new int[][] { new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 } };
        int[][] bigPlan = new int[][] { new int[] { 0, 1, 4 }, new int[] { 3, 2, 6, 7 }, new int[] { 8, 4, 9 }, new int[] { 11, 10, 6, 7 }, new int[] { 5, 1, 2, 6, 10, 9, 4 } };
        int[][] bigSquad = new int[][] { new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 } };


        //Fail because id is null
        processor.UpdateDefensePlan("cairo", new BattleCommand(null , pln, sqd));
        bool passedFirst = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(savedIslands, processor.state.islands)
        && DefensePlansAreEqual(savedIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedFirst);

        //Fail because pln is null
        processor.UpdateDefensePlan("cairo", new BattleCommand("a", null, sqd));
        bool passedSecond = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(savedIslands, processor.state.islands)
        && DefensePlansAreEqual(savedIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedSecond);

        //Fail because sqd is null
        processor.UpdateDefensePlan("cairo", new BattleCommand("a", pln, null));
        bool passedThird = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(savedIslands, processor.state.islands)
        && DefensePlansAreEqual(savedIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedThird);

        //Fail because id does not exist
        processor.UpdateDefensePlan("cairo", new BattleCommand("z", pln, sqd));
        bool passedFourth = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(savedIslands, processor.state.islands)
        && DefensePlansAreEqual(savedIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedFourth);

        //Fail because id does not belong to player
        processor.UpdateDefensePlan("cairo", new BattleCommand("o", pln, sqd));
        bool passedFifth = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(savedIslands, processor.state.islands)
        && DefensePlansAreEqual(savedIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedFifth);

        //Fail because pln length != sqd length
        processor.UpdateDefensePlan("cairo", new BattleCommand("a", new int[][] { new int[] { 0, 1, 4 } }, sqd));
        bool passedSixth = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(savedIslands, processor.state.islands)
        && DefensePlansAreEqual(savedIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedSixth);

        //Fail because pln length > 4
        processor.state.players["cairo"].units = new List<double>() { 0, 5, 10, 15, 20, 25, 30, 35, 40 };
        players["cairo"].units = new List<double>() { 0, 5, 10, 15, 20, 25, 30, 35, 40 };
        processor.UpdateDefensePlan("cairo", new BattleCommand("a", bigPlan, bigSquad));
        bool passedSeventh = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(savedIslands, processor.state.islands)
        && DefensePlansAreEqual(savedIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedSeventh);

        //Fail because sqd length != 9
        processor.state.players["cairo"].units = new List<double>() { 0, 2, 4, 6, 8, 10, 12, 14, 16 };
        players["cairo"].units = new List<double>() { 0, 2, 4, 6, 8, 10, 12, 14, 16 };
        int[][] oddSquad = new int[][] { new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 } };
        processor.UpdateDefensePlan("cairo", new BattleCommand("a", pln, oddSquad));
        bool passedEigth = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(savedIslands, processor.state.islands)
        && DefensePlansAreEqual(savedIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedEigth);

        //Succeed in defending one hill tile with one squad
        int[][] singlePlan = new int[][] { new int[] { 0, 1, 4 } };
        int[][] singleSquad = new int[][] { new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 } };
        processor.state.players["cairo"].units = new List<double>() { 0, 2, 4, 6, 8, 10, 12, 14, 16 };
        players["cairo"].units = new List<double>() { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        alteredIslands["a"].squadCounts = new List<List<int>>() { new List<int>(){ 0, 1, 2, 3, 4, 5, 6, 7, 8 } };
        alteredIslands["a"].squadPlans = new List<List<int>>() { new List<int>(){ 0, 1, 4} };
        processor.UpdateDefensePlan("cairo", new BattleCommand("a", singlePlan, singleSquad));
        bool passedNinth = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(alteredIslands, processor.state.islands)
        && DefensePlansAreEqual(alteredIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedNinth);

        //Succeed in defending multiple hill tiles with multiple squads
        ResetTestData();
        processor.state.players["cairo"].units = new List<double>() { 0, 4, 8, 12, 16, 20, 24, 28, 32 };
        players["cairo"].units.Clear();
        players["cairo"].units.AddRange( new double[9]);
        alteredIslands["a"].squadCounts = new List<List<int>>() { new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8 } };
        alteredIslands["a"].squadPlans = new List<List<int>>() { new List<int> { 0, 1, 4 }, new List<int> { 3, 2, 6, 7 }, new List<int> { 8, 4, 9 }, new List<int> { 11, 10, 6, 7 } };
        processor.UpdateDefensePlan("cairo", new BattleCommand("a", maxPlan, maxSquad));
        bool passedTenth = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(alteredIslands, processor.state.islands)
        && DefensePlansAreEqual(alteredIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedTenth);

        //Succeed in defending single lake tile with squad
        ResetTestData();
        int[][] singleLakePlan = new int[][] { new int[] { 6, 2, 3, 5, 7, 10, 11 } };
        int[][] singleLakeSquad = new int[][] { new int[] { 0, 1, 2, 0, 0, 0, 6, 7, 8 } };
        processor.state.players["cairo"].units = new List<double>() { 0, 1, 2, 0, 0, 0, 6, 7, 8 };
        players["cairo"].units.Clear();
        players["cairo"].units.AddRange(new double[9]);
        alteredIslands["a"].squadPlans = new List<List<int>>() { new List<int> { 6, 2, 3, 5, 7, 10, 11 } };
        alteredIslands["a"].squadCounts = new List<List<int>>() { new List<int> { 0, 1, 2, 0, 0, 0, 6, 7, 8 } };
        processor.UpdateDefensePlan("cairo", new BattleCommand("a", singleLakePlan, singleLakeSquad));
        bool passedEleventh = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(alteredIslands, processor.state.islands)
        && DefensePlansAreEqual(alteredIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedEleventh);

        //Fail in defending single lake tile with squad and 1 light tank
        ResetTestData();
        int[][] singleLakeSquadFail = new int[][] { new int[] { 0, 1, 2, 1, 0, 0, 6, 7, 8 } };
        processor.state.players["cairo"].units = new List<double>() { 0, 1, 2, 1, 0, 0, 6, 7, 8 };
        players["cairo"].units.Clear();
        players["cairo"].units = new List<double>() { 0, 1, 2, 1, 0, 0, 6, 7, 8 };
        alteredIslands["a"].squadPlans = null;
        alteredIslands["a"].squadCounts = null;
        processor.UpdateDefensePlan("cairo", new BattleCommand("a", singleLakePlan, singleLakeSquadFail));
        bool passedTwelfth = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(alteredIslands, processor.state.islands)
        && DefensePlansAreEqual(alteredIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedTwelfth);

        //Fail in defending single lake tile with squad and 1 medium tank
        ResetTestData();
        singleLakeSquadFail = new int[][] { new int[] { 0, 1, 2, 0, 1, 0, 6, 7, 8 } };
        processor.state.players["cairo"].units = new List<double>() { 0, 1, 2, 0, 1, 0, 6, 7, 8 };
        players["cairo"].units.Clear();
        players["cairo"].units = new List<double>() { 0, 1, 2, 0, 1, 0, 6, 7, 8 };
        processor.UpdateDefensePlan("cairo", new BattleCommand("a", singleLakePlan, singleLakeSquadFail));
        bool passedThirteenth = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(alteredIslands, processor.state.islands)
        && DefensePlansAreEqual(alteredIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedThirteenth);

        //Fail in defending single lake tile with squad and 1 heavy tank
        ResetTestData();
        singleLakeSquadFail = new int[][] { new int[] { 0, 1, 2, 0, 0, 1, 6, 7, 8 } };
        processor.state.players["cairo"].units = new List<double>() { 0, 1, 2, 0, 0, 1, 6, 7, 8 };
        players["cairo"].units.Clear();
        players["cairo"].units = new List<double>() { 0, 1, 2, 0, 0, 1, 6, 7, 8 };
        processor.UpdateDefensePlan("cairo", new BattleCommand("a", singleLakePlan, singleLakeSquadFail));
        bool passedFourteenth = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(alteredIslands, processor.state.islands)
        && DefensePlansAreEqual(alteredIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedFourteenth);

        //Succeed in defending mountain lake tile with squad
        ResetTestData();
        int[][] singleMountainPlan = new int[][] { new int[] { 10, 5, 6, 9, 11 } };
        int[][] singleMountainSquad = new int[][] { new int[] { 0, 1, 2, 3, 4, 5, 0, 0, 0 } };
        processor.state.players["cairo"].units = new List<double>() { 0, 1, 2, 3, 4, 5, 0, 0, 0 };
        players["cairo"].units.Clear();
        players["cairo"].units.AddRange(new double[9]);
        alteredIslands["a"].squadPlans = new List<List<int>>() { new List<int> { 10, 5, 6, 9, 11 } };
        alteredIslands["a"].squadCounts = new List<List<int>>() { new List<int> { 0, 1, 2, 3, 4, 5, 0, 0, 0 } };
        processor.UpdateDefensePlan("cairo", new BattleCommand("a", singleMountainPlan, singleMountainSquad));
        bool passedFifteenth = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(alteredIslands, processor.state.islands)
        && DefensePlansAreEqual(alteredIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedFifteenth);

        //Fail in defending single mountain tile with squad and 1 light fighter
        ResetTestData();
        int[][] singleMountainFail = new int[][] { new int[] { 0, 1, 2, 3, 4, 5, 1, 0, 0 } };
        processor.state.players["cairo"].units = new List<double>() { 0, 1, 2, 3, 4, 5, 1, 0, 0 };
        players["cairo"].units.Clear();
        players["cairo"].units = new List<double>() { 0, 1, 2, 3, 4, 5, 1, 0, 0 };
        alteredIslands["a"].squadPlans = null;
        alteredIslands["a"].squadCounts = null;
        processor.UpdateDefensePlan("cairo", new BattleCommand("a", singleMountainPlan, singleMountainFail));
        bool passedSixteenth = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(savedIslands, processor.state.islands)
        && DefensePlansAreEqual(savedIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedSixteenth);

        //Fail in defending single mountain tile with squad and 1 medium fighter
        ResetTestData();
        singleMountainFail = new int[][] { new int[] { 0, 1, 2, 3, 4, 5, 0, 1, 0 } };
        processor.state.players["cairo"].units = new List<double>() { 0, 1, 2, 3, 4, 5, 0, 1, 0 };
        players["cairo"].units.Clear();
        players["cairo"].units = new List<double>() { 0, 1, 2, 3, 4, 5, 0, 1, 0 };
        processor.UpdateDefensePlan("cairo", new BattleCommand("a", singleMountainPlan, singleMountainFail));
        bool passedSeventeen = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(savedIslands, processor.state.islands)
        && DefensePlansAreEqual(savedIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedSeventeen);

        //Fail in defending single mountain tile with squad and 1 bomber
        ResetTestData();
        singleMountainFail = new int[][] { new int[] { 0, 1, 2, 3, 4, 5, 0, 0, 1 } };
        processor.state.players["cairo"].units = new List<double>() { 0, 1, 2, 3, 4, 5, 0, 0, 1 };
        players["cairo"].units.Clear();
        players["cairo"].units = new List<double>() { 0, 1, 2, 3, 4, 5, 0, 0, 1 };
        processor.UpdateDefensePlan("cairo", new BattleCommand("a", singleMountainPlan, singleMountainFail));
        bool passedEighteenth = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(savedIslands, processor.state.islands)
        && DefensePlansAreEqual(savedIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedEighteenth);

        //Fail in defending one hill tile with one squad because not enough rifleman
        ResetTestData();
        processor.state.players["cairo"].units = new List<double>() { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        players["cairo"].units.Clear();
        players["cairo"].units = new List<double>() { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        processor.UpdateDefensePlan("cairo", new BattleCommand("a", singlePlan, new int[][] { new int[] { 1, 1, 2, 3, 4, 5, 6, 7, 8 } }));
        bool passedNineteenth = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(savedIslands, processor.state.islands)
        && DefensePlansAreEqual(savedIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedNineteenth);

        //Fail in defending one hill tile with one squad because not enough machinegunners
        ResetTestData();
        processor.state.players["cairo"].units = new List<double>() { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        players["cairo"].units.Clear();
        players["cairo"].units = new List<double>() { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        processor.UpdateDefensePlan("cairo", new BattleCommand("a", singlePlan, new int[][] { new int[] { 0, 2, 2, 3, 4, 5, 6, 7, 8 } }));
        bool passedTwentieth = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(savedIslands, processor.state.islands)
        && DefensePlansAreEqual(savedIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedTwentieth);

        //Fail in defending one hill tile with one squad because not enough bazookamen
        ResetTestData();
        processor.state.players["cairo"].units = new List<double>() { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        players["cairo"].units.Clear();
        players["cairo"].units = new List<double>() { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        processor.UpdateDefensePlan("cairo", new BattleCommand("a", singlePlan, new int[][] { new int[] { 0, 1, 3, 3, 4, 5, 6, 7, 8 } }));
        bool passedTwentyFirst = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(savedIslands, processor.state.islands)
        && DefensePlansAreEqual(savedIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedTwentyFirst);

        //Fail in defending one hill tile with one squad because not enough light tanks
        ResetTestData();
        processor.state.players["cairo"].units = new List<double>() { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        players["cairo"].units.Clear();
        players["cairo"].units = new List<double>() { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        processor.UpdateDefensePlan("cairo", new BattleCommand("a", singlePlan, new int[][] { new int[] { 0, 1, 2, 4, 4, 5, 6, 7, 8 } }));
        bool passedTwentySecond = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(savedIslands, processor.state.islands)
        && DefensePlansAreEqual(savedIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedTwentySecond);

        //Fail in defending one hill tile with one squad because not enough medium tanks
        ResetTestData();
        processor.state.players["cairo"].units = new List<double>() { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        players["cairo"].units.Clear();
        players["cairo"].units = new List<double>() { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        processor.UpdateDefensePlan("cairo", new BattleCommand("a", singlePlan, new int[][] { new int[] { 0, 1, 2, 3, 5, 5, 6, 7, 8 } }));
        bool passedTwentyThird = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(savedIslands, processor.state.islands)
        && DefensePlansAreEqual(savedIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedTwentyThird);

        //Fail in defending one hill tile with one squad because not enough heavy tanks
        ResetTestData();
        processor.state.players["cairo"].units = new List<double>() { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        players["cairo"].units.Clear();
        players["cairo"].units = new List<double>() { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        processor.UpdateDefensePlan("cairo", new BattleCommand("a", singlePlan, new int[][] { new int[] { 0, 1, 2, 3, 4, 6, 6, 7, 8 } }));
        bool passedTwentyFourth = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(savedIslands, processor.state.islands)
        && DefensePlansAreEqual(savedIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedTwentyFourth);

        //Fail in defending one hill tile with one squad because not enough light fighters
        ResetTestData();
        processor.state.players["cairo"].units = new List<double>() { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        players["cairo"].units.Clear();
        players["cairo"].units = new List<double>() { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        processor.UpdateDefensePlan("cairo", new BattleCommand("a", singlePlan, new int[][] { new int[] { 0, 1, 2, 3, 4, 5, 7, 7, 8 } }));
        bool passedTwentyFifth = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(savedIslands, processor.state.islands)
        && DefensePlansAreEqual(savedIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedTwentyFifth);

        //Fail in defending one hill tile with one squad because not enough medium fighters
        ResetTestData();
        processor.state.players["cairo"].units = new List<double>() { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        players["cairo"].units.Clear();
        players["cairo"].units = new List<double>() { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        processor.UpdateDefensePlan("cairo", new BattleCommand("a", singlePlan, new int[][] { new int[] { 0, 1, 2, 3, 4, 5, 6, 8, 8 } }));
        bool passedTwentySixth = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(savedIslands, processor.state.islands)
        && DefensePlansAreEqual(savedIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedTwentySixth);

        //Fail in defending one hill tile with one squad because not enough bombers
        ResetTestData();
        processor.state.players["cairo"].units = new List<double>() { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        players["cairo"].units.Clear();
        players["cairo"].units = new List<double>() { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        processor.UpdateDefensePlan("cairo", new BattleCommand("a", singlePlan, new int[][] { new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 9 } }));
        bool passedTwentySeventh = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(savedIslands, processor.state.islands)
        && DefensePlansAreEqual(savedIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedTwentySeventh);

        //Succeed in updating position that has existing squad
        ResetTestData();
        processor.state.players["cairo"].units = new List<double>() { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
        players["cairo"].units.Clear();
        players["cairo"].units = new List<double>() { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        processor.state.islands["a"].squadPlans = new List<List<int>> { new List<int> { 0, 1, 4 } };
        processor.state.islands["a"].squadCounts = new List<List<int>> { new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8 } };
        alteredIslands["a"].squadPlans = new List<List<int>> { new List<int> { 0, 1, 4 } };
        alteredIslands["a"].squadCounts = new List<List<int>> { new List<int> { 0, 2, 4, 6, 8, 10, 12, 14, 16, } };
        processor.UpdateDefensePlan("cairo", new BattleCommand("a", singlePlan, new int[][] { new int[] { 0, 2, 4, 6, 8, 10, 12, 14, 16, } }));
        bool passedTwentyEigth = PlayersAreEqualExcept("", players, processor.state.players) && IslandsAreEqual(alteredIslands, processor.state.islands)
        && DefensePlansAreEqual(alteredIslands, processor.state.islands);
        defenseUpdateResults += GetPassOrFail(passedTwentyEigth);
    }

    void IncrementResourcesTest()
    {
        ResetTestData();
        resourceUpdateResults = "";
        SetIslandResources();
        Dictionary<string, Island> savedIslands = JsonConvert.DeserializeObject<Dictionary<string, Island>>(JsonConvert.SerializeObject(processor.state.islands));
        Dictionary<string, Island> alteredIslands = JsonConvert.DeserializeObject<Dictionary<string, Island>>(JsonConvert.SerializeObject(processor.state.islands));

        //Succeed in only free resource updates with no collectors
        ClearPlayers();
        players["cairo"].resources.AddRange(new double[] { Constants.freeResourceRates[0], Constants.freeResourceRates[1], Constants.freeResourceRates[2], Constants.freeResourceRates[3] });
        players["pimpMacD"].resources.AddRange(new double[] { Constants.freeResourceRates[0], Constants.freeResourceRates[1], Constants.freeResourceRates[2], Constants.freeResourceRates[3] });
        players["nox"].resources.AddRange(new double[] { Constants.freeResourceRates[0], Constants.freeResourceRates[1], Constants.freeResourceRates[2], Constants.freeResourceRates[3] });
        processor.UpdateIslandAndPlayerResources();
        bool passedFirst = PlayersAreEqualExcept("", processor.state.players, players) && IslandsAreEqual(processor.state.islands, savedIslands)
        && ResourcesAreEqual(processor.state.islands, savedIslands);
        resourceUpdateResults += GetPassOrFail(passedFirst);

        //Succeed in one player updating oil where one collector on island exists
        ClearPlayers();
        processor.state.islands["a"].collectors = "100000000000";
        alteredIslands["a"].collectors = "100000000000";
        alteredIslands["a"].resources[0][0]--; 
        players["cairo"].resources.AddRange(new double[] { Constants.freeResourceRates[0], Constants.freeResourceRates[1] + Constants.extractRates[0], Constants.freeResourceRates[2], Constants.freeResourceRates[3] });
        players["pimpMacD"].resources.AddRange(new double[] { Constants.freeResourceRates[0], Constants.freeResourceRates[1], Constants.freeResourceRates[2], Constants.freeResourceRates[3] });
        players["nox"].resources.AddRange(new double[] { Constants.freeResourceRates[0], Constants.freeResourceRates[1], Constants.freeResourceRates[2], Constants.freeResourceRates[3] });
        processor.UpdateIslandAndPlayerResources();
        bool passedSecond = PlayersAreEqualExcept("", processor.state.players, players) && IslandsAreEqual(processor.state.islands, alteredIslands)
        && ResourcesAreEqual(processor.state.islands, alteredIslands);
        resourceUpdateResults += GetPassOrFail(passedSecond);

        //Succeed in multiple players updating oil where one collector on island exists
        ClearPlayers();
        ClearCollectors();
        ClearCollectors(alteredIslands);
        processor.state.islands["a"].collectors = "100000000000";
        alteredIslands["a"].collectors = "100000000000";
        alteredIslands["a"].resources[0][0]--; ;
        processor.state.islands["g"].collectors = "000100000000";
        alteredIslands["g"].collectors = "000100000000";
        alteredIslands["g"].resources[3][0]--;
        processor.state.islands["l"].collectors = "100000000000";
        alteredIslands["l"].collectors = "100000000000";
        alteredIslands["l"].resources[0][0]--;
        players["cairo"].resources.AddRange(new double[] { Constants.freeResourceRates[0], Constants.freeResourceRates[1] + Constants.extractRates[0], Constants.freeResourceRates[2], Constants.freeResourceRates[3] });
        players["pimpMacD"].resources.AddRange(new double[] { Constants.freeResourceRates[0], Constants.freeResourceRates[1] + Constants.extractRates[0], Constants.freeResourceRates[2], Constants.freeResourceRates[3] });
        players["nox"].resources.AddRange(new double[] { Constants.freeResourceRates[0], Constants.freeResourceRates[1] + Constants.extractRates[0], Constants.freeResourceRates[2], Constants.freeResourceRates[3] });
        processor.UpdateIslandAndPlayerResources();
        bool passedThird = PlayersAreEqualExcept("", processor.state.players, players) && IslandsAreEqual(processor.state.islands, alteredIslands)
        && ResourcesAreEqual(processor.state.islands, alteredIslands);
        resourceUpdateResults += GetPassOrFail(passedThird);

        //Succeed in one player updating metal where one collector on island exists
        ClearPlayers();
        ClearCollectors();
        ClearCollectors(alteredIslands);
        processor.state.islands["a"].collectors = "200000000000";
        alteredIslands["a"].collectors = "200000000000";
        alteredIslands["a"].resources[0][1]--;
        players["cairo"].resources.AddRange(new double[] { Constants.freeResourceRates[0], Constants.freeResourceRates[1], Constants.freeResourceRates[2] + Constants.extractRates[1], Constants.freeResourceRates[3] });
        players["pimpMacD"].resources.AddRange(new double[] { Constants.freeResourceRates[0], Constants.freeResourceRates[1], Constants.freeResourceRates[2], Constants.freeResourceRates[3] });
        players["nox"].resources.AddRange(new double[] { Constants.freeResourceRates[0], Constants.freeResourceRates[1], Constants.freeResourceRates[2], Constants.freeResourceRates[3] });
        processor.UpdateIslandAndPlayerResources();
        bool passedFourth = PlayersAreEqualExcept("", processor.state.players, players) && IslandsAreEqual(processor.state.islands, alteredIslands)
        && ResourcesAreEqual(processor.state.islands, alteredIslands);
        resourceUpdateResults += GetPassOrFail(passedFourth);

        //Succeed in multiple players updating metal where one collector on island exists
        ClearPlayers();
        ClearCollectors();
        ClearCollectors(alteredIslands);
        processor.state.islands["a"].collectors = "200000000000";
        alteredIslands["a"].collectors = "200000000000";
        alteredIslands["a"].resources[0][1]--;
        processor.state.islands["g"].collectors = "200000000000";
        alteredIslands["g"].collectors = "200000000000";
        alteredIslands["g"].resources[0][1]--;
        processor.state.islands["j"].collectors = "020000000000";
        alteredIslands["j"].collectors = "020000000000";
        alteredIslands["j"].resources[1][1]--;
        players["cairo"].resources.AddRange(new double[] { Constants.freeResourceRates[0], Constants.freeResourceRates[1], Constants.freeResourceRates[2] + Constants.extractRates[1], Constants.freeResourceRates[3] });
        players["pimpMacD"].resources.AddRange(new double[] { Constants.freeResourceRates[0], Constants.freeResourceRates[1], Constants.freeResourceRates[2] + Constants.extractRates[1], Constants.freeResourceRates[3] });
        players["nox"].resources.AddRange(new double[] { Constants.freeResourceRates[0], Constants.freeResourceRates[1], Constants.freeResourceRates[2] + Constants.extractRates[1], Constants.freeResourceRates[3] });
        processor.UpdateIslandAndPlayerResources();
        bool passedFifth = PlayersAreEqualExcept("", processor.state.players, players) && IslandsAreEqual(processor.state.islands, alteredIslands)
        && ResourcesAreEqual(processor.state.islands, alteredIslands);
        resourceUpdateResults += GetPassOrFail(passedFifth);

        //Succeed in one player updating lime where one collector on island exists
        ClearPlayers();
        ClearCollectors();
        ClearCollectors(alteredIslands);
        processor.state.islands["a"].collectors = "300000000000";
        alteredIslands["a"].collectors = "300000000000";
        alteredIslands["a"].resources[0][2]--;
        players["cairo"].resources.AddRange(new double[] { Constants.freeResourceRates[0], Constants.freeResourceRates[1], Constants.freeResourceRates[2], Constants.freeResourceRates[3] + Constants.extractRates[2] });
        players["pimpMacD"].resources.AddRange(new double[] { Constants.freeResourceRates[0], Constants.freeResourceRates[1], Constants.freeResourceRates[2], Constants.freeResourceRates[3] });
        players["nox"].resources.AddRange(new double[] { Constants.freeResourceRates[0], Constants.freeResourceRates[1], Constants.freeResourceRates[2], Constants.freeResourceRates[3] });
        processor.UpdateIslandAndPlayerResources();
        bool passedSixth = PlayersAreEqualExcept("", processor.state.players, players) && IslandsAreEqual(processor.state.islands, alteredIslands)
        && ResourcesAreEqual(processor.state.islands, alteredIslands);
        resourceUpdateResults += GetPassOrFail(passedSixth);

        //Succeed in multiple players updating lime where one collector on island exists
        ClearPlayers();
        ClearCollectors();
        ClearCollectors(alteredIslands);
        processor.state.islands["a"].collectors = "300000000000";
        alteredIslands["a"].collectors = "300000000000";
        alteredIslands["a"].resources[0][2]--;
        processor.state.islands["e"].collectors = "300000000000";
        alteredIslands["e"].collectors = "300000000000";
        alteredIslands["e"].resources[0][2]--;
        processor.state.islands["n"].collectors = "300000000000";
        alteredIslands["n"].collectors = "300000000000";
        alteredIslands["n"].resources[0][2]--;
        players["cairo"].resources.AddRange(new double[] { Constants.freeResourceRates[0], Constants.freeResourceRates[1], Constants.freeResourceRates[2], Constants.freeResourceRates[3] + Constants.extractRates[2] });
        players["pimpMacD"].resources.AddRange(new double[] { Constants.freeResourceRates[0], Constants.freeResourceRates[1], Constants.freeResourceRates[2], Constants.freeResourceRates[3] + Constants.extractRates[2] });
        players["nox"].resources.AddRange(new double[] { Constants.freeResourceRates[0], Constants.freeResourceRates[1], Constants.freeResourceRates[2], Constants.freeResourceRates[3] + Constants.extractRates[2] });
        processor.UpdateIslandAndPlayerResources();
        bool passedSeventh = PlayersAreEqualExcept("", processor.state.players, players) && IslandsAreEqual(processor.state.islands, alteredIslands)
        && ResourcesAreEqual(processor.state.islands, alteredIslands);
        resourceUpdateResults += GetPassOrFail(passedSeventh);

        //Succeed in players updating multiple resources where one tile has collectors.
        ClearPlayers();
        ClearCollectors();
        ClearCollectors(alteredIslands);
        //Cairo 1 oil 1 metal
        processor.state.islands["c"].collectors = "000004000000";
        alteredIslands["c"].collectors = "000004000000";
        alteredIslands["c"].resources[5][0]--;
        alteredIslands["c"].resources[5][1]--;

        //Nox 1 oil 1 lime
        processor.state.islands["l"].collectors = "000005000000";
        alteredIslands["l"].collectors = "000005000000";
        alteredIslands["l"].resources[5][0]--;
        alteredIslands["l"].resources[5][2]--;

        //PimpMacD 1 metal 1 lime
        processor.state.islands["g"].collectors = "600000000000";
        alteredIslands["g"].collectors = "600000000000";
        alteredIslands["g"].resources[0][1]--;
        alteredIslands["g"].resources[0][2]--;

        //Cairo 1 oil 1 metal 1 lime
        processor.state.islands["a"].collectors = "700000000000";
        alteredIslands["a"].collectors = "700000000000";
        alteredIslands["a"].resources[0][0]--;
        alteredIslands["a"].resources[0][1]--;
        alteredIslands["a"].resources[0][2]--;

        players["cairo"].resources.AddRange(new double[] { 0, 0, 0, 0 });
        players["pimpMacD"].resources.AddRange(new double[] { 0, 0, 0, 0 });
        players["nox"].resources.AddRange(new double[] { 0, 0, 0, 0 });

        players["cairo"].resources[0] = Constants.freeResourceRates[0];
        players["cairo"].resources[1] = Constants.freeResourceRates[1] + (Constants.extractRates[0] * 2);
        players["cairo"].resources[2] = Constants.freeResourceRates[2] + (Constants.extractRates[1] * 2);
        players["cairo"].resources[3] = Constants.freeResourceRates[3] + Constants.extractRates[2];

        players["pimpMacD"].resources[0] = Constants.freeResourceRates[0];
        players["pimpMacD"].resources[1] = Constants.freeResourceRates[1];
        players["pimpMacD"].resources[2] = Constants.freeResourceRates[2] + Constants.extractRates[1];
        players["pimpMacD"].resources[3] = Constants.freeResourceRates[3] + Constants.extractRates[2];

        players["nox"].resources[0] = Constants.freeResourceRates[0];
        players["nox"].resources[1] = Constants.freeResourceRates[1] + Constants.extractRates[0];
        players["nox"].resources[2] = Constants.freeResourceRates[2];
        players["nox"].resources[3] = Constants.freeResourceRates[3] + Constants.extractRates[2];

        processor.UpdateIslandAndPlayerResources();
        bool passedEighth = PlayersAreEqualExcept("", processor.state.players, players) && IslandsAreEqual(processor.state.islands, alteredIslands)
        && ResourcesAreEqual(processor.state.islands, alteredIslands);
        resourceUpdateResults += GetPassOrFail(passedEighth);

        //Succeed in updating single island with single collectors on multiple tiles.
        ClearPlayers();
        ClearCollectors();
        ClearCollectors(alteredIslands);
        processor.state.islands["a"].collectors = "300001000002";
        alteredIslands["a"].collectors = "300001000002";
        alteredIslands["a"].resources[0][2]--;
        alteredIslands["a"].resources[5][0]--;
        alteredIslands["a"].resources[11][1]--;
        players["cairo"].resources.AddRange(new double[] { Constants.freeResourceRates[0], Constants.freeResourceRates[1] + Constants.extractRates[0], Constants.freeResourceRates[2] + Constants.extractRates[1], Constants.freeResourceRates[3] + Constants.extractRates[2] });
        players["pimpMacD"].resources.AddRange(new double[] { Constants.freeResourceRates[0], Constants.freeResourceRates[1], Constants.freeResourceRates[2], Constants.freeResourceRates[3] });
        players["nox"].resources.AddRange(new double[] { Constants.freeResourceRates[0], Constants.freeResourceRates[1], Constants.freeResourceRates[2], Constants.freeResourceRates[3] });
        processor.UpdateIslandAndPlayerResources();
        bool passedNinth = PlayersAreEqualExcept("", processor.state.players, players) && IslandsAreEqual(processor.state.islands, alteredIslands)
        && ResourcesAreEqual(processor.state.islands, alteredIslands);
        resourceUpdateResults += GetPassOrFail(passedNinth);

        //Succeed in updating multiple islands with single collectors on multiple tiles.
        ClearPlayers();
        ClearCollectors();
        ClearCollectors(alteredIslands);
        processor.state.islands["a"].collectors = "300001000002";
        alteredIslands["a"].collectors = "300001000002";
        alteredIslands["a"].resources[0][2]--;
        alteredIslands["a"].resources[5][0]--;
        alteredIslands["a"].resources[11][1]--;
        processor.state.islands["g"].collectors = "300100000020";
        alteredIslands["g"].collectors = "300100000020";
        alteredIslands["g"].resources[0][2]--;
        alteredIslands["g"].resources[3][0]--;
        alteredIslands["g"].resources[10][1]--;
        processor.state.islands["o"].collectors = "120000003000";
        alteredIslands["o"].collectors = "120000003000";
        alteredIslands["o"].resources[0][0]--;
        alteredIslands["o"].resources[1][1]--;
        alteredIslands["o"].resources[8][2]--;
        players["cairo"].resources.AddRange(new double[] { Constants.freeResourceRates[0], Constants.freeResourceRates[1] + Constants.extractRates[0], Constants.freeResourceRates[2] + Constants.extractRates[1], Constants.freeResourceRates[3] + Constants.extractRates[2] });
        players["pimpMacD"].resources.AddRange(new double[] { Constants.freeResourceRates[0], Constants.freeResourceRates[1] + Constants.extractRates[0], Constants.freeResourceRates[2] + Constants.extractRates[1], Constants.freeResourceRates[3] + Constants.extractRates[2] });
        players["nox"].resources.AddRange(new double[] { Constants.freeResourceRates[0], Constants.freeResourceRates[1] + Constants.extractRates[0], Constants.freeResourceRates[2] + Constants.extractRates[1], Constants.freeResourceRates[3] + Constants.extractRates[2] });
        processor.UpdateIslandAndPlayerResources();
        bool passedTenth = PlayersAreEqualExcept("", processor.state.players, players) && IslandsAreEqual(processor.state.islands, alteredIslands)
        && ResourcesAreEqual(processor.state.islands, alteredIslands);
        resourceUpdateResults += GetPassOrFail(passedTenth);

        //Succeed in updating single island with multiple collectors on multiple tiles.
        ClearPlayers();
        ClearCollectors();
        ClearCollectors(alteredIslands);
        processor.state.islands["o"].collectors = "400006000000";
        alteredIslands["o"].collectors = "400006000000";
        alteredIslands["o"].resources[0][0]--;
        alteredIslands["o"].resources[0][1]--;
        alteredIslands["o"].resources[5][1]--;
        alteredIslands["o"].resources[5][2]--;
        players["cairo"].resources.AddRange(new double[] { Constants.freeResourceRates[0], Constants.freeResourceRates[1], Constants.freeResourceRates[2], Constants.freeResourceRates[3] });
        players["pimpMacD"].resources.AddRange(new double[] { Constants.freeResourceRates[0], Constants.freeResourceRates[1], Constants.freeResourceRates[2], Constants.freeResourceRates[3] });
        players["nox"].resources.AddRange(new double[] { Constants.freeResourceRates[0], Constants.freeResourceRates[1] + Constants.extractRates[0], Constants.freeResourceRates[2] + (Constants.extractRates[1] * 2), Constants.freeResourceRates[3] + Constants.extractRates[2] });
        processor.UpdateIslandAndPlayerResources();
        bool passedEleventh = PlayersAreEqualExcept("", processor.state.players, players) && IslandsAreEqual(processor.state.islands, alteredIslands)
        && ResourcesAreEqual(processor.state.islands, alteredIslands);
        resourceUpdateResults += GetPassOrFail(passedEleventh);

        //Succeed in updating multiple islands with multiple collectors on multiple tiles.
        ClearPlayers();
        ClearCollectors();
        ClearCollectors(alteredIslands);
        processor.state.islands["d"].collectors = "000000000460";
        alteredIslands["d"].collectors = "000000000460";
        alteredIslands["d"].resources[9][0]--;
        alteredIslands["d"].resources[9][1]--;
        alteredIslands["d"].resources[10][1]--;
        alteredIslands["d"].resources[10][2]--;
        processor.state.islands["o"].collectors = "400006000000";
        alteredIslands["o"].collectors = "400006000000";
        alteredIslands["o"].resources[0][0]--;
        alteredIslands["o"].resources[0][1]--;
        alteredIslands["o"].resources[5][1]--;
        alteredIslands["o"].resources[5][2]--;
        players["cairo"].resources.AddRange(new double[] { Constants.freeResourceRates[0], Constants.freeResourceRates[1] + Constants.extractRates[0], Constants.freeResourceRates[2] + (Constants.extractRates[1] * 2), Constants.freeResourceRates[3] + Constants.extractRates[2] });
        players["pimpMacD"].resources.AddRange(new double[] { Constants.freeResourceRates[0], Constants.freeResourceRates[1], Constants.freeResourceRates[2], Constants.freeResourceRates[3] });
        players["nox"].resources.AddRange(new double[] { Constants.freeResourceRates[0], Constants.freeResourceRates[1] + Constants.extractRates[0], Constants.freeResourceRates[2] + (Constants.extractRates[1] * 2), Constants.freeResourceRates[3] + Constants.extractRates[2] });
        processor.UpdateIslandAndPlayerResources();
        bool passedTwelfth = PlayersAreEqualExcept("", processor.state.players, players) && IslandsAreEqual(processor.state.islands, alteredIslands)
        && ResourcesAreEqual(processor.state.islands, alteredIslands);
        resourceUpdateResults += GetPassOrFail(passedTwelfth);
    }

    void TestDepletedIslandSubmissions()
    {
        ResetTestData();
        depletedIslandResults = "";
        SetIslandResources();
        Dictionary<string, Island> savedIslands = JsonConvert.DeserializeObject<Dictionary<string, Island>>(JsonConvert.SerializeObject(processor.state.islands));
        Dictionary<string, Island> alteredIslands = JsonConvert.DeserializeObject<Dictionary<string, Island>>(JsonConvert.SerializeObject(processor.state.islands));

        //Fail because island is not depleted
        processor.SubmitDepletedIslands("cairo", players["cairo"].islands);
        bool passedFirst = IslandsAreEqual(processor.state.islands, savedIslands) && PlayersAreEqualExcept("", processor.state.players, players)
        && processor.state.depletedContributions.Count == 0; 
        depletedIslandResults += GetPassOrFail(passedFirst);

        //Succeed with 1 depeleted island.
        ClearResources(processor.state.islands["a"].resources);
        alteredIslands.Remove("a");
        players["cairo"].islands.Remove("a");
        processor.SubmitDepletedIslands("cairo", new List<string> { "a" });
        bool passedSecond = IslandsAreEqual(processor.state.islands, alteredIslands) && PlayersAreEqualExcept("cairo", processor.state.players, players)
        && processor.state.depletedContributions.ContainsKey("cairo") && processor.state.depletedContributions["cairo"].Contains("a")
        && processor.state.depletedContributions.Count == 1 && processor.state.depletedContributions["cairo"].Count == 1;
        depletedIslandResults += GetPassOrFail(passedSecond);

        //Succeed with multiple depeleted island.
        ResetTestData();
        SetIslandResources();
        ClearResources(processor.state.islands["a"].resources);
        ClearResources(processor.state.islands["b"].resources);
        ClearResources(processor.state.islands["c"].resources);
        ClearResources(processor.state.islands["d"].resources);
        alteredIslands.Remove("b");
        alteredIslands.Remove("c");
        alteredIslands.Remove("d");
        players["cairo"].islands.Clear();
        processor.SubmitDepletedIslands("cairo", new List<string> { "a", "b", "c", "d" });
        bool passedThird = IslandsAreEqual(processor.state.islands, alteredIslands) && PlayersAreEqualExcept("cairo", processor.state.players, players)
        && processor.state.depletedContributions.ContainsKey("cairo") && Contains(processor.state.depletedContributions["cairo"], new List<string> {"a", "b", "c", "d" })
        && processor.state.depletedContributions.Count == 1 && processor.state.depletedContributions["cairo"].Count == 4;
        depletedIslandResults += GetPassOrFail(passedThird);

        //Succeed with updating to already existing contributions
        ResetTestData();
        SetIslandResources();
        alteredIslands = JsonConvert.DeserializeObject<Dictionary<string, Island>>(JsonConvert.SerializeObject(savedIslands));
        processor.state.islands.Remove("a");
        ClearResources(processor.state.islands["b"].resources);
        alteredIslands.Remove("a");
        alteredIslands.Remove("b");
        players["cairo"].islands.Remove("a");
        processor.state.depletedContributions.Add("cairo", new List<string> { "a" });
        processor.SubmitDepletedIslands("cairo", new List<string> { "b" });
        bool passedFourth = IslandsAreEqual(processor.state.islands, alteredIslands) && PlayersAreEqualExcept("cairo", processor.state.players, players)
        && processor.state.depletedContributions.ContainsKey("cairo") && Contains(processor.state.depletedContributions["cairo"], new List<string> { "a", "b" })
        && processor.state.depletedContributions.Count == 1 && processor.state.depletedContributions["cairo"].Count == 2;
        depletedIslandResults += GetPassOrFail(passedFourth);

        //Succeed with 1 depleted island from multiple users.
        ResetTestData();
        SetIslandResources();
        alteredIslands = JsonConvert.DeserializeObject<Dictionary<string, Island>>(JsonConvert.SerializeObject(savedIslands));
        ClearResources(processor.state.islands["a"].resources);
        ClearResources(processor.state.islands["e"].resources);
        ClearResources(processor.state.islands["j"].resources);
        alteredIslands.Remove("a");
        alteredIslands.Remove("e");
        alteredIslands.Remove("j");
        players["cairo"].islands.Remove("a");
        players["pimpMacD"].islands.Remove("e");
        players["nox"].islands.Remove("j");
        processor.SubmitDepletedIslands("cairo", new List<string> { "a" });
        processor.SubmitDepletedIslands("pimpMacD", new List<string> { "e" });
        processor.SubmitDepletedIslands("nox", new List<string> { "j" });
        bool passedFifth = IslandsAreEqual(processor.state.islands, alteredIslands) && PlayersAreEqualExcept("cairo", processor.state.players, players)
        && processor.state.depletedContributions.ContainsKey("pimpMacD") && processor.state.depletedContributions.ContainsKey("nox")
        && processor.state.depletedContributions.ContainsKey("cairo");
        
        if(passedFifth)
            passedFifth = processor.state.depletedContributions["cairo"].Contains("a")
            && processor.state.depletedContributions["pimpMacD"].Contains("e") && processor.state.depletedContributions["nox"].Contains("j")
            && processor.state.depletedContributions.Count == 3 && processor.state.depletedContributions["cairo"].Count == 1
            && processor.state.depletedContributions["pimpMacD"].Count == 1 && processor.state.depletedContributions["nox"].Count == 1;
        depletedIslandResults += GetPassOrFail(passedFifth);

        //Fail multi island because one island is not depleted.
        ResetTestData();
        SetIslandResources();
        ClearResources(processor.state.islands["b"].resources);
        ClearResources(processor.state.islands["c"].resources);
        ClearResources(processor.state.islands["d"].resources);
        processor.SubmitDepletedIslands("cairo", new List<string> { "a", "b", "c", "d" });
        bool passedSixth = IslandsAreEqual(processor.state.islands, savedIslands) && PlayersAreEqualExcept("cairo", processor.state.players, players)
        && processor.state.depletedContributions.Count == 0;
        depletedIslandResults += GetPassOrFail(passedSixth);

        //Fail multi island because one island is not owned by player.
        ResetTestData();
        SetIslandResources();
        ClearResources(processor.state.islands["a"].resources);
        ClearResources(processor.state.islands["b"].resources);
        ClearResources(processor.state.islands["c"].resources);
        ClearResources(processor.state.islands["e"].resources);
        processor.SubmitDepletedIslands("cairo", new List<string> { "a", "b", "c", "e" });
        bool passedSeventh = IslandsAreEqual(processor.state.islands, savedIslands) && PlayersAreEqualExcept("cairo", processor.state.players, players)
        && processor.state.depletedContributions.Count == 0;
        depletedIslandResults += GetPassOrFail(passedSeventh);
    }

    void TestReourcePoolSubmission()
    {
        ResetTestData();
        resourcePoolResults = "";
        double[] holder = new double[] { 3000, 7500, 6000, 1500 };
        List<List<double>> updatedContributions = new List<List<double>> { new List<double> { 0, 0, 0 }, new List<double> { 0, 0, 0 }, new List<double> { 0, 0, 0 } };

        //Succeed in metal submission to oil pool
        players["cairo"].resources[2] = 4000;
        processor.SubmitResourcesToPool("cairo", new ResourceOrder(0, new List<double> { 0, 2000, 0 }));
        updatedContributions[0][1] = 2000;
        bool passedFirst = PlayersAreEqualExcept("", processor.state.players, players) && processor.state.resourceContributions.Count == 1
        && processor.state.resourceContributions.ContainsKey("cairo");

        if (passedFirst)
            passedFirst = IsEqual(processor.state.resourceContributions["cairo"], updatedContributions);

        resourcePoolResults += GetPassOrFail(passedFirst);

        //Succeed in lime submission to metal pool
        players["cairo"].resources[3] = 1000;
        processor.SubmitResourcesToPool("cairo", new ResourceOrder(1, new List<double> { 0, 0, 500 }));
        updatedContributions[1][2] = 500;
        bool passedSecond = PlayersAreEqualExcept("", processor.state.players, players) && processor.state.resourceContributions.Count == 1
        && processor.state.resourceContributions.ContainsKey("cairo");

        if (passedSecond)
            passedSecond = IsEqual(processor.state.resourceContributions["cairo"], updatedContributions);

        resourcePoolResults += GetPassOrFail(passedSecond);

        //Succeed in lime submission to metal pool
        players["cairo"].resources[1] = 5000;
        processor.SubmitResourcesToPool("cairo", new ResourceOrder(2, new List<double> { 2500, 0, 0 }));
        updatedContributions[2][0] = 2500;
        bool passedThird = PlayersAreEqualExcept("", processor.state.players, players) && processor.state.resourceContributions.Count == 1
        && processor.state.resourceContributions.ContainsKey("cairo");

        if (passedThird)
            passedThird = IsEqual(processor.state.resourceContributions["cairo"], updatedContributions);

        resourcePoolResults += GetPassOrFail(passedThird);

        //Succeed with multiple submissions of metal and lime to the oil pool
        players["cairo"].resources[2] = 2000;
        players["cairo"].resources[3] = 500;
        processor.SubmitResourcesToPool("cairo", new ResourceOrder(0, new List<double> { 0, 2000, 500 }));
        updatedContributions[0][1] += 2000;
        updatedContributions[0][2] += 500;
        bool passedFourth = PlayersAreEqualExcept("", processor.state.players, players) && processor.state.resourceContributions.Count == 1
        && processor.state.resourceContributions.ContainsKey("cairo");

        if (passedFourth)
            passedFourth = IsEqual(processor.state.resourceContributions["cairo"], updatedContributions);

        resourcePoolResults += GetPassOrFail(passedFourth);
        
        //Succeed with multiple submissions of oil and lime to the metal pool
        players["cairo"].resources[1] = 2500;
        players["cairo"].resources[3] = 0;
        processor.SubmitResourcesToPool("cairo", new ResourceOrder(1, new List<double> { 2500, 0, 500 }));
        updatedContributions[1][0] += 2500;
        updatedContributions[1][2] += 500;
        bool passedFifth = PlayersAreEqualExcept("", processor.state.players, players) && processor.state.resourceContributions.Count == 1
        && processor.state.resourceContributions.ContainsKey("cairo");

        if (passedFifth)
            passedFifth = IsEqual(processor.state.resourceContributions["cairo"], updatedContributions);

        resourcePoolResults += GetPassOrFail(passedFifth);
        
        //Succeed with multiple submissions of oil and metal to the lime pool
        players["cairo"].resources[1] = 0;
        players["cairo"].resources[2] = 0;
        processor.SubmitResourcesToPool("cairo", new ResourceOrder(2, new List<double> { 2500, 2000, 0 }));
        updatedContributions[2][0] += 2500;
        updatedContributions[2][1] += 2000;
        bool passedSixth = PlayersAreEqualExcept("", processor.state.players, players) && processor.state.resourceContributions.Count == 1
        && processor.state.resourceContributions.ContainsKey("cairo");

        if (passedSixth)
            passedSixth = IsEqual(processor.state.resourceContributions["cairo"], updatedContributions);

        resourcePoolResults += GetPassOrFail(passedSixth);

        //Fail with oil submission to oil pool
        ResetTestData();
        processor.SubmitResourcesToPool("cairo", new ResourceOrder(0, new List<double> { 2500, 0, 0 }));
        bool passedSeventh = PlayersAreEqualExcept("", processor.state.players, players) && processor.state.resourceContributions.Count == 0
        && !processor.state.resourceContributions.ContainsKey("cairo");

        resourcePoolResults += GetPassOrFail(passedSeventh);

        //Fail with multi submission to oil pool
        processor.SubmitResourcesToPool("cairo", new ResourceOrder(0, new List<double> { 2500, 2000, 500 }));
        bool passedEighth = PlayersAreEqualExcept("", processor.state.players, players) && processor.state.resourceContributions.Count == 0
        && !processor.state.resourceContributions.ContainsKey("cairo");

        resourcePoolResults += GetPassOrFail(passedEighth);

        //Fail with metal submission to metal pool
        processor.SubmitResourcesToPool("cairo", new ResourceOrder(1, new List<double> { 0, 2000, 0 }));
        bool passedNinth = PlayersAreEqualExcept("", processor.state.players, players) && processor.state.resourceContributions.Count == 0
        && !processor.state.resourceContributions.ContainsKey("cairo");

        resourcePoolResults += GetPassOrFail(passedNinth);

        //Fail with multi submission to metal pool
        processor.SubmitResourcesToPool("cairo", new ResourceOrder(1, new List<double> { 2500, 2000, 500 }));
        bool passedTenth = PlayersAreEqualExcept("", processor.state.players, players) && processor.state.resourceContributions.Count == 0
        && !processor.state.resourceContributions.ContainsKey("cairo");

        resourcePoolResults += GetPassOrFail(passedTenth);

        //Fail with lime submission to lime pool
        processor.SubmitResourcesToPool("cairo", new ResourceOrder(2, new List<double> { 0, 0, 500 }));
        bool passedEleventh = PlayersAreEqualExcept("", processor.state.players, players) && processor.state.resourceContributions.Count == 0
        && !processor.state.resourceContributions.ContainsKey("cairo");

        resourcePoolResults += GetPassOrFail(passedEleventh);

        //Fail with multi submission to lime pool
        processor.SubmitResourcesToPool("cairo", new ResourceOrder(2, new List<double> { 2500, 2000, 500 }));
        bool passedTwelfth = PlayersAreEqualExcept("", processor.state.players, players) && processor.state.resourceContributions.Count == 0
        && !processor.state.resourceContributions.ContainsKey("cairo");

        resourcePoolResults += GetPassOrFail(passedTwelfth);

        //Fail with negative metal submission to oil pool
        processor.SubmitResourcesToPool("cairo", new ResourceOrder(0, new List<double> { 0, -2500, 0 }));
        bool passedThirteenth = PlayersAreEqualExcept("", processor.state.players, players) && processor.state.resourceContributions.Count == 0
        && !processor.state.resourceContributions.ContainsKey("cairo");

        resourcePoolResults += GetPassOrFail(passedThirteenth);
    }

    void ResourceTransferTest()
    {
        ResetTestData();
        transferResourcesResults = "";

        //Succeed in transfering 15% of unit purchase to depleted pool.
        processor.PurchaseUnits("cairo", new List<int> { 1, 1, 1, 1, 1, 1, 1, 1, 1 });
        bool passedFirst = processor.state.resourcePools[0] == 2710.0 * 0.15 && processor.state.resourcePools[1] == 1035.0 * 0.05
        && processor.state.resourcePools[2] == 1175 * 0.05 && processor.state.resourcePools[3] == 0;
        transferResourcesResults += GetPassOrFail(passedFirst);

        //Succeed in transfering 15% of collector purchases to depleted pool
        ResetTestData();
        processor.state.players["cairo"].resources[0] = 10000;
        processor.state.players["cairo"].resources[1] = 10000;
        processor.state.players["cairo"].resources[2] = 10000;
        processor.state.players["cairo"].resources[3] = 10000;
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "700000000000", "))))))))))))"));
        bool passedSecond = processor.state.resourcePools[0] == 4500.0 * 0.15 && processor.state.resourcePools[1] == 2500.0 * 0.05
        && processor.state.resourcePools[2] == 2500.0 * 0.05 && processor.state.resourcePools[3] == 2500.0 * 0.05;
        transferResourcesResults += GetPassOrFail(passedSecond);

        //Succeed in transfering 15% of defense purchases to depleted pool
        ResetTestData();
        processor.state.players["cairo"].resources[0] = 10000;
        processor.state.players["cairo"].resources[1] = 10000;
        processor.state.players["cairo"].resources[2] = 10000;
        processor.state.players["cairo"].resources[3] = 10000;
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "000000000000", "6)))))))))))"));
        bool passedThird = processor.state.resourcePools[0] == 4500.0 * 0.15 && processor.state.resourcePools[1] == 1600.0 * 0.05
        && processor.state.resourcePools[2] == 2500.0 * 0.05 && processor.state.resourcePools[3] == 1600.0 * 0.05;
        transferResourcesResults += GetPassOrFail(passedThird);

        //Succeed in transfering 15% of search purchase to depleted pool
        ResetTestData();
        processor.DiscoverOrScoutIsland("cairo", "norm", "p");
        bool passedFourth = processor.state.resourcePools[0] == 1000.0 * 0.15 && processor.state.resourcePools[1] == 2500.0 * 0.05
        && processor.state.resourcePools[2] == 0 && processor.state.resourcePools[3] == 0;
        transferResourcesResults += GetPassOrFail(passedFourth);

        //Succeed in updating resource pool.
        ResetTestData();
        processor.state.players["cairo"].resources[0] = 20000;
        processor.state.players["cairo"].resources[1] = 20000;
        processor.state.players["cairo"].resources[2] = 20000;
        processor.state.players["cairo"].resources[3] = 20000;
        processor.PurchaseUnits("cairo", new List<int> { 1, 1, 1, 1, 1, 1, 1, 1, 1 });
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "700000000000", "))))))))))))"));
        processor.DevelopIsland("cairo", new IslandBuildOrder("a", "000000000000", "6)))))))))))"));
        processor.DiscoverOrScoutIsland("cairo", "norm", "p");
        bool passedFifth = processor.state.resourcePools[0] == 12710.0 * 0.15 && processor.state.resourcePools[1] == 7635.0 * 0.05
        && processor.state.resourcePools[2] == 6175.0 * 0.05 && processor.state.resourcePools[3] == 4100.0 * 0.05;
        transferResourcesResults += GetPassOrFail(passedFifth);

        //Succeed in updating resources with multiple users
        processor.state.players["pimpMacD"].resources[0] = 20000;
        processor.state.players["pimpMacD"].resources[1] = 20000;
        processor.state.players["pimpMacD"].resources[2] = 20000;
        processor.state.players["pimpMacD"].resources[3] = 20000;
        processor.PurchaseUnits("pimpMacD", new List<int> { 1, 1, 1, 1, 1, 1, 1, 1, 1 });
        processor.DevelopIsland("pimpMacD", new IslandBuildOrder("g", "300100000020", "))))))))))))"));
        processor.DevelopIsland("pimpMacD", new IslandBuildOrder("e", "000000000000", "6)))))))))))"));
        processor.DiscoverOrScoutIsland("pimpMacD", "norm", "q");
        processor.state.players["nox"].resources[0] = 20000;
        processor.state.players["nox"].resources[1] = 20000;
        processor.state.players["nox"].resources[2] = 20000;
        processor.state.players["nox"].resources[3] = 20000;
        processor.PurchaseUnits("nox", new List<int> { 1, 1, 1, 1, 1, 1, 1, 1, 1 });
        processor.DevelopIsland("nox", new IslandBuildOrder("m", "000003201000", "))))))))))))"));
        processor.DevelopIsland("nox", new IslandBuildOrder("j", "000000000000", "6)))))))))))"));
        processor.DiscoverOrScoutIsland("nox", "norm", "r");
        bool passedSixth = processor.state.resourcePools[0] == 12710.0 * 0.15 * 3.0 && processor.state.resourcePools[1] == 7635.0 * 0.05 * 3.0
        && processor.state.resourcePools[2] == 6175.0 * 0.05 * 3.0 && processor.state.resourcePools[3] == 4100.0 * 0.05 * 3.0;
        transferResourcesResults += GetPassOrFail(passedSixth);

        string savedState = JsonConvert.SerializeObject(processor.state);
        RewardDepletedPool(savedState);
        RewardResourcePool(savedState);
    }

    void RewardDepletedPool(string savedState)
    {
        processor.state = JsonConvert.DeserializeObject<State>(savedState);
        rewardDepletedResults = "";

        //Succeed in splitting with single player.
        processor.SubmitDepletedIslands("cairo", new List<string> { "a" });
        processor.RewardDepletedPool();
        bool passedFirst = (Math.Floor(12710.0 * 0.15 * 3.0) + (20000.0 - 12710.0)) == processor.state.players["cairo"].resources[0]
        && processor.state.resourcePools[0] == 0;
        rewardDepletedResults += GetPassOrFail(passedFirst);

        //Succeed in splitting with multiple players.
        processor.state = JsonConvert.DeserializeObject<State>(savedState);
        processor.SubmitDepletedIslands("cairo", new List<string> { "a" });
        processor.SubmitDepletedIslands("pimpMacD", new List<string> { "e" });
        processor.SubmitDepletedIslands("nox", new List<string> { "j" });
        processor.RewardDepletedPool();
        double share = Math.Floor((12710.0 * 0.15 * 3.0) * 1.0 / 3.0) + (20000.0 - 12710.0);
        bool passedSecond = processor.state.players["cairo"].resources[0] == share && processor.state.players["pimpMacD"].resources[0] == share
        && processor.state.players["nox"].resources[0] == share && processor.state.resourcePools[0] == 0;
        rewardDepletedResults += GetPassOrFail(passedSecond);

        //Succeed in no reward with no players
        processor.state = JsonConvert.DeserializeObject<State>(savedState);
        processor.RewardDepletedPool();
        bool passedThird = processor.state.resourcePools[0] == 12710.0 * 0.15 * 3.0;
        rewardDepletedResults += GetPassOrFail(passedThird);

        //Succeed in bigger reward added after reward failed because no players.
        processor.PurchaseUnits("cairo", new List<int> { 1, 1, 1, 1, 1, 1, 1, 1, 1 });
        processor.PurchaseUnits("pimpMacD", new List<int> { 1, 1, 1, 1, 1, 1, 1, 1, 1 });
        processor.PurchaseUnits("nox", new List<int> { 1, 1, 1, 1, 1, 1, 1, 1, 1 });
        processor.SubmitDepletedIslands("cairo", new List<string> { "a" });
        processor.RewardDepletedPool();
        bool passedFourth = processor.state.resourcePools[0] == 0 
        && processor.state.players["cairo"].resources[0] == (15420.0 * 0.15 * 3.0) + (20000.0-15420.0);
        rewardDepletedResults += GetPassOrFail(passedFourth);
    }

    void RewardResourcePool(string savedState)
    {
        processor.state = JsonConvert.DeserializeObject<State>(savedState);
        rewardResourcePoolResults = "";
        double[] remainingPool = new double[] { processor.state.resourcePools[0], 0, 0, 0 };

        //Succeed in creating correct modifiers.
        double[] mods = processor.CalculateResourcePoolModifiers(new double[] { 1000, 2000, 4000 });
        bool passedFirst = mods[0] == 2.0 && mods[1] == 0.5 && mods[2] == 4.0 && mods[3] == 0.25 && mods[4] == 2.0 && mods[5] == 0.5;
        rewardResourcePoolResults += GetPassOrFail(passedFirst);

        //Succeed in rewarding single player all pool types.
        processor.SubmitResourcesToPool("cairo", new ResourceOrder(0, new List<double> {0.0, 1.0, 0.0}));
        processor.SubmitResourcesToPool("cairo", new ResourceOrder(1, new List<double> {1.0, 0.0, 0.0}));
        processor.SubmitResourcesToPool("cairo", new ResourceOrder(2, new List<double> {1.0, 0.0, 0.0}));
        processor.RewardResourcePools();
        double[] target = new double[] { processor.state.players["cairo"].resources[0], Math.Floor(7635.0 * 0.05 * 3.0 + 2.0) + (20000.0 - 7637.0) ,
        Math.Floor(6175.0 * 0.05 *3.0 + 1.0) + (20000.0 - 6176.0), Math.Floor(4100.0 * 0.05 * 3.0) + (20000.0 - 4100.0)};
        bool passedSecond = IsEqual(processor.state.players["cairo"].resources.ToArray(), target) && IsEqual(processor.state.resourcePools.ToArray(), remainingPool);
        rewardResourcePoolResults += GetPassOrFail(passedSecond);

        //Succeed in rewarding multiple players of all pool types.
        processor.state = JsonConvert.DeserializeObject<State>(savedState);
        processor.SubmitResourcesToPool("cairo", new ResourceOrder(0, new List<double> { 0.0, 1.0, 0.0 }));
        processor.SubmitResourcesToPool("cairo", new ResourceOrder(1, new List<double> { 1.0, 0.0, 0.0 }));
        processor.SubmitResourcesToPool("cairo", new ResourceOrder(2, new List<double> { 1.0, 0.0, 0.0 }));
        processor.SubmitResourcesToPool("pimpMacD", new ResourceOrder(0, new List<double> { 0.0, 1.0, 0.0 }));
        processor.SubmitResourcesToPool("pimpMacD", new ResourceOrder(1, new List<double> { 1.0, 0.0, 0.0 }));
        processor.SubmitResourcesToPool("pimpMacD", new ResourceOrder(2, new List<double> { 1.0, 0.0, 0.0 }));
        processor.SubmitResourcesToPool("nox", new ResourceOrder(0, new List<double> { 0.0, 1.0, 0.0 }));
        processor.SubmitResourcesToPool("nox", new ResourceOrder(1, new List<double> { 1.0, 0.0, 0.0 }));
        processor.SubmitResourcesToPool("nox", new ResourceOrder(2, new List<double> { 1.0, 0.0, 0.0 }));
        processor.RewardResourcePools();
        target = new double[] { processor.state.players["cairo"].resources[0], Math.Floor(7635.0 * 0.05) + (20000.0 - 7635.0),
        Math.Floor(6175.0 * 0.05) + (20000.0 - 6175.0), Math.Floor(4100.0 * 0.05) + (20000.0 - 4100.0)};
        double[] pimpTarget = new double[] { processor.state.players["pimpMacD"].resources[0], Math.Floor(7635.0 * 0.05) + (20000.0 - 7635.0),
        Math.Floor(6175.0 * 0.05) + (20000.0 - 6175.0), Math.Floor(4100.0 * 0.05) + (20000.0 - 4100.0)};
        double[] noxtarget = new double[] { processor.state.players["nox"].resources[0], Math.Floor(7635.0 * 0.05) + (20000.0 - 7635.0),
        Math.Floor(6175.0 * 0.05) + (20000.0 - 6175.0), Math.Floor(4100.0 * 0.05) + (20000.0 - 4100.0)};
        bool passedThird = IsEqual(processor.state.players["cairo"].resources.ToArray(), target) 
        && IsEqual(processor.state.players["pimpMacD"].resources.ToArray(), pimpTarget) && IsEqual(processor.state.players["nox"].resources.ToArray(), noxtarget)
        && IsEqual(processor.state.resourcePools.ToArray(), remainingPool); 
        rewardResourcePoolResults += GetPassOrFail(passedThird);

        //Succeed in rewarding multiple players with one pool type each only.
        processor.state = JsonConvert.DeserializeObject<State>(savedState);
        processor.SubmitResourcesToPool("cairo", new ResourceOrder(0, new List<double> { 0.0, 1.0, 0.0 }));
        processor.SubmitResourcesToPool("cairo", new ResourceOrder(1, new List<double> { 0.0, 0.0, 0.0 }));
        processor.SubmitResourcesToPool("cairo", new ResourceOrder(2, new List<double> { 0.0, 0.0, 0.0 }));
        processor.SubmitResourcesToPool("pimpMacD", new ResourceOrder(0, new List<double> { 0.0, 0.0, 0.0 }));
        processor.SubmitResourcesToPool("pimpMacD", new ResourceOrder(1, new List<double> { 1.0, 0.0, 0.0 }));
        processor.SubmitResourcesToPool("pimpMacD", new ResourceOrder(2, new List<double> { 0.0, 0.0, 0.0 }));
        processor.SubmitResourcesToPool("nox", new ResourceOrder(0, new List<double> { 0.0, 0.0, 0.0 }));
        processor.SubmitResourcesToPool("nox", new ResourceOrder(1, new List<double> { 0.0, 0.0, 0.0 }));
        processor.SubmitResourcesToPool("nox", new ResourceOrder(2, new List<double> { 1.0, 0.0, 0.0 }));

        target = processor.state.players["cairo"].resources.ToArray();
        target[1] += Math.Floor(7635.0 * 0.05 * 3.0 + 2.0);
        pimpTarget = processor.state.players["pimpMacD"].resources.ToArray();
        pimpTarget[2] += Math.Floor(6175.0 * 0.05 * 3.0 + 1.0);
        noxtarget = processor.state.players["nox"].resources.ToArray();
        noxtarget[3] += Math.Floor(4100.0 * 0.05 * 3.0);

        processor.RewardResourcePools();
        
        bool passedFourth = IsEqual(processor.state.players["cairo"].resources.ToArray(), target)
        && IsEqual(processor.state.players["pimpMacD"].resources.ToArray(), pimpTarget) && IsEqual(processor.state.players["nox"].resources.ToArray(), noxtarget)
        && IsEqual(processor.state.resourcePools.ToArray(), remainingPool);
        rewardResourcePoolResults += GetPassOrFail(passedFourth);

        //Succeed in rewarding single player one pool type.
        processor.state = JsonConvert.DeserializeObject<State>(savedState);
        processor.SubmitResourcesToPool("cairo", new ResourceOrder(0, new List<double> { 0.0, 1.0, 0.0 }));
        remainingPool = processor.state.resourcePools.ToArray();
        remainingPool[1] = 0.0;
        target = processor.state.players["cairo"].resources.ToArray();
        target[1] += Math.Floor(7635.0 * 0.05 * 3.0);
        processor.RewardResourcePools();
        bool passedFifth = IsEqual(processor.state.players["cairo"].resources.ToArray(), target) && IsEqual(processor.state.resourcePools.ToArray(), remainingPool);
        rewardResourcePoolResults += GetPassOrFail(passedFifth);

        //Succeed in rewarding no one and adding to pool.
        processor.PurchaseUnits("cairo", new List<int> { 1, 1, 1, 1, 1, 1, 1, 1, 1 });
        remainingPool[0] += 2710.0 * 0.15;
        remainingPool[1] += 1035.0 * 0.05;
        remainingPool[2] += 1175.0 * 0.05;
        target = processor.state.players["cairo"].resources.ToArray();
        processor.RewardResourcePools();
        bool passedSixth = IsEqual(processor.state.players["cairo"].resources.ToArray(), target) && IsEqual(processor.state.resourcePools.ToArray(), remainingPool);
        rewardResourcePoolResults += GetPassOrFail(passedSixth);
    }

    void AttackCycleTesting()
    {
        ResetTestData();
        attackTestingResults = "";
        string savedState = JsonConvert.SerializeObject(processor.state);
        int[][] goodPlanMin = new int[][] { new int[] { 0, 1, 2, 3, 7, 6 } };
        int[][] goodPlanMax = new int[][] { new int[] { 0, 1, 2, 3, 7, 6 }, new int[] { 11, 10, 9, 8, 4, 5 }, new int[] { 3, 6, 5, 4, 9, 10 } };
        int[][] tooLongPlan = new int[][] { new int[] { 0, 1, 2, 3, 7, 6 }, new int[] { 11, 10, 9, 8, 4, 5 }, new int[] { 8, 4, 1, 5, 10, 6 }, new int[] { 3, 6, 5, 4, 9, 10 } };
        int[][] notAdjacentPlan = new int[][] { new int[] { 0, 10, 2, 3, 7, 6 } };
        int[][] notLandablePlan = new int[][] { new int[] { 5, 6, 7, 3, 2, 1 } };
        int[][] goodForceMin = new int[][] { new int[] { 50, 25, 12, 0, 0, 0, 0, 0, 0 } };
        int[][] tinyForceMin = new int[][] { new int[] { 1, 0, 0, 0, 0, 0, 0, 0, 0 } };
        int[][] goodForceMax = new int[][] 
        {
            new int[] { 18, 9, 4, 0, 0, 0, 0, 0, 0 },
            new int[] { 16, 8, 4, 0, 0, 0, 0, 0, 0 },
            new int[] { 16, 8, 4, 0, 0, 0, 0, 0, 0 }
        };
        int[][] tooLongForce = new int[][]
        {
            new int[] { 12, 6, 3, 0, 0, 0, 0, 0, 0 },
            new int[] { 12, 6, 3, 0, 0, 0, 0, 0, 0 },
            new int[] { 12, 6, 3, 0, 0, 0, 0, 0, 0 },
            new int[] { 13, 7, 3, 0, 0, 0, 0, 0, 0 }
        };
        int[][] notEnoughTroops = new int[][] { new int[] { 100, 25, 12, 0, 0, 0, 0, 0, 0 } };
        int[][] badSquadSize = new int[][] { new int[] { 50, 25, 12, 0, 0, 0 } };

        //Fail because bad id
        State tempState = JsonConvert.DeserializeObject<State>(savedState);
        processor.AttackIsland("pimpMacD", new BattleCommand("", goodPlanMin, goodForceMin));
        bool passedFirst = IslandsAreEqual(processor.state.islands, tempState.islands) && PlayersAreEqualExcept("", processor.state.players, tempState.players)
        && DefensePlansAreEqual(processor.state.islands, tempState.islands);
        attackTestingResults += GetPassOrFail(passedFirst);

        //Fail because id doesn't exist
        processor.AttackIsland("pimpMacD", new BattleCommand("z", goodPlanMin, goodForceMin));
        bool passedSecond = IslandsAreEqual(processor.state.islands, tempState.islands) && PlayersAreEqualExcept("", processor.state.players, tempState.players)
        && DefensePlansAreEqual(processor.state.islands, tempState.islands);
        attackTestingResults += GetPassOrFail(passedSecond);

        //Fail because id belongs to self
        processor.AttackIsland("pimpMacD", new BattleCommand("e", goodPlanMin, goodForceMin));
        bool passedThird = IslandsAreEqual(processor.state.islands, tempState.islands) && PlayersAreEqualExcept("", processor.state.players, tempState.players)
        && DefensePlansAreEqual(processor.state.islands, tempState.islands);
        attackTestingResults += GetPassOrFail(passedThird);

        //Fail because plan was too short
        processor.AttackIsland("pimpMacD", new BattleCommand("a", new int[][] { }, new int[][] { }));
        bool passedFourth = IslandsAreEqual(processor.state.islands, tempState.islands) && PlayersAreEqualExcept("", processor.state.players, tempState.players)
        && DefensePlansAreEqual(processor.state.islands, tempState.islands);
        attackTestingResults += GetPassOrFail(passedFourth);

        //Fail because plan was too long
        processor.AttackIsland("pimpMacD", new BattleCommand("a", tooLongPlan, tooLongForce));
        bool passedFifth = IslandsAreEqual(processor.state.islands, tempState.islands) && PlayersAreEqualExcept("", processor.state.players, tempState.players)
        && DefensePlansAreEqual(processor.state.islands, tempState.islands);
        attackTestingResults += GetPassOrFail(passedFifth);

        //Fail because plan does not match squads
        processor.AttackIsland("pimpMacD", new BattleCommand("a", goodPlanMin, goodForceMax));
        bool passedSixth = IslandsAreEqual(processor.state.islands, tempState.islands) && PlayersAreEqualExcept("", processor.state.players, tempState.players)
        && DefensePlansAreEqual(processor.state.islands, tempState.islands);
        attackTestingResults += GetPassOrFail(passedSixth);

        //Fail because plan is not adjacent
        processor.AttackIsland("pimpMacD", new BattleCommand("a", notAdjacentPlan, goodForceMin));
        bool passedSeventh = IslandsAreEqual(processor.state.islands, tempState.islands) && PlayersAreEqualExcept("", processor.state.players, tempState.players)
        && DefensePlansAreEqual(processor.state.islands, tempState.islands);
        attackTestingResults += GetPassOrFail(passedSeventh);

        //Fail because units can not start in center
        processor.AttackIsland("pimpMacD", new BattleCommand("a", notLandablePlan, goodForceMin));
        bool passedEighth = IslandsAreEqual(processor.state.islands, tempState.islands) && PlayersAreEqualExcept("", processor.state.players, tempState.players)
        && DefensePlansAreEqual(processor.state.islands, tempState.islands);
        attackTestingResults += GetPassOrFail(passedEighth);

        //Fail because not enough units
        processor.AttackIsland("pimpMacD", new BattleCommand("a", goodPlanMin, notEnoughTroops));
        bool passedNinth = IslandsAreEqual(processor.state.islands, tempState.islands) && PlayersAreEqualExcept("", processor.state.players, tempState.players)
        && DefensePlansAreEqual(processor.state.islands, tempState.islands);
        attackTestingResults += GetPassOrFail(passedNinth);

        //Fail because malformed squad
        processor.AttackIsland("pimpMacD", new BattleCommand("a", goodPlanMin, badSquadSize));
        bool passedTenth = IslandsAreEqual(processor.state.islands, tempState.islands) && PlayersAreEqualExcept("", processor.state.players, tempState.players)
        && DefensePlansAreEqual(processor.state.islands, tempState.islands);
        attackTestingResults += GetPassOrFail(passedTenth);

        //Succeed in capturing undefended island
        tempState.islands["a"].owner = "pimpMacD";
        tempState.players["cairo"].islands.Remove("a");
        tempState.players["pimpMacD"].islands.Add("a");
        processor.AttackIsland("pimpMacD", new BattleCommand("a", goodPlanMin, goodForceMin));
        bool passedEleventh = IslandsAreEqual(processor.state.islands, tempState.islands) && PlayersAreEqualExcept("", processor.state.players, tempState.players)
        && DefensePlansAreEqual(processor.state.islands, tempState.islands);
        attackTestingResults += GetPassOrFail(passedEleventh);

        //Succeed in capturing defended island with minimum squad.
        ResetTestData();
        processor.state.islands["a"].squadPlans = new List<List<int>> { new List<int> { 5, 1, 2, 6, 4, 9, 10 } };
        processor.state.islands["a"].squadCounts = new List<List<int>> { new List<int> { 1, 0, 0, 0, 0, 0, 0, 0, 0 } };
        processor.AttackIsland("pimpMacD", new BattleCommand("a", goodPlanMin, goodForceMin));
        bool passedTwelfth = IslandsAreEqual(processor.state.islands, tempState.islands) && PlayersAreEqualExcept("", processor.state.players, tempState.players)
        && DefensePlansAreEqual(processor.state.islands, tempState.islands);
        attackTestingResults += GetPassOrFail(passedTwelfth);

        //Succeed in capturing defended island while landing with minimum squad.
        ResetTestData();
        processor.state.islands["a"].squadPlans = new List<List<int>> { new List<int> { 0, 1, 4 } };
        processor.state.islands["a"].squadCounts = new List<List<int>> { new List<int> { 1, 0, 0, 0, 0, 0, 0, 0, 0 } };
        processor.AttackIsland("pimpMacD", new BattleCommand("a", goodPlanMin, goodForceMin));
        bool passedThirteenth = IslandsAreEqual(processor.state.islands, tempState.islands) && PlayersAreEqualExcept("", processor.state.players, tempState.players)
        && DefensePlansAreEqual(processor.state.islands, tempState.islands);
        attackTestingResults += GetPassOrFail(passedThirteenth);

        //Fail in capturing defended island while landing with minimum squad.
        ResetTestData();
        processor.state.islands["a"].squadPlans = new List<List<int>> { new List<int> { 0, 1, 4 } };
        processor.state.islands["a"].squadCounts = new List<List<int>> { new List<int> { 100, 100, 100, 0, 0, 0, 0, 0, 0 } };
        tempState = JsonConvert.DeserializeObject<State>(JsonConvert.SerializeObject(processor.state));
        tempState.players["pimpMacD"].units[0]--;
        processor.AttackIsland("pimpMacD", new BattleCommand("a", goodPlanMin, tinyForceMin));
        bool passedFourteenth = IslandsAreEqual(processor.state.islands, tempState.islands) && PlayersAreEqualExcept("", processor.state.players, tempState.players)
        && DefensePlansAreEqual(processor.state.islands, tempState.islands);
        attackTestingResults += GetPassOrFail(passedFourteenth);

        //Fail in capturing defended island with minimum squad.
        ResetTestData();
        processor.state.islands["a"].squadPlans = new List<List<int>> { new List<int> { 5, 1, 2, 6, 4, 9, 10 } };
        processor.state.islands["a"].squadCounts = new List<List<int>> { new List<int> { 100, 100, 100, 0, 0, 0, 0, 0, 0 } };
        tempState = JsonConvert.DeserializeObject<State>(JsonConvert.SerializeObject(processor.state));
        tempState.players["pimpMacD"].units[0]--;
        processor.AttackIsland("pimpMacD", new BattleCommand("a", goodPlanMin, tinyForceMin));
        bool passedFifteenth = IslandsAreEqual(processor.state.islands, tempState.islands) && PlayersAreEqualExcept("", processor.state.players, tempState.players)
        && DefensePlansAreEqual(processor.state.islands, tempState.islands);
        attackTestingResults += GetPassOrFail(passedFifteenth);

        //Fail in capturing defended island with minimum squad because did not kill last squad.
        ResetTestData();
        processor.state.islands["a"].squadPlans = new List<List<int>> { new List<int> { 5 } };
        processor.state.islands["a"].squadCounts = new List<List<int>> { new List<int> { 1, 0, 0, 0, 0, 0, 0, 0, 0 } };
        tempState = JsonConvert.DeserializeObject<State>(JsonConvert.SerializeObject(processor.state));
        processor.AttackIsland("pimpMacD", new BattleCommand("a", goodPlanMin, goodForceMin));
        bool passedSixteenth = IslandsAreEqual(processor.state.islands, tempState.islands) && PlayersAreEqualExcept("", processor.state.players, tempState.players)
        && DefensePlansAreEqual(processor.state.islands, tempState.islands);
        attackTestingResults += GetPassOrFail(passedSixteenth);

        //Fail in capturing defended island with minimum squad because both teams died.
        ResetTestData();
        tempState = JsonConvert.DeserializeObject<State>(savedState);
        tempState.players["pimpMacD"].units[0]--;
        processor.state.islands["a"].squadPlans = new List<List<int>> { new List<int> { 0 } };
        processor.state.islands["a"].squadCounts = new List<List<int>> { new List<int> { 1, 0, 0, 0, 0, 0, 0, 0, 0 } };
        processor.AttackIsland("pimpMacD", new BattleCommand("a", goodPlanMin, tinyForceMin));
        bool passedSeventeenth = IslandsAreEqual(processor.state.islands, tempState.islands) && PlayersAreEqualExcept("", processor.state.players, tempState.players)
        && DefensePlansAreEqual(processor.state.islands, tempState.islands);
        attackTestingResults += GetPassOrFail(passedSeventeenth);

        //Succeed in capturing defended island with maximum squads vs multiple defenders.
        ResetTestData();
        tempState = JsonConvert.DeserializeObject<State>(savedState);
        tempState.islands["a"].owner = "pimpMacD";
        tempState.players["cairo"].islands.Remove("a");
        tempState.players["pimpMacD"].islands.Add("a");
        processor.state.islands["a"].squadPlans = new List<List<int>> { new List<int> { 1, 0, 4, 5, 2 }, new List<int> { 9, 8, 4, 5, 10 } };
        processor.state.islands["a"].squadCounts = new List<List<int>> { new List<int> { 1, 0, 0, 0, 0, 0, 0, 0, 0 }, new List<int> { 1, 0, 0, 0, 0, 0, 0, 0, 0 } };
        processor.AttackIsland("pimpMacD", new BattleCommand("a", goodPlanMax, goodForceMax));
        bool passedEigthteenth = IslandsAreEqual(processor.state.islands, tempState.islands) && PlayersAreEqualExcept("", processor.state.players, tempState.players)
        && DefensePlansAreEqual(processor.state.islands, tempState.islands);
        attackTestingResults += GetPassOrFail(passedEigthteenth);
    }

    void MovementBlockTesting()
    {
        ResetTestData();
        terrainTestingResults = "";
        processor.state.players["pimpMacD"].units = new List<double> { 100, 100, 100, 100, 100, 100, 100, 100, 100 };
        string savedState = JsonConvert.SerializeObject(processor.state);
        int[][] allTerrainTypesPlan = new int[][] { new int[] { 3, 6, 10, 9, 8, 4 } };
        int[][] dualAttack = new int[][] { new int[] { 1, 4, 8, 9, 5, 2 }, new int[] { 11, 10, 9, 9, 9, 9 } };
        int[][] tinyTroopForce = new int[][] { new int[] { 1, 0, 0, 0, 0, 0, 0, 0, 0 } };
        int[][] dualTinyTroopForce = new int[][] { new int[] { 1, 0, 0, 0, 0, 0, 0, 0, 0 }, new int[] { 1, 0, 0, 0, 0, 0, 0, 0, 0 } };
        int[][] troopsHeavyForce = new int[][] { new int[] { 100, 100, 100, 0, 0, 0, 0, 0, 0 } };
        int[][] tankHeavyForce = new int[][] { new int[] { 0, 0, 0, 100, 100, 100, 0, 0, 0 } };
        int[][] airHeavyForce = new int[][] { new int[] { 0, 0, 0, 0, 0, 0, 100, 100, 100 } };

        //Succeed in blocking troops with troop blocker
        State tempState = JsonConvert.DeserializeObject<State>(savedState);
        tempState.islands["a"].squadPlans = new List<List<int>> { new List<int> { 3 }, new List<int> { 6 } };
        tempState.islands["a"].squadCounts = new List<List<int>> { new List<int> { 1, 0, 0, 0, 0, 0, 0, 0, 0}, new List<int> { 1000, 1000, 1000, 0, 0, 0, 0, 0, 100 } };
        tempState.islands["a"].defenses = ")))0))))))))";
        processor.state = JsonConvert.DeserializeObject<State>(JsonConvert.SerializeObject(tempState));
        processor.AttackIsland("pimpMacD", new BattleCommand("a", allTerrainTypesPlan, troopsHeavyForce));
        bool passedFirst = IslandsAreEqual(processor.state.islands, tempState.islands) && PlayersAreEqualExcept("", processor.state.players, tempState.players)
        && DefensePlansAreEqual(processor.state.islands, tempState.islands);
        terrainTestingResults += GetPassOrFail(passedFirst);

        //Succeed in blocking tanks with tank blocker
        tempState = JsonConvert.DeserializeObject<State>(savedState);
        tempState.islands["a"].squadPlans = new List<List<int>> { new List<int> { 3 }, new List<int> { 10 } };
        tempState.islands["a"].squadCounts = new List<List<int>> { new List<int> { 1, 0, 0, 0, 0, 0, 0, 0, 0 }, new List<int> { 1000, 1000, 1000, 0, 0, 0, 0, 0, 100 } };
        tempState.islands["a"].defenses = ")))a))))))))";
        processor.state = JsonConvert.DeserializeObject<State>(JsonConvert.SerializeObject(tempState));
        processor.AttackIsland("pimpMacD", new BattleCommand("a", allTerrainTypesPlan, tankHeavyForce));
        bool passedSecond = IslandsAreEqual(processor.state.islands, tempState.islands) && PlayersAreEqualExcept("", processor.state.players, tempState.players)
        && DefensePlansAreEqual(processor.state.islands, tempState.islands);
        terrainTestingResults += GetPassOrFail(passedSecond);

        //Succeed in blocking aircraft with air blocker
        tempState = JsonConvert.DeserializeObject<State>(savedState);
        tempState.islands["a"].squadPlans = new List<List<int>> { new List<int> { 3 }, new List<int> { 6 } };
        tempState.islands["a"].squadCounts = new List<List<int>> { new List<int> { 1, 0, 0, 0, 0, 0, 0, 0, 0 }, new List<int> { 0, 0, 0, 0, 0, 0, 900, 900, 0 } };
        tempState.islands["a"].defenses = ")))A))))))))";
        processor.state = JsonConvert.DeserializeObject<State>(JsonConvert.SerializeObject(tempState));
        processor.AttackIsland("pimpMacD", new BattleCommand("a", allTerrainTypesPlan, airHeavyForce));
        bool passedThird = IslandsAreEqual(processor.state.islands, tempState.islands) && PlayersAreEqualExcept("", processor.state.players, tempState.players)
        && DefensePlansAreEqual(processor.state.islands, tempState.islands);
        terrainTestingResults += GetPassOrFail(passedThird);

        //Succeed in blocking tanks with lake feature
        tempState = JsonConvert.DeserializeObject<State>(savedState);
        tempState.islands["a"].squadPlans = new List<List<int>> { new List<int> { 6 }, new List<int> { 10 } };
        tempState.islands["a"].squadCounts = new List<List<int>> { new List<int> { 1, 0, 0, 0, 0, 0, 0, 0, 0 }, new List<int> { 0, 0, 0, 500, 500, 500, 0, 0, 0 } };
        processor.state = JsonConvert.DeserializeObject<State>(JsonConvert.SerializeObject(tempState));
        processor.AttackIsland("pimpMacD", new BattleCommand("a", allTerrainTypesPlan, tankHeavyForce));
        bool passedFourth = IslandsAreEqual(processor.state.islands, tempState.islands) && PlayersAreEqualExcept("", processor.state.players, tempState.players)
        && DefensePlansAreEqual(processor.state.islands, tempState.islands);
        terrainTestingResults += GetPassOrFail(passedFourth);

        //Succeed in blocking aircraft with mountain feature
        tempState = JsonConvert.DeserializeObject<State>(savedState);
        tempState.islands["a"].squadPlans = new List<List<int>> { new List<int> { 10 }, new List<int> { 9 } };
        tempState.islands["a"].squadCounts = new List<List<int>> { new List<int> { 1, 0, 0, 0, 0, 0, 0, 0, 0 }, new List<int> { 0, 500, 0, 0, 500, 0, 0, 500, 0 } };
        processor.state = JsonConvert.DeserializeObject<State>(JsonConvert.SerializeObject(tempState));
        processor.AttackIsland("pimpMacD", new BattleCommand("a", allTerrainTypesPlan, airHeavyForce));
        bool passedFifth = IslandsAreEqual(processor.state.islands, tempState.islands) && PlayersAreEqualExcept("", processor.state.players, tempState.players)
        && DefensePlansAreEqual(processor.state.islands, tempState.islands);
        terrainTestingResults += GetPassOrFail(passedFifth);

        //Succeed in blocking defender tanks with lake feature
        tempState = JsonConvert.DeserializeObject<State>(savedState);
        tempState.islands["a"].squadPlans = new List<List<int>> { new List<int> { 11, 6 } };
        tempState.islands["a"].squadCounts = new List<List<int>> { new List<int> { 0, 0, 0, 500, 500, 500, 0, 0, 0 }};
        processor.state = JsonConvert.DeserializeObject<State>(JsonConvert.SerializeObject(tempState));
        processor.state.islands["a"].defenses = "))))))))))a)";
        processor.AttackIsland("pimpMacD", new BattleCommand("a", allTerrainTypesPlan, tinyTroopForce));
        bool passedSixth = IslandsAreEqual(processor.state.islands, tempState.islands) && PlayersAreEqualExcept("", processor.state.players, tempState.players)
        && DefensePlansAreEqual(processor.state.islands, tempState.islands);
        terrainTestingResults += GetPassOrFail(passedSixth);

        //Succeed in blocking defender aircraft with mountain feature
        tempState = JsonConvert.DeserializeObject<State>(savedState);
        tempState.islands["a"].squadPlans = new List<List<int>> { new List<int> { 11, 10 } };
        tempState.islands["a"].squadCounts = new List<List<int>> { new List<int> { 0, 0, 0, 0, 0, 0, 500, 500, 500 } };
        processor.state = JsonConvert.DeserializeObject<State>(JsonConvert.SerializeObject(tempState));
        processor.state.islands["a"].defenses = ")))))))))a))";
        processor.AttackIsland("pimpMacD", new BattleCommand("a", allTerrainTypesPlan, tinyTroopForce));
        bool passedSeventh = IslandsAreEqual(processor.state.islands, tempState.islands) && PlayersAreEqualExcept("", processor.state.players, tempState.players)
        && DefensePlansAreEqual(processor.state.islands, tempState.islands);
        terrainTestingResults += GetPassOrFail(passedSeventh);

        //Fail in defender attacking unit in non adjacent react zone
        tempState = JsonConvert.DeserializeObject<State>(savedState);
        tempState.islands["a"].squadPlans = new List<List<int>> { new List<int> { 5, 1, 10 } };
        tempState.islands["a"].squadCounts = new List<List<int>> { new List<int> { 0, 0, 0, 500, 500, 500, 0, 0, 0 } };
        processor.state = JsonConvert.DeserializeObject<State>(JsonConvert.SerializeObject(tempState));
        tempState.players["pimpMacD"].units[0]--;
        processor.AttackIsland("pimpMacD", new BattleCommand("a", dualAttack, dualTinyTroopForce));
        bool passedEighth = IslandsAreEqual(processor.state.islands, tempState.islands) && PlayersAreEqualExcept("", processor.state.players, tempState.players)
        && DefensePlansAreEqual(processor.state.islands, tempState.islands);
        terrainTestingResults += GetPassOrFail(passedEighth);
    }


    void SetIslandResources()
    {
        foreach (KeyValuePair<string, Island> pair in processor.state.islands)
        {
            pair.Value.SetResources();
        }
    }

    bool Contains(List<string> container, List<string> requirements)
    {
        bool contains = container.Count >= requirements.Count;

        for (int r = 0; r < requirements.Count && contains; r++)
        {
            contains = container.Contains(requirements[r]);
        }

        return contains;
    }

    void ClearPlayers()
    {
        processor.state.players["cairo"].resources.Clear();
        processor.state.players["pimpMacD"].resources.Clear();
        processor.state.players["nox"].resources.Clear();
        processor.state.players["cairo"].resources = new List<double>() { 0, 0, 0, 0 };
        processor.state.players["pimpMacD"].resources = new List<double>() { 0, 0, 0, 0 };
        processor.state.players["nox"].resources = new List<double>() { 0, 0, 0, 0 };
        players["cairo"].resources.Clear();
        players["pimpMacD"].resources.Clear();
        players["nox"].resources.Clear();
    }

    void ClearCollectors()
    {
        foreach (KeyValuePair<string, Island> pair in processor.state.islands)
        {
            pair.Value.collectors = "000000000000";
        }
    }

    void ClearCollectors(Dictionary<string, Island> islands)
    {
        foreach (KeyValuePair<string, Island> pair in islands)
        {
            pair.Value.collectors = "000000000000";
        }
    }

    void ClearResources(List<List<double>> resources)
    {
        for (int t = 0; t < resources.Count; t++)
        {
            resources[t][0] = 0;
            resources[t][1] = 0;
            resources[t][2] = 0;
        }
    }

    void ResetTestData()
    {
        //Initialize Random Seed to get the same islands everytime.
        //Changing this seed will break a lot of the tests.
        UnityEngine.Random.InitState(1337);
        processor = new StateProcessor();

        players = new Dictionary<string, PlayerState>
        {
            {"cairo", new PlayerState("US", new double[9], new double[] {3000, 7500, 6000, 1500}, new string[] {IIDs[0], IIDs[1], IIDs[2], IIDs[3]}, "") },
            {"pimpMacD", new PlayerState("US", new double[] {50, 25, 12, 5, 2, 1, 5, 2, 1 }, new double[] {1000, 2500, 2000, 500}, new string[] { IIDs[4], IIDs[5], IIDs[6], IIDs[7], IIDs[8]}, "") },
            {"nox", new PlayerState("MX", new double[] {100, 50, 25, 10, 5, 2, 10, 5, 1 }, new double[] {0, 0, 0, 0}, new string[] { IIDs[9], IIDs[10], IIDs[11], IIDs[12], IIDs[13], IIDs[14]}, "") }
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

        processor.state = new State(players, islands, new Dictionary<string, List<List<double>>>(), new Dictionary<string, List<string>>());
    }

    double[] Add(double[] a, double[] b)
    {
        double[] c = new double[a.Length];

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

    double[] GetCostOfBunkers(int[] bunkerCounts)
    {
        double[] cost = new double[4];

        for (int b = 0; b < bunkerCounts.Length; b++)
        {
            cost[0] += bunkerCounts[b] * Constants.bunkerCosts[b, 0];
            cost[1] += bunkerCounts[b] * Constants.bunkerCosts[b, 1];
            cost[2] += bunkerCounts[b] * Constants.bunkerCosts[b, 2];
            cost[3] += bunkerCounts[b] * Constants.bunkerCosts[b, 3];
        }

        return cost;
    }

    double[] GetCostOfBlockers(int[] blockerCounts)
    {
        double[] cost = new double[4];

        for (int b = 0; b < blockerCounts.Length; b++)
        {
            cost[0] += blockerCounts[b] * Constants.blockerCosts[b, 0];
            cost[1] += blockerCounts[b] * Constants.blockerCosts[b, 1];
            cost[2] += blockerCounts[b] * Constants.blockerCosts[b, 2];
            cost[3] += blockerCounts[b] * Constants.blockerCosts[b, 3];
        }

        return cost;
    }

    double[] GetCostOfCollectors(int[] typeCount)
    {
        double[] finalPrice = new double[4];

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
        {
            passCount++;
            return "O ";
        }
        else
        {
            failCount++;
            return "X ";
        }
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

    bool ResourcesAreEqual(Dictionary<string, Island> a, Dictionary<string, Island> b)
    {
        bool equal = a.Count == b.Count;

        foreach (KeyValuePair<string, Island> pair in b)
        {
            equal = equal && a.ContainsKey(pair.Key) && a[pair.Key].resources.Count == pair.Value.resources.Count;

            if (equal)
            {
                for (int t = 0; t < 12 && equal; t++)
                {
                    for (int r = 0; r < 3 && equal; r++)
                    {
                        equal = equal && pair.Value.resources[t][r] == a[pair.Key].resources[t][r];
                    }
                }
            }
        }

        return equal;
    }

    bool IslandsAreEqual(Dictionary<string, Island> a, Dictionary<string, Island> b)
    {
        bool equal = a.Count == b.Count;

        foreach (KeyValuePair<string, Island> pair in b)
        {
            equal = equal && a.ContainsKey(pair.Key);

            if (equal)
                equal = equal && a[pair.Key].collectors == pair.Value.collectors
                && a[pair.Key].defenses == pair.Value.defenses && pair.Value.features == a[pair.Key].features
                && a[pair.Key].owner == pair.Value.owner;
        }

        return equal;
    }

    bool DefensePlansAreEqual(Dictionary<string, Island> a, Dictionary<string, Island> b)
    {
        bool equal = true;

        foreach (KeyValuePair<string, Island> pair in b)
        {
            equal = equal && a.ContainsKey(pair.Key);
            bool countsIsNull = a[pair.Key].squadCounts == null || pair.Value.squadCounts == null;
            bool plansIsNull =  a[pair.Key].squadPlans == null || pair.Value.squadPlans == null;

            if (equal && !countsIsNull)
                equal = equal && IsEqual(a[pair.Key].squadCounts, pair.Value.squadCounts);
            else if (equal && countsIsNull)
                equal = equal && a[pair.Key].squadCounts == pair.Value.squadCounts;

            if (equal && !plansIsNull)
                equal = equal && IsEqual(a[pair.Key].squadPlans, pair.Value.squadPlans);
            else if (equal && plansIsNull)
                equal = equal && a[pair.Key].squadPlans == pair.Value.squadPlans;
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

    bool IsEqual(List<List<double>> a, List<List<double>> b)
    {
        bool equal = a.Count == b.Count;

        for (int i = 0; i < a.Count && equal; i++)
        {
            equal = equal && a[i].Count == b[i].Count;

            for (int j = 0; j < a[i].Count && equal; j++)
            {
                equal = equal && a[i][j] == b[i][j];
            }
        }

        return equal;
    }

    bool IsEqual(List<List<int>> a, List<List<int>> b)
    {
        bool equal = a.Count == b.Count;

        for (int i = 0; i < a.Count && equal; i++)
        {
            equal = equal && a[i].Count == b[i].Count;

            for (int j = 0; j < a[i].Count && equal; j++)
            {
                equal = equal && a[i][j] == b[i][j];
            }
        }

        return equal;
    }

    bool IsEqual(double[] a, double[] b)
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
        "BuildBlocker       : ", "BuildBlonkers     : ", "UpdateDefenders: ", "UpdateResources: ", "SubmitDepleted  : ", "SubmitResources: ",
        "ResourceTransfer: ", "RewardDepleted  : ", "RewardResources: ", "AttackTests         : ", "MoveBlocking      : "};
        results = new string[] { nationResults, searchIslandResults, purchaseUnitResults, purchaseCollectorsResults, purchaseBunkerResults,
        purchaseBlockerResults, purchaseBlockerAndBunkerResults, defenseUpdateResults, resourceUpdateResults, depletedIslandResults, resourcePoolResults,
        transferResourcesResults, rewardDepletedResults, rewardResourcePoolResults, attackTestingResults, terrainTestingResults};
        
        for (int r = 0; r < results.Length; r++)
        {
            entireResults += label[r] + results[r] + "\n";
        }

        string testTotals = string.Format("\n- TestTotals - \nPasses : {0} \nFails    : {1} \nPercent: {2:00.00}%\n", passCount, failCount, ((float)passCount/(passCount+failCount))*100);

        return entireResults + testTotals;
    }
}

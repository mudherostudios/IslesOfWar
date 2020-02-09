using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IslesOfWar;
using IslesOfWar.ClientSide;
using IslesOfWar.Communication;
using IslesOfWar.GameStateProcessing;
using Newtonsoft.Json;

public class MarketOrderTesting : MonoBehaviour
{
    StateProcessor processor;
    MudHeroRandom random;
    Dictionary<string, PlayerState> players;
    Dictionary<string, Island> islands;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetState();
            RewardDepleted();
        }
    }

    void RewardPool()
    {
        processor.state.resourcePools = new List<double>() { 100, 100, 100 };
        processor.state.resourceContributions = new Dictionary<string, List<List<double>>>();
        ResourceOrder order = new ResourceOrder(1, new List<double>() { 10, 0, 10 });
        processor.SubmitResourcesToPool("cairo", order);
        order = new ResourceOrder(2, new List<double>() { 10, 10, 0 });
        processor.SubmitResourcesToPool("cairo", order);
        processor.RewardResourcePools();
    }

    void MarketAccept()
    {
        double[] forSell = new double[] { 1000, 0, 0, 0 };
        double[] buying = new double[] { 0, 1000, 0, 0 };
        MarketOrderAction order = new MarketOrderAction(forSell, buying);
        processor.OpenOrder("cairo", order, "12345678901234567890");
        processor.AcceptOrder("pimpMacD", "cairo", "12345678");
        PrintState();
        PrintList(processor.state.players["cairo"].resources);
    }

    void Drop()
    {
        AirDrop airDrop = new AirDrop();
        airDrop.amount = new double[] {100,100,100};
        airDrop.players = new string[] { "cairo" };
        airDrop.reason = "HAX :(";
        AdminCommands cmd = new AdminCommands();
        cmd.airDrop = airDrop;
        processor.ApplyAdminCommands(cmd);
    }

    void RewardDepleted()
    {
        Island depleted = new Island("cairo", "000000000000","000000000000","))))))))))))");
        depleted.SetResources(ref random, new int[,] { { 0,0}, {0,0 },{0,0 } } );
        processor.state.islands = new Dictionary<string, Island>();
        processor.state.islands.Add("12345678", depleted);
        processor.state.players["cairo"].islands.Add("12345678");
        processor.state.warbucksPool = 10000000;
        processor.SubmitDepletedIslands("cairo", new List<string>() { "12345678" });
        processor.RewardDepletedPool();
        Debug.Log(processor.state.players["cairo"].resources[0]);
    }

    void ResetState()
    {
        random = new MudHeroRandom(1337);

        players = new Dictionary<string, PlayerState>
        {
            {"cairo", new PlayerState("US", new double[9], new double[] {3000, 7500, 6000, 1500}, new string[] {}, "") },
            {"pimpMacD", new PlayerState("US", new double[] {50, 25, 12, 5, 2, 1, 5, 2, 1 }, new double[] {1000, 2500, 2000, 500}, new string[] {}, "") },
            {"nox", new PlayerState("MX", new double[] {100, 50, 25, 10, 5, 2, 10, 5, 1 }, new double[] {0, 0, 0, 0}, new string[] {}, "") }
        };

        processor = new StateProcessor(new State(players, islands));
        processor.state.currentConstants = new Constants();
    }

    void PrintState()
    {
        string jsonState = JsonConvert.SerializeObject(processor.state);
        Debug.Log(jsonState);
    }

    void PrintList(List<double> array)
    {
        string arrayValues = "";
        for (int i = 0; i < array.Count; i++)
        {
            arrayValues += i + ":" + array[i] + "\n";
        }

        Debug.Log(arrayValues);
    }
}

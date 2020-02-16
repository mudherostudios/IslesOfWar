using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IslesOfWar;
using IslesOfWar.ClientSide;
using IslesOfWar.Combat;
using IslesOfWar.Communication;
using IslesOfWar.GameStateProcessing;
using Newtonsoft.Json;

public class CombatTesting : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Fight();
        }
    }

    void Fight()
    {
        MudHeroRandom random = new MudHeroRandom(1337);
        double[] opforCounts = new double[] { 25, 0, 0, 0, 0, 0, 0, 0, 0 };
        double[] bluforCounts = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0 };
        Squad opfor = new Squad(opforCounts);
        Squad blufor = new Squad(bluforCounts);
        Engagement engagement = new Engagement(blufor, opfor, new Constants());
        EngagementHistory history = engagement.ResolveEngagement(ref random, new Constants());
        Debug.Log(history.winner);
        Debug.Log(history.remainingSquad.bunkers[0]);
    }
}

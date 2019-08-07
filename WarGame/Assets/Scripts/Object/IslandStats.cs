using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IslesOfWar.ClientSide;

public class IslandStats: MonoBehaviour
{
    public Transform[] hexTiles;
    public Transform[] fogs;
    public GameObject[] islandBadges;
    public Animator animator;
    public Island islandInfo;

    public int totalTiles
    {
        get { return hexTiles.Length; }
    }

    public void TurnOnIslandBadge(string player)
    {
        int type = 0;

        if (islandInfo.isDepleted())
            type = 2;
        else if (islandInfo.owner != player && islandInfo.owner != "")
            type = 1;

        islandBadges[type].SetActive(true);
    }
}

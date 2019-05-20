using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClientSide;
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

    public void TurnOnIslandBadge()
    {
        islandBadges[islandInfo.type].SetActive(true);
    }
}

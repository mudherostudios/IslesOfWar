using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClientSide;
public class IslandStats: MonoBehaviour
{
    public Transform[] hexTiles;
    public Transform[] fogs;
    public Animator animator;
    public Island islandInfo;
    public int layer = 0;
    private float lerpValue;

    public int totalTiles
    {
        get { return hexTiles.Length; }
    }
}

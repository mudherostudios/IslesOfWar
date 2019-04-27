using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandStats: MonoBehaviour
{
    public Transform[] hexTiles;
    public Animator animator;
    public int layer = 0;
    private float lerpValue;

    public int totalTiles
    {
        get { return hexTiles.Length; }
    }
}

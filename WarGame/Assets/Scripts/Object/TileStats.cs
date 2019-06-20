using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileStats : MonoBehaviour
{
    [Header("Single On/Of Objects")]
    public GameObject[] resourceParents;
    public GameObject[] collectorParents;
    public GameObject water;

    [Header("Probabilistic Quantity Objects")]
    public GameObject[] rocks;
    public GameObject[] vegetation;
    public GameObject[] structures;

    [Header("Probabilities")]
    public float[] rockProbabilities;
    public float[] vegetationProbabilities;
    public float[] structureProbabilities;

    [Header("Type Info")]
    public GameObject positionParent;
}

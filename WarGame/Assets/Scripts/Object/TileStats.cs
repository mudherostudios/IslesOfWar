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
    public GameObject indexParent;

    public void SetIndexParent(GameObject _indexParent)
    {
        indexParent = _indexParent;

        TilePurchasePrompter[] oilWells = GetChildren(resourceParents[0].transform);
        TilePurchasePrompter[] metalMines = GetChildren(resourceParents[1].transform);
        TilePurchasePrompter[] limeNodes = GetChildren(resourceParents[2].transform);

        for (int n = 0; n < oilWells.Length; n++)
        {
            oilWells[n].indexParent = indexParent;
            metalMines[n].indexParent = indexParent;
            limeNodes[n].indexParent = indexParent;
        }
    }

    TilePurchasePrompter[] GetChildren(Transform parent)
    {
        TilePurchasePrompter[] children = new TilePurchasePrompter[parent.childCount];

        for (int c = 0; c < children.Length; c++)
        {
            children[c] = parent.GetChild(c).gameObject.GetComponent<TilePurchasePrompter>();
        }

        return children;
    }
}

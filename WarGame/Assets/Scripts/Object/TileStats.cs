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

        CollectorPurchasePrompter[] oilWells = GetChildren(resourceParents[0].transform);
        CollectorPurchasePrompter[] metalMines = GetChildren(resourceParents[1].transform);
        CollectorPurchasePrompter[] limeNodes = GetChildren(resourceParents[2].transform);

        for (int n = 0; n < oilWells.Length; n++)
        {
            oilWells[n].indexParent = indexParent;
            metalMines[n].indexParent = indexParent;
            limeNodes[n].indexParent = indexParent;
        }
    }

    CollectorPurchasePrompter[] GetChildren(Transform parent)
    {
        CollectorPurchasePrompter[] children = new CollectorPurchasePrompter[parent.childCount];

        for (int c = 0; c < children.Length; c++)
        {
            children[c] = parent.GetChild(c).gameObject.GetComponent<CollectorPurchasePrompter>();
        }

        return children;
    }
}

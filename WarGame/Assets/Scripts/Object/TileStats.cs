using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileStats : MonoBehaviour
{
    [Header("Single On/Off Objects")]
    public GameObject[] resourceParents;
    public GameObject[] collectorParents;
    public GameObject[] bunkers;
    public GameObject[] blockers;
    public GameObject[] bunkerPrompters;
    public GameObject[] blockerPrompters;
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

        StructurePurchasePrompter[] oilWells = GetChildren(resourceParents[0].transform);
        StructurePurchasePrompter[] metalMines = GetChildren(resourceParents[1].transform);
        StructurePurchasePrompter[] limeNodes = GetChildren(resourceParents[2].transform);

        for (int n = 0; n < oilWells.Length; n++)
        {
            oilWells[n].indexParent = indexParent;
            metalMines[n].indexParent = indexParent;
            limeNodes[n].indexParent = indexParent;
        }
    }

    void ToggleAllCollection(bool on, GameObject[] collection)
    {
        if (!on)
        {
            foreach (GameObject parent in collection)
            {
                for (int c = 0; c < parent.transform.childCount; c++)
                {
                    parent.transform.GetChild(c).gameObject.SetActive(false);
                }
            }
        }
    }

    public void ToggleOffCollectors() { ToggleAllCollection(false, collectorParents); }
    public void ToggleOffResources() { ToggleAllCollection(false, resourceParents); }
    public void ToggleBunkers(bool on) { ToggleAllCollection(on, bunkers); }
    public void ToggleBlockers(bool on) { ToggleAllCollection(on, blockers); }
    public void ToggleBunkerPrompters(bool on) { ToggleAllCollection(on, bunkerPrompters); }
    public void ToggleBlockerPrompters(bool on) { ToggleAllCollection(on, blockerPrompters); }
    

    StructurePurchasePrompter[] GetChildren(Transform parent)
    {
        StructurePurchasePrompter[] children = new StructurePurchasePrompter[parent.childCount];

        for (int c = 0; c < children.Length; c++)
        {
            children[c] = parent.GetChild(c).gameObject.GetComponent<StructurePurchasePrompter>();
        }

        return children;
    }
}

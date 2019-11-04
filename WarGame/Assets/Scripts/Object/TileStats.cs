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

    private int usedPosition = -1;

    void ToggleAllCollection(bool on, bool toggleChildren, GameObject[] collection)
    {
        foreach (GameObject parent in collection)
        {
            for (int c = 0; c < parent.transform.childCount && toggleChildren; c++)
            {
                parent.transform.GetChild(c).gameObject.SetActive(on);
            }

            if (!toggleChildren)
                parent.SetActive(on);
        }
    }

    void ToggleOnValidCollection(bool toggleChildren, GameObject[] collection, int[] validIndices)
    {
        for (int c = 0; c < collection.Length; c++)
        {
            for (int t = 0; t < collection[c].transform.childCount && toggleChildren; t++)
            {
                if (validIndices[t] == 1)
                    collection[c].transform.GetChild(t).gameObject.SetActive(true);
            }

            if (!toggleChildren && validIndices[c] > 0)
                collection[c].SetActive(true);
        }
    }

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

        for (int p = 0; p < bunkerPrompters.Length; p++)
        {
            StructurePurchasePrompter prompter = bunkerPrompters[p].GetComponent<StructurePurchasePrompter>();
            prompter.indexParent = indexParent;
        }

        for (int p = 0; p < blockerPrompters.Length; p++)
        {
            StructurePurchasePrompter prompter = blockerPrompters[p].GetComponent<StructurePurchasePrompter>();
            prompter.indexParent = indexParent;
        }
    }

    public void ToggleOffCollectors() { ToggleAllCollection(false, true, collectorParents); }
    public void ToggleOffResources() { ToggleAllCollection(false, true, resourceParents); }
    public void ToggleBunkers(bool on) { ToggleAllCollection(on, false, bunkers); }
    public void ToggleBlockers(bool on) { ToggleAllCollection(on, false, blockers); }
    public void ToggleBunkerPrompters(bool on) { ToggleAllCollection(on, false, bunkerPrompters); }
    public void ToggleBlockerPrompters(bool on) { ToggleAllCollection(on, false, blockerPrompters); }

    public void ToggleBunkerPrompters(bool on, int[] validIndices)
    {
        for (int i = 0; i < validIndices.Length; i++)
        {
            if (on && validIndices[i] > 0)
                bunkerPrompters[i].SetActive(true);
            else if (on && validIndices[i] <= 0)
                bunkerPrompters[i].SetActive(false);
            else if (!on && validIndices[i] > 0) 
                bunkerPrompters[i].SetActive(false);
            else if (!on && validIndices[i] <= 0)
                bunkerPrompters[i].SetActive(true);
        }
    }

    public void ToggleBunkerSystem(bool on, int[] validIndices)
    {
        if (!on)
        {
            ToggleBunkers(false);
            ToggleBunkerPrompters(false);
        }
        else
        {
            int[] validPrompters = new int[validIndices.Length];

            for (int p = 0; p < validPrompters.Length; p++)
            {
                if (validIndices[p] == 0)
                    validPrompters[p] = 1;
            }

            ToggleOnValidCollection(false, bunkers, validIndices);
            ToggleOnValidCollection(false, bunkerPrompters, validPrompters);
        }
    }

    public void ToggleBlockerSystem(bool on, int type)
    {
        if (on)
        {
            if (type == 0)
                ToggleBlockerPrompters(true);
            else
                blockers[type-1].SetActive(true);
        }
        else
        {
            ToggleBlockers(false);
            ToggleBlockerPrompters(false);
        }
    }

    StructurePurchasePrompter[] GetChildren(Transform parent)
    {
        StructurePurchasePrompter[] children = new StructurePurchasePrompter[parent.childCount];

        for (int c = 0; c < children.Length; c++)
        {
            children[c] = parent.GetChild(c).gameObject.GetComponent<StructurePurchasePrompter>();
        }

        return children;
    }

    public Vector3[] GetSpawnPositions()
    {
        int selection = usedPosition;

        while (selection == usedPosition)
        {
            selection = Random.Range(0, 3);
        }

        Vector3 secondPosition = Vector3.zero;

        if (usedPosition != -1)
            secondPosition = transform.Find("Combat").GetChild(usedPosition).position;

        usedPosition = selection;

        return new Vector3[] { transform.Find("Combat").GetChild(selection).position, secondPosition };
    }
}

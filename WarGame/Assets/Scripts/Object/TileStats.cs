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
    public Transform spawnCollectionParent;

    private List<SquadMarker> squadMarkers;
    private int[] opponentSquadCounts = new int[2]; //Squads not unit counts. Checking for opfor and blufor squads on tile.

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

    public void SetMarkerOnTile(SquadMarker squadMarker, Vector3 offset)
    {
        if (squadMarkers == null)
            squadMarkers = new List<SquadMarker>();

        int owner = 0; //0 is players
        if (!squadMarker.isPlayers)
            owner = 1;
        opponentSquadCounts[owner]++;
        squadMarkers.Add(squadMarker);
        squadMarkers[squadMarkers.Count - 1].transform.position = spawnCollectionParent.GetChild(squadMarkers.Count - 1).position + offset;
        SetRotations();
    }

    public void MoveMarkerOffTile(SquadMarker squadMarker, Vector3 offset)
    {
        int owner = 0; //0 is players
        if (!squadMarker.isPlayers)
            owner = 1;
        squadMarkers.Remove(squadMarker);
        opponentSquadCounts[owner]--;

        for (int m = 0; m < squadMarkers.Count; m++)
        {
            squadMarkers[m].transform.position = spawnCollectionParent.GetChild(m).position + offset;
        }

        SetRotations();
    }

    public void SetRotations()
    {
        for (int m = 0; m < squadMarkers.Count; m++)
        {
            if (squadMarkers[m].displayType < 3 && squadMarkers[m].displayType > 5) // Exclude Tank Types
            {
                if (opponentSquadCounts[0] > 0 && opponentSquadCounts[1] > 0) //If there are two opposing forces
                {
                    FindEnemy(m);
                }
                else
                {
                    squadMarkers[m].transform.Rotate(Vector3.up, Random.value * 360);
                }
            }
            else // Tanks
            {
                squadMarkers[m].transform.rotation = spawnCollectionParent.GetChild(m).rotation;
            }
        }
    }

    private void FindEnemy(int squad)
    {
        for (int s = 0; s < squadMarkers.Count; s++)
        {
            if (!squadMarkers[s].isPlayers)
            {
                Vector3 target = squadMarkers[squad].transform.position - squadMarkers[s].transform.position;
                squadMarkers[squad].transform.rotation = Quaternion.Euler(Vector3.RotateTowards(squadMarkers[squad].transform.forward, target, 360.0f, 0.0f));
            }
        }
    }
}

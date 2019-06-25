using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClientSide;

public class IslandDiscoveryInteraction : Interaction
{
    [Header("Prefabs")]
    public GameObject[] tilePrefabs;
    public GameObject tileHolderPrefab;
    public Vector3 offset;

    [Header("Discovery Generation")]
    public string[] tileVariations;
    public Transform[] spawnPositions;
    public Vector3 maxPositionAdjustment;
    public Transform threeIslandObservationPoint;
    public Transform fiveIslandObservationPoint;
    public Transform threeIslandFocus;
    public Transform fiveIslandFocus;

    private Island[] discoveredIslands;
    private Island selectedDiscoveredIsland;
    private GameObject[] discoveredIslandObjects;

    private void Update()
    {
        WorldButtonCheck(Input.GetButtonDown("Fire1"));
    }

    public void SetGenerationVariables(GameObject[] prefabs, string[] _tileVariations, Vector3 _offset, Transform[] spawns, Vector3 _maxPositionAdjustment)
    {
        tileHolderPrefab = prefabs[prefabs.Length - 1];
        tilePrefabs = new GameObject[prefabs.Length];

        for (int p = 0; p < prefabs.Length - 1; p++)
        {
            tilePrefabs[p] = prefabs[p];
        }

        tileVariations = _tileVariations;
        offset = _offset;

        spawnPositions = spawns;
    }

    public void SetObservationPoints(Transform[] observationPoints)
    {
        threeIslandObservationPoint = observationPoints[0];
        fiveIslandObservationPoint = observationPoints[1];
        threeIslandFocus = observationPoints[2];
        fiveIslandFocus = observationPoints[3];
    }

    public void PlaceTiles(Island island, IslandStats islandStats, Transform tileParent)
    {
        IslandStats parent = tileParent.GetComponent<IslandStats>();
        
        parent.fogs[0].eulerAngles = new Vector3(0, Random.value * 360, 0);
        parent.fogs[1].eulerAngles = new Vector3(0, Random.value * 360, 0);
        parent.fogs[0].gameObject.SetActive(true);
        parent.fogs[1].gameObject.SetActive(true);
        
        tileParent.GetComponent<IslandStats>().islandInfo = island;

        for (int h = 0; h < island.totalTiles; h++)
        {
            int r = Mathf.FloorToInt(Random.Range(0, 6));
            string featString = island.features[h].ToString();
            string collectorString = island.collectors[h].ToString();
            GameObject tempTile = null;

            if (tileVariations[0].Contains(featString))
            {
                tempTile = Instantiate(tilePrefabs[0], islandStats.hexTiles[h].position + offset, tileParent.rotation);
            }
            else if (tileVariations[1].Contains(featString))
            {
                tempTile = Instantiate(tilePrefabs[1], islandStats.hexTiles[h].position + offset, tileParent.rotation);
            }
            else if (tileVariations[2].Contains(featString))
            {
                tempTile = Instantiate(tilePrefabs[2], islandStats.hexTiles[h].position + offset, tileParent.rotation);
            }

            tempTile.transform.Rotate(Vector3.up, 60 * r);
            tempTile.transform.SetParent(tileParent);

            if (tempTile.GetComponent<TileStats>().water != null)
                tempTile.GetComponent<TileStats>().water.SetActive(false);

            TileStats tempStats = tempTile.GetComponent<TileStats>();
            TurnOnDetails(tempStats.rocks, tempStats.rockProbabilities);
            TurnOnDetails(tempStats.vegetation, tempStats.vegetationProbabilities);

            float tempStructureProb = 0;

            if (tempStats.structureProbabilities.Length != tempStats.structures.Length && tempStats.structureProbabilities != null)
                tempStructureProb = tempStats.structureProbabilities[0];

            ActivateRandomObject(tempStats.structures, tempStructureProb);
        }
    }

    void TurnOnDetails(GameObject[] details, float[] detailProbs)
    {
        if (details != null)
        {
            for (int d = 0; d < details.Length; d++)
            {
                float threshold = Random.value;

                if (threshold <= detailProbs[d])
                    details[d].SetActive(true);
            }
        }
    }

    void ActivateRandomObject(GameObject[] objects, float noneProbability)
    {
        if (objects != null)
        {
            int paddedTotal = objects.Length;

            if (noneProbability > 0)
                paddedTotal = (int)((float)objects.Length / noneProbability);

            int r = (int)Mathf.Floor(Random.value * paddedTotal);
            if (r < objects.Length && r >= 0)
                objects[r].SetActive(true);
        }
    }

    void DiscoverRandomIslands(int missionType)
    {
        if (missionType == 0)
        {
            FakeIslandJson islandData = stateMaster.SendIslandDiscoveryRequest(3);

            if (islandData.success)
            {
                discoveredIslands = islandData.islands;
                orbital.ExploreMode(threeIslandObservationPoint, false);
                orbital.SetNewObservePoint(threeIslandObservationPoint, threeIslandFocus);
            }
        }
        else if (missionType == 1)
        {
            FakeIslandJson islandData = stateMaster.SendIslandDiscoveryRequest(5);

            if (islandData.success)
            {
                discoveredIslands = islandData.islands;
                orbital.ExploreMode(fiveIslandObservationPoint, false);
                orbital.SetNewObservePoint(fiveIslandObservationPoint, fiveIslandFocus);
            }
        }
    }

    public void GenerateDiscoveryIslands(int missionType)
    {
        //Talks to Server
        DiscoverRandomIslands(missionType);

        if (missionType >= 0)
        {
            int count = 1;

            if (missionType == 0)
                count = 3;
            else if (missionType == 1)
                count = 5;

            int discoveredIndex = 0;
            discoveredIslandObjects = new GameObject[count];

            for (int i = 0; i < count; i++, discoveredIndex++)
            {
                Vector3 pos = spawnPositions[i].position + Vector3.Scale(maxPositionAdjustment, new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f)));
                Vector3 rot = new Vector3(0, Random.value * 360, 0);
                discoveredIslandObjects[discoveredIndex] = Instantiate(tileHolderPrefab, pos, Quaternion.Euler(rot.x, rot.y, rot.z));
                IslandStats stats = discoveredIslandObjects[discoveredIndex].transform.GetComponent<IslandStats>();
                stats.animator.enabled = false;
                stats.islandInfo = discoveredIslands[discoveredIndex];
                stats.TurnOnIslandBadge();
                PlaceTiles(discoveredIslands[discoveredIndex], stats, discoveredIslandObjects[discoveredIndex].transform);
                
            }
        }
    }

    public void RemoveIslands()
    {
        if (discoveredIslandObjects != null)
        {
            for (int i = 0; i < discoveredIslandObjects.Length; i++)
            {
                Destroy(discoveredIslandObjects[i]);
            }
        }

        discoveredIslandObjects = null;
    }

    /*
    public void SubmitSelectedIsland()
    {
        if (stateMaster.SendDiscoveredIslandSelection(selectedDiscoveredIsland))
        {
            islands = stateMaster.playerState.islands;
            islandCount = islands.Length;
        }
    }*/
}

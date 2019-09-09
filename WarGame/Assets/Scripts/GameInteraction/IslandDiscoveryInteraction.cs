using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IslesOfWar.ClientSide;

public class IslandDiscoveryInteraction : Interaction
{
    [Header("Prefabs")]
    public GameObject[] tilePrefabs;
    public GameObject tileHolderPrefab;
    public Vector3 offset;

    [Header("Discovery Generation")]
    public string[] tileVariations;
    public Transform spawnPosition;
    public Vector3 maxPositionAdjustment;
    public Transform newIslandObservePoint;
    public Transform newIslandFocus;

    private Island discoveredIsland;
    private Island selectedDiscoveredIsland;
    private GameObject discoveredIslandObject;

    private void Update()
    {
        WorldButtonCheck(Input.GetButtonDown("Fire1"));
    }

    public void SetGenerationVariables(GameObject[] prefabs, string[] _tileVariations, Vector3 _offset, Transform spawn, Vector3 _maxPositionAdjustment)
    {
        tileHolderPrefab = prefabs[prefabs.Length - 1];
        tilePrefabs = new GameObject[prefabs.Length];

        for (int p = 0; p < prefabs.Length - 1; p++)
        {
            tilePrefabs[p] = prefabs[p];
        }

        tileVariations = _tileVariations;
        offset = _offset;

        spawnPosition = spawn;
    }

    public void SetObservationPoints(Transform[] observationPoints)
    {
        newIslandObservePoint = observationPoints[0];
        newIslandFocus = observationPoints[1];
    }

    public void PlaceTiles(Island island, IslandStats islandStats, Transform tileParent)
    {
        IslandStats parent = tileParent.GetComponent<IslandStats>();
        
        parent.fogs[0].eulerAngles = new Vector3(0, Random.value * 360, 0);
        parent.fogs[1].eulerAngles = new Vector3(0, Random.value * 360, 0);
        parent.fogs[0].gameObject.SetActive(true);
        parent.fogs[1].gameObject.SetActive(true);
        
        tileParent.GetComponent<IslandStats>().islandInfo = island;

        for (int h = 0; h < island.features.Length; h++)
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

    void DiscoverRandomIslands()
    {
        bool success = clientInterface.SearchIslands();
    }

    //Remove. Only send command. Game State Processor must generate. Just view the one that it generates. 
    public void GenerateDiscoveryIslands()
    {
        Debug.Log("Remember to make this use a reliable random with seed.");
        DiscoverRandomIslands();
        discoveredIslandObject = new GameObject();

        Vector3 pos = spawnPosition.position + Vector3.Scale(maxPositionAdjustment, new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f)));
        Vector3 rot = new Vector3(0, Random.value * 360, 0);
        discoveredIslandObject = Instantiate(tileHolderPrefab, pos, Quaternion.Euler(rot.x, rot.y, rot.z));
        IslandStats stats = discoveredIslandObject.transform.GetComponent<IslandStats>();
        stats.animator.enabled = false;
        stats.islandInfo = discoveredIsland;
        stats.TurnOnIslandBadge(clientInterface.player);
        PlaceTiles(discoveredIsland, stats, discoveredIslandObject.transform);
    }

    public void RemoveIslands()
    {
        if (discoveredIslandObject != null)
        {
            Destroy(discoveredIslandObject);
        }

        discoveredIslandObject = null;
    }
}

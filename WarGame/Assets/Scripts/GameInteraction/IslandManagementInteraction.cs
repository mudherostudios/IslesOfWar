using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClientSide;

public class IslandManagementInteraction: Interaction
{
    public Island[] islands;
    public string[] tileVariations;
    
    public Transform defaultObservePoint;
    public Transform defaultObservationFocus;

    [Header("Prefabs")]
    public GameObject[] tilePrefabs;
    public GameObject tileHolderPrefab;
    public Vector3 offset;

    [Header("Main Generation")]
    public Vector3 generateCenter;
    public Vector3 genereateRotation;
    public float islandChangeSpeed;
    public Vector3 deleteLeft;
    public Vector3 deleteRight;

    private int islandIndex, islandCount;
    private GameObject currentIsland, bufferedIsland;
    private IslandStats currentStats, bufferedStats;
    private int direction;

    public void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if (selectedButton != null && !selectedButton.gameObject.activeSelf && PeekButtonType() == "CollectorReveal")
                selectedButton.gameObject.SetActive(true);
            
            WorldButtonCheck();
            CheckNodeSelection();
        }

        if (direction != 0)
        {
            float leftDist = Vector3.Distance(currentIsland.transform.position, deleteLeft);
            float rightDist = Vector3.Distance(currentIsland.transform.position, deleteRight);

            if (leftDist <= 1 || rightDist <= 1)
            {
                Destroy(currentIsland);
                currentIsland = bufferedIsland;
                currentStats = bufferedStats;
                bufferedIsland = null;
                bufferedStats = null;
                direction = 0;
            }
        }
    }

    void CheckNodeSelection()
    {
        string peekedType = PeekButtonType();

        if (selectedWorldUI != null && peekedType == "CollectorReveal")
        {
            if (selectedWorldUIObject.gameObject.activeSelf)
            {
                selectedWorldUIObject.gameObject.SetActive(false);
            }
            else
            {
                selectedWorldUIObject.gameObject.SetActive(true);
                selectedButton.gameObject.SetActive(false);
            }
        }
    }

    public void GotoObservationPoint()
    {
        orbital.ExploreMode(defaultObservePoint, false);
        orbital.SetNewObservePoint(defaultObservePoint, defaultObservationFocus);
    }

    public void SetGenerationVariables(GameObject[] prefabs, string[] _tileVariations, Vector3 _offset, Vector3[] positional, float changeSpeed)
    {
        tileHolderPrefab = prefabs[prefabs.Length - 1];
        tilePrefabs = new GameObject[prefabs.Length];

        for (int p = 0; p < prefabs.Length - 1; p++)
        {
            tilePrefabs[p] = prefabs[p];
        }
        
        tileVariations = _tileVariations;
        offset = _offset;

        generateCenter = positional[0];
        genereateRotation = positional[1];
        deleteLeft = positional[2];
        deleteRight = positional[3];
        islandChangeSpeed = changeSpeed;
    }

    public void TurnOnIsland()
    {
        islandIndex = 0;
        currentIsland = Instantiate(tileHolderPrefab, generateCenter, Quaternion.Euler(genereateRotation));
        currentStats = currentIsland.GetComponent<IslandStats>();
        PlaceTiles(islands[islandIndex], currentStats, currentIsland.transform);
    }

    public void TurnOffIsland()
    {
        Destroy(currentIsland);
        currentIsland = null;
        currentStats = null;
    }

    public void SetObservationPoints(Transform observe, Transform focus)
    {
        defaultObservePoint = observe;
        defaultObservationFocus = focus;
    }

    public void IncrementIslandIndex(int increment)
    {
        if (bufferedStats == null)
        {
            direction = increment;

            if (direction > 0)
                bufferedIsland = Instantiate(tileHolderPrefab, generateCenter, Quaternion.Euler(genereateRotation));
            else if (direction < 0)
                bufferedIsland = Instantiate(tileHolderPrefab, generateCenter, Quaternion.Euler(genereateRotation));

            bufferedStats = bufferedIsland.GetComponent<IslandStats>();

            if (direction > 0)
            {
                bufferedStats.animator.SetLayerWeight(0, 0);
                bufferedStats.animator.SetLayerWeight(1, 1);
                bufferedStats.animator.SetTrigger("Right");
                currentStats.animator.SetTrigger("Right");
            }
            else if (direction < 0)
            {

                bufferedStats.animator.SetLayerWeight(0, 0);
                bufferedStats.animator.SetLayerWeight(2, 1);
                bufferedStats.animator.SetTrigger("Left");
                currentStats.animator.SetTrigger("Left");
            }

            int possibleIndexes = islandCount;
            int attackableIndexes = stateMaster.playerState.attackableIslands.Length;

            if (stateMaster.playerState.attackableIslands[attackableIndexes - 1].owner.username != null)
                possibleIndexes += attackableIndexes;

            if (islandIndex + increment >= possibleIndexes)
                islandIndex = 0;
            else if (islandIndex + increment < 0)
                islandIndex = possibleIndexes - 1;
            else
                islandIndex += increment;

            Island island = new Island();

            if (islandIndex < islandCount)
                island = islands[islandIndex];
            else
                island = stateMaster.playerState.attackableIslands[possibleIndexes - islandCount - 1];

            PlaceTiles(island, bufferedStats, bufferedIsland.transform);
        }
    }
    
    void PlaceTiles(Island island, IslandStats islandStats, Transform tileParent)
    {
        IslandStats parent = tileParent.GetComponent<IslandStats>();

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

            TileStats tempStats = tempTile.GetComponent<TileStats>();
            TurnOnResourcesAndCollectors(tempStats.resourceParents, tempStats.collectorParents, featString, collectorString);
            TurnOnDetails(tempStats.rocks, tempStats.rockProbabilities);
            TurnOnDetails(tempStats.vegetation, tempStats.vegetationProbabilities);

            float tempStructureProb = 0;

            if (tempStats.structureProbabilities.Length != tempStats.structures.Length && tempStats.structureProbabilities != null)
                tempStructureProb = tempStats.structureProbabilities[0];

            ActivateRandomObject(tempStats.structures, tempStructureProb);
        }
    }

    void TurnOnResourcesAndCollectors(GameObject[] resources, GameObject[] collectors, string type, string built)
    {
        int r = GetConvertedType(type);
        int c = GetConvertedType(built);
        GameObject resourceObject = null;

        if (r == 1 || r == 4 || r == 5 || r == 7)
        {
            resourceObject = resources[0];

            if (c == 1 || c == 4 || c == 5 || c == 7)
                resourceObject = collectors[0];

            ActivateRandomChild(resourceObject.transform);
        }

        if (r == 2 || r == 4 || r == 6 || r == 7)
        {
            resourceObject = resources[1];

            if (c == 2 || c == 4 || c == 6 || c == 7)
                resourceObject = collectors[1];

            ActivateRandomChild(resourceObject.transform);
        }

        if (r == 3 || r == 5 || r == 6 || r == 7)
        {
            resourceObject = resources[2];

            if (c == 3 || c == 5 || c == 6 || c == 7)
                resourceObject = collectors[2];

            ActivateRandomChild(resourceObject.transform);
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

    void ActivateRandomChild(Transform collection)
    {
        int r = (int)Mathf.Floor(Random.value * collection.childCount);
        collection.GetChild(r).gameObject.SetActive(true);
    }

    void ActivateRandomObject(GameObject[] objects, float noneProbability)
    {
        if (objects != null)
        {
            int paddedTotal = objects.Length;

            if (noneProbability > 0)
                paddedTotal = (int)((float)objects.Length / noneProbability);

            int r = (int)Mathf.Floor(Random.value * paddedTotal);
            if(r < objects.Length && r >= 0)
                objects[r].SetActive(true);
        }
    }

    int GetConvertedType(string type)
    {
        int converted = -1;

        if ("0aA".Contains(type))
            converted = 0;
        else if ("1bB".Contains(type))
            converted = 1;
        else if ("2cC".Contains(type))
            converted = 2;
        else if ("3dD".Contains(type))
            converted = 3;
        else if ("4eE".Contains(type))
            converted = 4;
        else if ("5fF".Contains(type))
            converted = 5;
        else if ("6gG".Contains(type))
            converted = 6;
        else if ("7hH".Contains(type))
            converted = 7;

        return converted;
    }

    public void Initialize()
    {
        islands = stateMaster.playerState.islands;
        islandIndex = 0;
        islandCount = islands.Length;
        direction = 0;
    }
}

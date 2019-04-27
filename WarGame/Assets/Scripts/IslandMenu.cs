using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClientSide;
using ServerSide;

public class IslandMenu : MonoBehaviour
{
    //0 = hills
    //1 = lake
    //2 = mountain
    public Island[] islands;
    public string[] tileVariations;

    [Header ("Prefabs")]
    public GameObject[] tilePrefabs;
    public GameObject tileHolderPrefab;
    public Vector3 offset;

    [Header("Instantiation")]
    public Vector3 generateCenter;
    public Vector3 genereateRotation;
    public int islandCount;
    public float islandChangeSpeed;

    [Header("Deletion")]
    public Vector3 deleteLeft;
    public Vector3 deleteRight;

    
    private int islandIndex;
    private GameObject currentIsland, bufferedIsland;
    private IslandStats currentStats, bufferedStats;
    private int direction;
    

    public void Start()
    {
        islandIndex = 0;
        direction = 0;

        islands = GetRandomIslands(islandCount);

        currentIsland = Instantiate(tileHolderPrefab, generateCenter, Quaternion.Euler(genereateRotation));
        currentStats = currentIsland.GetComponent<IslandStats>();
        PlaceTiles(islands[islandIndex], currentStats, currentIsland.transform);
    }

    public void Update()
    {
        float leftDist = Vector3.Distance(currentIsland.transform.position, deleteLeft);
        float rightDist = Vector3.Distance(currentIsland.transform.position, deleteRight);
        if ( leftDist <= 1 || rightDist <= 1)
        {
            Destroy(currentIsland);
            currentIsland = bufferedIsland;
            currentStats = bufferedStats;
            bufferedIsland = null;
            bufferedStats = null;
        }
    }

    public void RefreshIsland(int increment)
    {
        if (bufferedStats == null)
        {
            direction = increment;

            if (direction > 0)
                bufferedIsland = Instantiate(tileHolderPrefab, generateCenter, Quaternion.Euler(genereateRotation));
            else if (direction < 0)
                bufferedIsland = Instantiate(tileHolderPrefab, generateCenter, Quaternion.Euler(genereateRotation));

            bufferedStats = bufferedIsland.GetComponent<IslandStats>();

            if(direction > 0)
            {
                bufferedStats.animator.SetLayerWeight(0, 0);
                bufferedStats.animator.SetLayerWeight(1, 1);
                bufferedStats.animator.SetTrigger("Right");
                currentStats.animator.SetTrigger("Right");
            }
            else if(direction < 0)
            {

                bufferedStats.animator.SetLayerWeight(0, 0);
                bufferedStats.animator.SetLayerWeight(2, 1);
                bufferedStats.animator.SetTrigger("Left");
                currentStats.animator.SetTrigger("Left");
            }

            if (islandIndex + increment >= islandCount)
                islandIndex = 0;
            else if (islandIndex + increment < 0)
                islandIndex = islandCount - 1;
            else
                islandIndex += increment;

            PlaceTiles(islands[islandIndex], bufferedStats, bufferedIsland.transform);
        }
    }

    public void PlaceTiles(Island island, IslandStats islandStats, Transform tileParent)
    {
        for (int h = 0; h < island.totalTiles; h++)
        {
            int r = Mathf.FloorToInt(Random.Range(0, 6));
            string featString = island.features[h].ToString();
            string collectorString = island.collectors[h].ToString();
            GameObject tempTile = null;

            if (tileVariations[0].Contains(featString))
            {
                tempTile = Instantiate(tilePrefabs[0], islandStats.hexTiles[h].position+offset, Quaternion.Euler(genereateRotation));
            }
            else if (tileVariations[1].Contains(featString))
            {
                tempTile = Instantiate(tilePrefabs[1], islandStats.hexTiles[h].position+offset, Quaternion.Euler(genereateRotation));
            }
            else if (tileVariations[2].Contains(featString))
            {
                tempTile = Instantiate(tilePrefabs[2], islandStats.hexTiles[h].position+offset, Quaternion.Euler(genereateRotation));
            }

            tempTile.transform.Rotate(Vector3.up, 60 * r);
            tempTile.transform.SetParent(tileParent);
            TurnOnResourcesAndCollectors(tempTile.GetComponent<TileStats>().resourceParents, tempTile.GetComponent<TileStats>().collectorParents, featString, collectorString);
        }
    }

    void TurnOnResourcesAndCollectors(GameObject[] resources, GameObject[] collectors, string type, string built)
    {
        int r = GetConvertedType(type);
        int c = GetConvertedType(built);

        if (r == 1 || r == 4 || r == 5  || r == 7)
        {
            GameObject tileObject = resources[0];

            if (c == 1 || c == 4 || c == 5 || c == 7)
                tileObject = collectors[0];

            ActivateRandomChild(tileObject.transform);
        }

        if (r == 2 || r == 4 || r == 6 || r == 7)
        {
            GameObject tileObject = resources[0];

            if (c == 2 || c == 4 || c == 6 || c == 7)
                tileObject = collectors[1];

            ActivateRandomChild(resources[1].transform);
        }

        if (r == 3 || r == 5 || r == 6 || r == 7)
        {
            GameObject tileObject = resources[0];

            if (c == 3 || c == 5 || c == 6 || c == 7)
                tileObject = collectors[2];

            ActivateRandomChild(resources[2].transform);
        }
    }

    void ActivateRandomChild(Transform collection)
    {
        int r = (int)Mathf.Floor(Random.value * collection.childCount);
        collection.GetChild(r).gameObject.SetActive(true);
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

    Island[] GetRandomIslands(int count)
    {
        IslandGenerator generator = new IslandGenerator();
        Island[] tempIslands = new Island[count];

        for(int i = 0; i < count; i++)
        {
            tempIslands[i] = new Island(generator.Generate(), "000000000000");
        }

        return tempIslands;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ServerSide;

public class IslandCreation : MonoBehaviour
{
    //0 = hills
    //1 = lake
    //2 = mountain
    public string islandFeatures = "000000000000";
    public string[] tileVariations;
    public GameObject[] tilePrefabs;
    public Transform[] hexTiles;
    public Vector3 offset;
    public bool isRandom;

    public void Start()
    {
        if (isRandom || islandFeatures.Length != 12)
            islandFeatures = GetRandomIsland();

        PlaceTiles(islandFeatures);
    }

    public void PlaceTiles(string features)
    {
        for (int h = 0; h < hexTiles.Length; h++)
        {
            int r = Mathf.FloorToInt(Random.Range(0, 6));
            GameObject tile = null;
            string featString = islandFeatures[h].ToString();

            if (tileVariations[0].Contains(featString))
            {
                tile = Instantiate(tilePrefabs[0], hexTiles[h].position+offset, Quaternion.identity);
            }
            else if (tileVariations[1].Contains(featString))
            {
                tile = Instantiate(tilePrefabs[1], hexTiles[h].position+offset, Quaternion.identity);
            }
            else if (tileVariations[2].Contains(featString))
            {
                tile = Instantiate(tilePrefabs[2], hexTiles[h].position+offset, Quaternion.identity);
            }

            tile.transform.Rotate(Vector3.up, 60 * r);
            TurnOnResources(tile, featString);
        }
    }

    void TurnOnResources(GameObject tile, string type)
    {
        int r = GetConvertedType(type);

        if (r == 1 || r == 4 || r == 5  || r == 7)
        {
            ActivateRandomChild(tile.GetComponent<TileStats>().resourceParents[0].transform);
        }

        if (r == 2 || r == 4 || r == 6 || r == 7)
        {
            ActivateRandomChild(tile.GetComponent<TileStats>().resourceParents[1].transform);
        }

        if (r == 3 || r == 5 || r == 6 || r == 7)
        {
            ActivateRandomChild(tile.GetComponent<TileStats>().resourceParents[2].transform);
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

    string GetRandomIsland()
    {
        IslandGenerator generator = new IslandGenerator();
        return generator.Generate();
    }
}

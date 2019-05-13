using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ClientSide;
using ServerSide;

public class IslandMenu : MonoBehaviour
{
    //0 = hills
    //1 = lake
    //2 = mountain
    public Island[] islands;
    public string[] tileVariations;

    public Camera cam;
    public OrbitalFocusCam orbital;
    public Transform defaultObservePoint;
    public Transform defaultObservationFocus;

    [Header("GUI Update")]
    public GameObject[] searchGUIElements;
    public GameObject[] exploreGUIElements;
    public GameObject[] discoveryGUIElements;
    public GameObject[] inspectGUIElements;
    public Text islandName;

    [Header ("Prefabs")]
    public GameObject[] tilePrefabs;
    public GameObject tileHolderPrefab;
    public Vector3 offset;

    [Header("Main Generation")]
    public Vector3 generateCenter;
    public Vector3 genereateRotation;
    public int islandCount;
    public float islandChangeSpeed;
    public Vector3 deleteLeft;
    public Vector3 deleteRight;

    [Header("Discovery Generation")]
    public Transform[] spawnPositions;
    public Vector3 maxPositionAdjustment;
    public Transform threeIslandObservationPoint;
    public Transform fiveIslandObservationPoint;
    public Transform threeIslandFocus;
    public Transform fiveIslandFocus;
    public string discoveryText;

    private int islandIndex;
    private GameObject currentIsland, bufferedIsland;
    private IslandStats currentStats, bufferedStats;
    private int direction;
    private Island[] discoveredIslands;
    private GameObject[] discoveredIslandObjects;
    StateMaster stateMaster;
    

    public void Start()
    {
        stateMaster = GameObject.FindGameObjectWithTag("StateMaster").GetComponent<StateMaster>();
        stateMaster.InitilializeConnection();
        stateMaster.GetState();

        islands = stateMaster.playerState.islands;
        islandIndex = 0;
        direction = 0;

        currentIsland = Instantiate(tileHolderPrefab, generateCenter, Quaternion.Euler(genereateRotation));
        currentStats = currentIsland.GetComponent<IslandStats>();
        PlaceTiles(islands[islandIndex], currentStats, currentIsland.transform, true);
    }

    public void Update()
    {
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
        else
        {
            if (Input.GetButtonUp("Fire1") && !orbital.exploring && !EventSystem.current.IsPointerOverGameObject())
            {
                RaycastHit hit;
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);

                bool didHit = Physics.Raycast(ray, out hit);

                if (didHit)
                {
                    if (hit.transform.tag == "WorldButton")
                    { 
                        WorldButton button = hit.transform.GetComponent<WorldButton>();

                        if (button.buttonType == "MenuRevealer")
                        {
                            button.logicParent.gameObject.SetActive(true);
                        }
                        else if (button.buttonType == "IslandTile")
                        {
                            orbital.ExploreMode(hit.transform, true);

                            if (orbital.observePoint == defaultObservePoint)
                            {
                                ToggleGUIElementsTo(searchGUIElements, false);
                                ToggleGUIElementsTo(exploreGUIElements, true);
                            }
                            else
                            {
                                ToggleGUIElementsTo(discoveryGUIElements, false);
                                ToggleGUIElementsTo(inspectGUIElements, true);
                            }
                        }
                    }
                }
            }
        }
    }

    public void IslandQueue(int type)
    {

        orbital.ExploreMode(defaultObservationFocus, false);

        if (type == 0)
        {
            ToggleGUIElementsTo(searchGUIElements, true);
            ToggleGUIElementsTo(exploreGUIElements, false);
        }
        else if (type == 1)
        {
            ToggleGUIElementsTo(discoveryGUIElements, true);
            ToggleGUIElementsTo(inspectGUIElements, false);
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
            
            PlaceTiles(islands[islandIndex], bufferedStats, bufferedIsland.transform, true);
        }
    }

    public void PlaceTiles(Island island, IslandStats islandStats, Transform tileParent, bool isKnown)
    {
        if(isKnown)
            islandName.text = island.name;

        for (int h = 0; h < island.totalTiles; h++)
        {
            int r = Mathf.FloorToInt(Random.Range(0, 6));
            string featString = island.features[h].ToString();
            string collectorString = island.collectors[h].ToString();
            GameObject tempTile = null;

            if (tileVariations[0].Contains(featString))
            {
                tempTile = Instantiate(tilePrefabs[0], islandStats.hexTiles[h].position+offset, tileParent.rotation);
            }
            else if (tileVariations[1].Contains(featString))
            {
                tempTile = Instantiate(tilePrefabs[1], islandStats.hexTiles[h].position+offset, tileParent.rotation);
            }
            else if (tileVariations[2].Contains(featString))
            {
                tempTile = Instantiate(tilePrefabs[2], islandStats.hexTiles[h].position+offset, tileParent.rotation);
            }

            tempTile.transform.Rotate(Vector3.up, 60 * r);
            tempTile.transform.SetParent(tileParent);

            if (isKnown)
            {
                TurnOnResourcesAndCollectors(tempTile.GetComponent<TileStats>().resourceParents, tempTile.GetComponent<TileStats>().collectorParents, featString, collectorString);
            }
            else
            {
                IslandStats parent = tileParent.GetComponent<IslandStats>();
                parent.fogs[0].eulerAngles = new Vector3(0, Random.value * 360, 0);
                parent.fogs[1].eulerAngles = new Vector3(0, Random.value * 360, 0);
                parent.fogs[0].gameObject.SetActive(true);
                parent.fogs[1].gameObject.SetActive(true);
                if(tempTile.GetComponent<TileStats>().water != null)
                    tempTile.GetComponent<TileStats>().water.SetActive(false);
            }
        }
    }

    void DiscoverRandomIslands(int missionType)
    {
        if (missionType >= 0)
        {
            currentIsland.SetActive(false);
            islandName.text = discoveryText;
        }

        if (missionType == 0)
        {
            FakeIslandJson islandData = stateMaster.SendIslandDiscoveryRequest(3);

            if (islandData.success)
            {
                discoveredIslands = islandData.islands;
                orbital.SetNewObservePoint(threeIslandObservationPoint, threeIslandFocus);
            }
        }
        else if (missionType == 1)
        {
            FakeIslandJson islandData = stateMaster.SendIslandDiscoveryRequest(5);

            if (islandData.success)
            {
                discoveredIslands = islandData.islands;
                orbital.SetNewObservePoint(fiveIslandObservationPoint, fiveIslandFocus);
            }
        }
        else if (missionType == -1)
        {
            orbital.SetNewObservePoint(defaultObservePoint, defaultObservationFocus);
            currentIsland.SetActive(true);
            islandName.text = islands[islandIndex].name;
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
                PlaceTiles(discoveredIslands[discoveredIndex], stats, discoveredIslandObjects[discoveredIndex].transform, false);
            }
        }
        
    }

    void TurnOnResourcesAndCollectors(GameObject[] resources, GameObject[] collectors, string type, string built)
    {
        int r = GetConvertedType(type);
        int c = GetConvertedType(built);
        GameObject resourceObject = null;

        if (r == 1 || r == 4 || r == 5  || r == 7)
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


    void ToggleGUIElementsTo(GameObject[] guiElements, bool active)
    {
        for (int e = 0; e < guiElements.Length; e++)
        {
            guiElements[e].SetActive(active);
        }
    }
}

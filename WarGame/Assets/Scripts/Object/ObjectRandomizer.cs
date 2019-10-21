using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRandomizer : MonoBehaviour
{
    public string objectTag;
    public float probabilityOn;
    public bool setOnStart = false;
    public Material[] materials;
    public bool changeMaterial = false;

    // Start is called before the first frame update
    void Start()
    {
        if (setOnStart)
        {
            GameObject[] tempObjects = GameObject.FindGameObjectsWithTag(objectTag);
            int[][] winnersAndLosers = GetWinnerAndLoserIndices(tempObjects);
            TurnOffObjects(tempObjects, winnersAndLosers[1]);

            if (changeMaterial)
                ChangeMaterials(tempObjects, winnersAndLosers[0]);
        }
    }

    public void TurnOffObjects(GameObject[] objects, int[] indicesToTurnOff)
    {
        for (int i = 0; i < indicesToTurnOff.Length; i++)
        {
            int winner = indicesToTurnOff[i];
            objects[winner].SetActive(false);
        }
    }

    public void ChangeMaterials(GameObject[] objects, int[] indicesToChange)
    {
        for (int i = 0; i < indicesToChange.Length; i++)
        {
            int choice = Random.Range(0, materials.Length);
            objects[indicesToChange[i]].GetComponent<Renderer>().material = materials[choice];
        }
    }

    int[][] GetWinnerAndLoserIndices(GameObject[] objects)
    {
        List<int> winners = new List<int>();
        List<int> losers = new List<int>();
        
        for (int o = 0; o < objects.Length; o++)
        {
            float choice = Random.value;

            if (choice <= probabilityOn)
                winners.Add(o);
            else
                losers.Add(o);
        }

        return new int[][] { winners.ToArray(), losers.ToArray() };
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRandomizer : MonoBehaviour
{
    public string objectTag;
    public float probabilityOn;
    public bool setOnStart = false;

    // Start is called before the first frame update
    void Start()
    {
        if (setOnStart)
        {
            GameObject[] tempObjects = GameObject.FindGameObjectsWithTag(objectTag);
            TurnOffObjects(tempObjects);
        }
    }

    public void TurnOffObjects(GameObject[] objects)
    {
        int[] winners = GetLosingIndices(objects);
        for (int w = 0; w < winners.Length; w++)
        {
            int winner = winners[w];
            objects[winner].SetActive(false);
        }
    }

    int[] GetLosingIndices(GameObject[] objects)
    {
        List<int> winners = new List<int>();
        int totalPossibleWinners = Mathf.CeilToInt((1.0f-probabilityOn) * objects.Length);
        int breakerCount = 0;

        for (int o = 0; o < totalPossibleWinners; o++, breakerCount++)
        {
            if (breakerCount > objects.Length)
                o = totalPossibleWinners * 2;

            int winner = Random.Range(0, objects.Length - 1);

            if (winners.Contains(winner))
            {
                o -= 1;
            }
            else
            {
                winners.Add(winner);
            }
        }

        return winners.ToArray();
    }
}

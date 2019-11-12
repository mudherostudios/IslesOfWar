using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPrompter : MonoBehaviour
{
    public Tutorial tutorial;
    public string tutorialName;
    public GameObject[] worldIndicators;
    public GameObject[] messages;
    private int lastMessage = -1;
    private int orderIndex = -1;
    private bool hasBeenInitiated = false;

    public void Start()
    {
        tutorial = GameObject.Find("MasterTutorial").GetComponent<Tutorial>();
    }

    public void InitiateTutorial()
    {
        if (!hasBeenInitiated)
        {
            if (worldIndicators != null)
            {
                for (int i = 0; i < worldIndicators.Length; i++)
                {
                    worldIndicators[i].SetActive(true);
                }
            }

            NextTutorial();
            hasBeenInitiated = true;
        }
    }

    public void NextTutorial()
    {
        orderIndex++;
        if (orderIndex < messages.Length)
            ShowTutorial(orderIndex);
        else
            CompleteTutorial();
    }

    public void CompleteTutorial()
    {
        if (worldIndicators != null)
        {
            for (int i = 0; i < worldIndicators.Length; i++)
            {
                Destroy(worldIndicators[i]);
            }
        }
        tutorial.CompleteTutorial(tutorialName);
    }

    public void ShowTutorial(int message)
    {
        HideTutorial();

        if (message != -1)
        {
            messages[message].SetActive(true);
            lastMessage = message;
        }
    }

    public void HideTutorial()
    {
        HideTutorial(lastMessage);
    }

    public void HideTutorial(int message)
    {
        if (message != -1)
            messages[message].SetActive(false);
    }
}

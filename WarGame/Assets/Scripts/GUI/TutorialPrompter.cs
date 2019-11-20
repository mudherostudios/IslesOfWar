using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPrompter : MonoBehaviour
{
    public Tutorial tutorial;
    public string tutorialName;
    public GameObject[] worldIndicators;
    public GameObject[] messages;
    public int completeThreshold = -1;
    private int lastMessage = -1;
    private int orderIndex = -1;
    private bool hasBeenInitiated = false;

    public void Start()
    {
        GameObject tutorialObject = GameObject.Find("MasterTutorial");

        if (tutorialObject != null)
            tutorial = tutorialObject.GetComponent<Tutorial>();
        else
        {
            this.enabled = false;
            gameObject.SetActive(false);
        }
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

    public void SkipToIndex(int index)
    {
        orderIndex = index;

        if (orderIndex < messages.Length)
            ShowTutorial(orderIndex);
        else
            CompleteTutorial();
    }

    public void CompleteTutorial()
    {
        if (orderIndex >= completeThreshold)
        {
            HideTutorial();
            if (worldIndicators != null)
            {
                for (int i = 0; i < worldIndicators.Length; i++)
                {
                    Destroy(worldIndicators[i]);
                }
            }
            tutorial.CompleteTutorial(tutorialName);
        }
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

    public void CloseTutorial()
    {
        for (int m = 0; m < messages.Length; m++)
        {
            messages[m].SetActive(false);
            if (m < worldIndicators.Length)
                worldIndicators[m].SetActive(false);
        }
    }

    public bool isCompletable { get { return orderIndex >= completeThreshold; } }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tutorial : Interaction
{
    List<string> tutorialStages = new List<string>() { "login", "selectUser", "chooseNation", "commandCenter", "menuActions", "cancelAction", "lighthouse"};
    Dictionary<string, TutorialPrompter> prompters;
    TutorialPrompter currentTutorialPrompter;
    int frameTracker = 0;

    public GameObject namePanel;

    private void Awake()
    {
        DontDestroyOnLoad(this);
        CheckForPrompters();
    }

    public void CheckForPrompters()
    {
        string[] completedTutorials = PlayerPrefs.GetString("completedTutorials").Split(',');
        GameObject[] prompterObjects = GameObject.FindGameObjectsWithTag("TutorialPrompter");
        prompters = new Dictionary<string, TutorialPrompter>();

        if (completedTutorials != null)
        {
            for (int t = 0; t < completedTutorials.Length; t++)
            {
                tutorialStages.Remove(completedTutorials[t]);
            }
        }

        if (tutorialStages.Count > 0)
        {
            for (int p = 0; p < prompterObjects.Length; p++)
            {
                TutorialPrompter prompter = prompterObjects[p].GetComponent<TutorialPrompter>();

                if (tutorialStages.Contains(prompter.tutorialName))
                    prompters.Add(prompter.tutorialName, prompter);
            }
        }
        else
            Destroy(gameObject);

        CheckTutorialCondition();
    }

    public void Update()
    {
        bool clicked = Input.GetButtonDown("Fire1");

        CheckTutorialCondition();
        
        if (cam != null)
        {
            WorldButtonCheck(clicked);

            if (clicked && currentTutorialPrompter == null && tutorialStages.Count > 0)
            {
                Transform tutorialObject = selectedButton.transform.Find("TutorialPrompter");

                if (tutorialObject != null)
                {
                    currentTutorialPrompter = tutorialObject.GetComponent<TutorialPrompter>();
                    if (currentTutorialPrompter.tutorialName == tutorialStages[0])
                        currentTutorialPrompter.NextTutorial();
                }
            }
        }
    }

    public void CompleteTutorial(string tutorialName)
    {
        string completedTutorials = PlayerPrefs.GetString("completedTutorials");
        PlayerPrefs.SetString("completedTutorials", string.Format("{0},{1}", completedTutorials, tutorialName));
        tutorialStages.Remove(tutorialName);
        Destroy(prompters[tutorialName].gameObject);
        prompters.Remove(tutorialName);

        CheckTutorialCondition();

        if (tutorialStages.Count == 0)
            Destroy(gameObject);
    }

    public void CheckTutorialCondition()
    {
        if (frameTracker == 45)
        {
            if (clientInterface == null)
            {
                if (login && tutorialStages[0] == "login")
                    prompters["login"].InitiateTutorial();
                else if (selectUser && tutorialStages[0] == "selectUser")
                    prompters["selectUser"].InitiateTutorial();
            }
            else if (clientInterface != null)
            {
                if (tutorialStages[0] == "chooseNation")
                {
                    if (clientInterface.isPlaying)
                        CompleteTutorial("chooseNation");
                    else if(chooseNation)
                        prompters["chooseNation"].InitiateTutorial();
                }
                else if (commandCenter && tutorialStages[0] == "commandCenter")
                    prompters["commandCenter"].InitiateTutorial();
            }

            frameTracker = 0;
        }

        frameTracker++;
    }

    public bool login { get { return SceneManager.GetActiveScene().buildIndex == 0; } }
    public bool selectUser { get { return SceneManager.GetActiveScene().buildIndex == 0 && namePanel.activeSelf; } }
    public bool chooseNation { get { return SceneManager.GetActiveScene().buildIndex == 1 && !clientInterface.isPlaying; } }
    public bool commandCenter { get { return orbital.focalTarget.name == "CommandCenterBase"; } }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tutorial : Interaction
{
    List<string> tutorialStages = new List<string>()
    {
        "login", "selectUser", "chooseNation", "commandCenter", "menuActions", "cancelAction", "lighthouse",
        "manageIsland", "unitPurchase", "formSquads", "defend", "attack", "warbuxPool", "goodbye"
    };
    Dictionary<string, TutorialPrompter> prompters;
    TutorialPrompter currentTutorialPrompter;
    int frameTracker = 0;
    float goodbyeTimeStart = 0;
    float goodbyeTimer = 60;

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
        CheckTutorialCondition();
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
                if (login)
                    prompters["login"].InitiateTutorial();
                else if (selectUser)
                    prompters["selectUser"].InitiateTutorial();
            }
            else if (clientInterface != null)
            {
                if (tutorialStages[0] == "chooseNation")
                {
                    if (clientInterface.isPlaying)
                        CompleteTutorial("chooseNation");
                    else if (chooseNation)
                        prompters["chooseNation"].InitiateTutorial();
                }
                else if (commandCenter)
                    prompters["commandCenter"].InitiateTutorial();
                else if (menuActions)
                    prompters["menuActions"].InitiateTutorial();
                else if (cancelAction)
                    prompters["cancelAction"].InitiateTutorial();
                else if (lighthouse)
                    prompters["lighthouse"].InitiateTutorial();
                else if (manageIsland)
                    prompters["manageIsland"].InitiateTutorial();
                else if (unitPurchase)
                    prompters["unitPurchase"].InitiateTutorial();
                else if (formSquads)
                    prompters["formSquads"].InitiateTutorial();
                else if (defend)
                    prompters["defend"].InitiateTutorial();
                else if (attack)
                    prompters["attack"].InitiateTutorial();
                else if (warbuxPool)
                    prompters["warbuxPool"].InitiateTutorial();
                else if (tutorialStages[0] == "goodbye")
                {
                    if (goodbyeTimeStart != 0)
                    {
                        if(goodbye)
                            prompters["goodbye"].InitiateTutorial();
                    }
                    else
                    {
                        goodbyeTimeStart = Time.time;
                    }
                }
            }

            frameTracker = 0;
        }

        frameTracker++;
    }

    public bool login { get { return SceneManager.GetActiveScene().buildIndex == 0 && tutorialStages[0] == "login"; } }
    public bool selectUser { get { return SceneManager.GetActiveScene().buildIndex == 0 && namePanel.activeSelf && tutorialStages[0] == "selectUser"; } }
    public bool chooseNation { get { return SceneManager.GetActiveScene().buildIndex == 1 && !clientInterface.isPlaying; } }
    public bool commandCenter { get { return orbital.focalTarget.name == "CommandCenterBase" && tutorialStages[0] == "commandCenter"; } }
    public bool menuActions
    {
        get
        {
            return tutorialStages[0] == "menuActions" && orbital.focalTarget.name == "CommandCenterBase" 
            && GameObject.Find("GameGUI").transform.Find("CommandCenterMenu").gameObject.activeSelf;
        }
    }
    public bool cancelAction { get { return tutorialStages[0] == "cancelAction" && clientInterface.queuedActions.nat != null; } }
    public bool lighthouse { get { return tutorialStages[0] == "lighthouse" && clientInterface.queuedActions.nat == null; } }
    public bool manageIsland { get { return tutorialStages[0] == "manageIsland" && clientInterface.playerIslands.Length > 0; } }
    public bool unitPurchase { get { return tutorialStages[0] == "unitPurchase" && PlayerHasCollectors(); } }
    public bool formSquads { get { return tutorialStages[0] == "formSquads" && PlayerHasUnits(); } }
    public bool defend
    {
        get
        {
            bool hasKeys = PlayerPrefs.HasKey("keys");
            if (hasKeys)
                return tutorialStages[0] == "defend" && PlayerPrefs.GetString("keys") != "";
            else
                return false;
        }
    }
    public bool attack { get { return tutorialStages[0] == "attack" && clientInterface.attackableIsland.owner != null; } }
    public bool warbuxPool { get { return tutorialStages[0] == "warbuxPool"; } }
    public bool goodbye { get { return Time.time - goodbyeTimeStart >= goodbyeTimer; } }

    bool PlayerHasCollectors()
    {
        bool hasCollectors = true;
        
        for (int i = 0; i < clientInterface.playerIslands.Length && hasCollectors; i++)
        {
            hasCollectors = hasCollectors && clientInterface.playerIslands[i].collectors != "000000000000";
        }

        return hasCollectors;
    }

    bool PlayerHasUnits()
    {
        bool hasUnits = false;

        for (int u = 0; u < clientInterface.playerUnits.Length && !hasUnits; u++)
        {
            hasUnits = clientInterface.playerUnits[0] != 0;
        }

        return hasUnits;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using IslesOfWar.ClientSide;

public class Interaction : MonoBehaviour
{
    public Camera cam;
    public OrbitalFocusCam orbital;
    public StateMaster stateMaster;
    public ScreenGUI screenGUI;

    [Header("Button Types")]
    public string[] buttonTypes = new string[] { "NavigationButton", "ObjectRevealer", "InputField", "IndexedNavigation"};

    private string clickedButtonType = "None";
    protected bool isTyping = false;
    protected int fieldID = -1;

    protected Transform selectedWorldUIObject;
    protected WorldGUI selectedWorldUI;
    protected WorldButton selectedButton;

    protected Vector3 targetPosition;
    protected bool isAtTarget;

    protected void WorldButtonCheck(bool didClick)
    {
        if (didClick)
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.tag == "WorldButton")
                {
                    selectedButton = hit.transform.GetComponent<WorldButton>();
                    clickedButtonType = selectedButton.buttonType;

                    if (clickedButtonType == buttonTypes[0] || clickedButtonType == buttonTypes[3])
                    {
                        NavigateToDestination();
                    }
                    else if (clickedButtonType == buttonTypes[1])
                    {
                        RevealObject();
                    }
                    else if (clickedButtonType == buttonTypes[2])
                    {
                        SelectInputField();
                    }
                }
            }
        }
        else
        {
            clickedButtonType = "None";
        }
    }

    private void NavigateToDestination()
    {
        Transform destination = selectedButton.GetComponent<NavigationButton>().navigationDestination;

        if (destination != null)
        {
            orbital.ExploreMode(destination, true);
            selectedWorldUIObject = null;
            selectedWorldUI = null;
        }
    }

    private void RevealObject()
    {
        GameObject hiddenObject = selectedButton.GetComponent<ObjectRevealer>().hiddenObject;

        if (hiddenObject != null)
        {
            if (hiddenObject.activeSelf)
                hiddenObject.SetActive(false);
            else if (!hiddenObject.activeSelf)
                hiddenObject.SetActive(true);
        }
    }

    //Fix "selectedWorldUI" logic in CommandIslandInteraction
    //Currently uses whether or not it is null to reset guis
    private void SelectInputField()
    {
        InputField inputField = selectedButton.GetComponent<InputField>();

        if (inputField != null)
        {
            selectedWorldUI = inputField.guiParent;
            isTyping = true;
            fieldID = inputField.fieldID;
        }
    }

    protected void Typing()
    {
        if (isTyping && selectedWorldUI != null)
        {
            selectedWorldUI.AddCharacter(Input.inputString, fieldID);

            if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
                selectedWorldUI.DeleteCharacter(fieldID);
        }
    }

    string GetOrderOfMagnitudeString(ulong amount)
    {
        string orderOfMag = "";

        if (amount < 10000)
            orderOfMag = amount.ToString();
        else
            orderOfMag = amount.ToString("G3", CultureInfo.InvariantCulture);
        return orderOfMag;
    }

    public string CheckButtonType()
    {
        string temp = clickedButtonType;
        clickedButtonType = "None";
        return temp;
    }

    public string PeekButtonType()
    {
        return clickedButtonType;
    }

    public void SetGUIContents()
    {
        screenGUI.SetGUIContents();
    }

    public void SetVariables(StateMaster _stateMaster, Camera _cam, OrbitalFocusCam _orbital, ScreenGUI _screenGUI, string[] _buttonTypes)
    {
        stateMaster = _stateMaster;
        cam = _cam;
        orbital = _orbital;
        screenGUI = _screenGUI;
        buttonTypes = _buttonTypes;
    }
}

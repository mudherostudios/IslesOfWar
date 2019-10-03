using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
using IslesOfWar.ClientSide;
using IslesOfWar.GameStateProcessing;

public class Interaction : MonoBehaviour
{
    public Camera cam;
    public OrbitalFocusCam orbital;
    public ClientInterface clientInterface;
    public ScreenGUI screenGUI;

    [Header("Button Types")]
    public string[] buttonTypes = new string[] { "NavigationButton", "ObjectRevealer", "IndexedNavigation"};

    private string clickedButtonType = "None";
    protected bool isTyping = false;
    protected int fieldID = -1;

    protected Transform selectedWorldUIObject;
    protected WorldGUI selectedWorldUI;
    protected WorldButton selectedButton;

    protected Vector3 targetPosition;
    protected bool isAtTarget;
    protected int indexLocation = 4;
    protected int lastIndexLocation = 11;

    protected void WorldButtonCheck(bool didClick)
    {
        WorldButtonCheck(didClick, new List<string>());
    }

    protected void WorldButtonCheck(bool didClick, List<string> navigators)
    {
        if (didClick)
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit) && !EventSystem.current.IsPointerOverGameObject())
            {
                bool isConstructing = hit.transform.tag == "UnderConstruction";

                if (hit.transform.tag == "WorldButton" || isConstructing)
                {
                    selectedButton = hit.transform.GetComponent<WorldButton>();
                    selectedWorldUIObject = hit.transform;
                    clickedButtonType = selectedButton.buttonType;

                    if (clickedButtonType == buttonTypes[0] || clickedButtonType == buttonTypes[2] || navigators.Contains(clickedButtonType))
                    {
                        NavigateToDestination();

                        if (clickedButtonType == buttonTypes[2])
                        {
                            lastIndexLocation = indexLocation;
                            indexLocation = selectedWorldUIObject.GetComponent<IndexedNavigationButton>().index;
                        }
                    }

                    if (clickedButtonType == buttonTypes[1])
                    {
                        RevealObject();
                    }

                    if(isConstructing)
                        RevealObject();
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

    public void SetVariables(ClientInterface client, Camera _cam, OrbitalFocusCam _orbital, ScreenGUI _screenGUI, string[] _buttonTypes)
    {
        clientInterface = client;
        cam = _cam;
        orbital = _orbital;
        screenGUI = _screenGUI;
        buttonTypes = _buttonTypes;
    }
}

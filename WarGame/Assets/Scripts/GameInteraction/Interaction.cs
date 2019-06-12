using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class Interaction : MonoBehaviour
{
    public Camera cam;
    public StateMaster stateMaster;
    public ScreenGUI screenGUI;

    [Header("Button Types")]
    public string[] buttonTypes = new string[] { "InputField", "MenuRevealer" };

    private string clickedButtonType = "None";
    protected bool isTyping = false;
    protected int fieldID = -1;

    protected Transform selectedWorldUIObject;
    protected WorldGUI selectedWorldUI;
    protected WorldButton selectedButton;

    protected void WorldButtonCheck()
    {
        if (cam != null && Input.GetButtonDown("Fire1"))
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.tag == "WorldButton")
                {
                    selectedButton = hit.transform.GetComponent<WorldButton>();
                    clickedButtonType = selectedButton.buttonType;
                    Debug.Log(clickedButtonType);

                    if (selectedWorldUIObject != null && selectedWorldUIObject.gameObject.activeSelf && selectedButton.logicParent != selectedWorldUI)
                        selectedWorldUIObject.gameObject.SetActive(false);

                    selectedWorldUIObject = selectedButton.logicParent;
                    selectedWorldUI = selectedWorldUIObject.GetComponent<WorldGUI>();

                    
                    if (clickedButtonType == buttonTypes[0])
                    {
                        isTyping = true;
                        fieldID = selectedButton.fieldID;
                    }
                    else if (clickedButtonType == buttonTypes[1])
                    {
                        if (selectedWorldUI.gameObject.activeSelf)
                            selectedWorldUI.gameObject.SetActive(false);
                        else
                            selectedWorldUI.gameObject.SetActive(true);
                    }
                }
                else
                {
                    clickedButtonType = "None";

                    if (selectedWorldUIObject != null)
                    {
                        selectedWorldUIObject.gameObject.SetActive(false);
                    }

                    isTyping = false;
                }
            }
            else
            {
                clickedButtonType = "None";
            }
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
}

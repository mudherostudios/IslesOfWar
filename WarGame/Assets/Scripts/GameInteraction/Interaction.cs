using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class Interaction : MonoBehaviour
{
    public Camera cam;
    public StateMaster stateMaster;

    [Header("Button Types")]
    public string[] buttonTypes = new string[] { "InputField", "MenuRevealer" };

    private string clickedButtonType = "None";
    private bool isTyping = false;
    private int fieldID = -1;

    private Transform selectedWorldUIObject;
    private WorldGUI selectedWorldUI;

    void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    void Update()
    {
        Typing();
        WorldButtonCheck();
    }

    void WorldButtonCheck()
    {
        if (cam != null && Input.GetButtonDown("Fire1"))
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.tag == "WorldButton")
                {
                    WorldButton button = hit.transform.GetComponent<WorldButton>();

                    if (selectedWorldUIObject != null && selectedWorldUIObject.gameObject.activeSelf && button.logicParent != selectedWorldUI)
                        selectedWorldUIObject.gameObject.SetActive(false);

                    selectedWorldUIObject = button.logicParent;
                    selectedWorldUI = selectedWorldUIObject.GetComponent<WorldGUI>();

                    clickedButtonType = button.buttonType;

                    if (clickedButtonType == buttonTypes[0])
                    {
                        isTyping = true;
                        fieldID = button.fieldID;
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

                    if (selectedWorldUI != null)
                    {
                        selectedWorldUI.gameObject.SetActive(false);
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

    void Typing()
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

}

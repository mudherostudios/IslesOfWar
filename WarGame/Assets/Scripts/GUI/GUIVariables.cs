using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GUIVariables : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ScreenGUI master;
    public string toolTipContent;

    public void OnPointerEnter(PointerEventData eventData)
    {
        master.SetToolTip(toolTipContent);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        master.SetToolTip("");
    }
}

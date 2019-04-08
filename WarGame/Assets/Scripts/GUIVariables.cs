using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GUIVariables : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameMaster master;
    public string toolTipContent;
   
    void Start()
    {
        if(master == null)
            master = GameObject.FindGameObjectWithTag("GameMaster").GetComponent<GameMaster>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        master.SetToolTip(toolTipContent);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        master.SetToolTip("");
    }
}

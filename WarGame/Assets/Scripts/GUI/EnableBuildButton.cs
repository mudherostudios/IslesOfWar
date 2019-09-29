using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableBuildButton : MonoBehaviour
{
    public IslandManagementInteraction managementScript;

    public void EnableBuild(int type)
    {
       managementScript.EnableBuildStructures(type);
    }

    public void Show(bool show)
    {
        gameObject.SetActive(show);
    }
}

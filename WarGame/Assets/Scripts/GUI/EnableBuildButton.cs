using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableBuildButton : MonoBehaviour
{
    public IslandManagementInteraction managementScript;

    public void EnableBuild(int type)
    {
       bool canBuild = managementScript.EnableBuildStructures(type);

        if (canBuild)
            gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }
}

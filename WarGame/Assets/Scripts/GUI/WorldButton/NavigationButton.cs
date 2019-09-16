using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationButton : WorldButton
{
    public Transform navigationDestination;

    public void Start()
    {
        UnityEditorInternal.ComponentUtility.MoveComponentUp(this);
    }
}

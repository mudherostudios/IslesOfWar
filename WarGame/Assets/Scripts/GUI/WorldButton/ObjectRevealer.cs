using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRevealer : WorldButton
{
    public GameObject hiddenObject;
    public AudioClip showObjectSound;
    public AudioClip hideObjectSound;
    public bool hasShowSound { get { return showObjectSound != null; } }
    public bool hasHideSound { get { return hideObjectSound != null; } }
}

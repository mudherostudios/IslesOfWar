using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationButton : WorldButton
{
    public Transform navigationDestination;
    public AudioClip tileEntrance;
    public AudioClip tileAmbience;
    public bool hasEntranceSound { get { return tileEntrance != null; } }
    public bool hasAmbienceSound { get { return tileAmbience != null; } }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadMarker : WorldButton
{
    public int squad;
    public int owner;
    public string squadName;
    public TextMesh textName;

    public void SetName()
    {
        textName.text = squadName;
    }
}

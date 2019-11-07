using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadMarker : WorldButton
{
    public int squad;
    public int owner;
    public string squadName;
    public TextMesh textName;
    public TileStats currentTile;

    public void SetName()
    {
        textName.text = squadName;
    }

    public void SetCurrentTile(TileStats tile, Vector3 offset)
    {
        if (tile != currentTile)
        {
            if (currentTile != null)
                currentTile.MoveMarkerOffTile(this, offset);

            currentTile = tile;
            currentTile.SetMarkerOnTile(this, offset);
        }
    }
}

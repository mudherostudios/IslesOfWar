using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadMarker : WorldButton
{
    public int squad;
    public int owner;
    public int displayType = 0;
    public string squadName;
    public TextMesh textName;
    public TileStats currentTile;

    public void SetNameAndType(int type)
    {
        textName.text = squadName;
        displayType = type;
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

    public void RemoveFromBattleField(Vector3 offset)
    {
        if(currentTile != null)
            currentTile.MoveMarkerOffTile(this, offset);
    }
}

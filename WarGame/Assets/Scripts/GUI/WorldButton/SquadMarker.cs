using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadMarker : WorldButton
{
    public int squad;
    public bool isPlayers;
    public bool defender = false;
    public int displayType = 0;
    public string squadName;
    public SkinnedMeshRenderer squadMeshRenderer;
    public Material playerTeamMaterial;
    public Material enemyTeamMaterial;
    public TextMesh textName;
    public TileStats currentTile;

    public void SetNameAndType(int type, bool _isPlayers)
    {
        isPlayers = _isPlayers; 
        textName.text = string.Format("{0} Squad",squadName);
        displayType = type;

        if (type <= 2)
        {
            if (isPlayers)
                squadMeshRenderer.material = playerTeamMaterial;
            else
                squadMeshRenderer.material = enemyTeamMaterial;
        }
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

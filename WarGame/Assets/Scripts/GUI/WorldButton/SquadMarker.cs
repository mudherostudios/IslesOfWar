using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadMarker : WorldButton
{
    public int squad;
    private List<GameObject> trailPoints = new List<GameObject>();
    private List<int> positions = new List<int>();
    private List<LineRenderer> trailLines;

    public void AddNewPosition(int position, Vector3 spawn, GameObject prefab)
    {
        trailPoints.Add(Instantiate(prefab, spawn, Quaternion.identity));
        positions.Add(position);
    }

    public void RemovePosition(int position)
    {
        if (positions.Contains(position))
        {
            int index = positions.IndexOf(position);
            Destroy(trailPoints[index]);
            trailPoints.RemoveAt(index);
            positions.RemoveAt(index);
        }
    }

    public void Reset()
    {
        
    }

}

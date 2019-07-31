using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IslesOfWar.Combat;
using IslesOfWar.ClientSide;
using TMPro;

public class SquadGUI : WorldGUI
{
    public TextMeshPro[] unitCounts;
    public TextMeshPro[] squadCounts;
    public TextMeshPro[] unitInputs;

    public long[][] squads;
    public long[] allUnits;

    private int selectedSquad = 0;
    private PlayerState playerState;

    public void Initialize(PlayerState state)
    {
        playerState = state;
        squads = new long[squads.Length][];

        for (int s = 0; s < playerState.squads.Count; s++)
        {
            squads[s] = playerState.squads[s].fullSquad;
        }

        allUnits = playerState.allUnits;
    }

    public void AddUnitsToSquad(int type)
    {
        for (int f = 0; f < 3; f++)
        {
            int index = type * 3 + f;
            long count = fieldAmounts[index];

            if (count <= allUnits[index])
            {
                allUnits[index] -= count;
                squads[selectedSquad][index] += count;
            }

            Reset(index);
        }
    }

    public void RemoveUnitsFromSquad(int type)
    {
        for (int f = 0; f < 3; f++)
        {
            int index = type * 3 + f;
            long count = fieldAmounts[index];

            if (count <= squads[selectedSquad][f])
            {
                allUnits[index] += count;
                squads[selectedSquad][index] -= count;
            }
        }
    }

    public void SwitchToSquad(int squad)
    {
        if (squad < 3 && squad >= 0)
        {
            selectedSquad = squad;
            Reset();
        }
    }

    public void Cancel()
    {
        Reset();

        for (int s = 0; s < squads.Length; s++)
        {
            squads[s] = playerState.squads[s].fullSquad;
        }
    }

    public Squad[] TrySubmit()
    {
        Squad[] squadObjects = new Squad[squads.Length];

        for (int s = 0; s < squads.Length; s++)
        {
            squadObjects[s] = new Squad(squads[s]);
        }

        return squadObjects;
    }

    public void UpdateAllStats()
    {
        for (int u = 0; u < allUnits.Length; u++)
        {
            string unitFormat = "";
            string squadFormat = "";

            if (allUnits[u] > 999999999)
                unitFormat = "G2";
            if (squads[selectedSquad][u] > 999999999)
                squadFormat = "G2";

            unitCounts[u].text = allUnits[u].ToString(unitFormat);
            squadCounts[u].text = squads[selectedSquad][u].ToString(squadFormat);
            unitInputs[u].text = fieldAmounts[u].ToString();
        }

    }
}

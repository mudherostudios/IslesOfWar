using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGUI : MonoBehaviour
{
    protected ulong[] fieldAmounts;
    protected string[] fields;

    public void AddCharacter(string str, int ID)
    {
        if (fields[ID].Length < 10 && "0123456789".Contains(str))
        {
            fields[ID] += str;
            ulong temp = 0;
            ulong.TryParse(fields[ID], out temp);
            fieldAmounts[ID] = temp;
        }
    }

    public void DeleteCharacter(int ID)
    {
        if (fields[ID].Length > 0)
            fields[ID] = fields[ID].Remove(fields[ID].Length - 1);

        if (fields[ID].Length == 0)
            fieldAmounts[ID] = 0;
    }

    public void Reset()
    {
        for(int i = 0; i < fields.Length; i++)
        {
            fields[i] = "0";
            fieldAmounts[i] = 0;
        }
    }

    public void Reset(int field)
    {
        fields[field] = "0";
        fieldAmounts[field] = 0;
    }

    public static long MapUlongToLong(ulong ulongValue)
    {
        return unchecked((long)ulongValue + long.MinValue);
    }
}

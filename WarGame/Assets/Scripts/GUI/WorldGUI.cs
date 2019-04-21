using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGUI : MonoBehaviour
{
    protected ulong amount;
    protected string strAmount;

    public void AddCharacter(string str)
    {
        if (strAmount.Length < 10 && "0123456789".Contains(str))
        {
            strAmount += str;
            ulong temp = 1;
            ulong.TryParse(strAmount, out temp);
            amount = temp;
        }
    }

    public void DeleteCharacter()
    {
        if (strAmount.Length > 0)
            strAmount = strAmount.Remove(strAmount.Length - 1);

        if (strAmount.Length == 0)
            amount = 1;
    }

    public void Reset()
    {
        strAmount = "";
        amount = 1;
    }
}

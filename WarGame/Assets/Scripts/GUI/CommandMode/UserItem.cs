using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UserItem : MonoBehaviour, IPointerClickHandler
{
    public PlayerTrading master;
    public int orderCounts;
    public string userName;
    public Text info;

    public void Set(string user, int count, PlayerTrading trading)
    {
        master = trading;
        orderCounts = count;
        userName = user;
        string temp = string.Copy(user);

        if (temp.Length > 12)
            temp = user.Substring(0, 12);

        info.text = string.Format("{0}: {1} items", temp, orderCounts.ToString("G4"));
    }

    public void SetTextColor(Color color)
    {
        info.color = color;
    }

    public void OnPointerClick(PointerEventData data)
    {
        master.SetSelected(gameObject);
    }
}

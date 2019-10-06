using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationObject : MonoBehaviour
{
    public Image typeIcon;
    public Text message;

    public void SetMessage(Sprite icon, string _message)
    {
        if(icon != null)
            typeIcon.sprite = icon;

        message.text = _message;
    }
}

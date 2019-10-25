using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Notifications : MonoBehaviour
{
    public PlayerAudio playerAudio;
    public Transform contentParent;
    public GameObject simpleMessage;
    public GameObject notificationPrefab;
    public Sprite infoIcon, queueIcon, submitIcon, cancelIcon;
    public float stayTimer = 2.5f;
    public Vector3 onPosition, offPosition, simpleOnPosition;
    public GameObject onButton, offButton;
    private List<GameObject> notifications = new List<GameObject>();
    private float lastTime = 0.0f;
    private bool doneTiming = true;

    public void Update()
    {
        if (Time.time - lastTime >= stayTimer && !doneTiming)
        {
            Close();
        }
    }

    public void Open(bool simple)
    {
        if (simple)
        {
            lastTime = Time.time;
            doneTiming = false;
            simpleMessage.transform.position = new Vector3(simpleOnPosition.x, simpleOnPosition.y, 0);

            //Off
            transform.position = new Vector3(offPosition.x, transform.position.y, 0);
        }
        else
        {
            transform.position = new Vector3(onPosition.x, transform.position.y, 0);
            onButton.SetActive(false);
            offButton.SetActive(true);

            //Off
            simpleMessage.transform.position = new Vector3(offPosition.x, simpleMessage.transform.position.y, 0);
        }
    }

    public void Close()
    {
        transform.position = new Vector3(offPosition.x, transform.position.y, 0);
        simpleMessage.transform.position = new Vector3(offPosition.x, transform.position.y, 0);
        doneTiming = true;
        offButton.SetActive(false);
        onButton.SetActive(true);
    }

    public void PushNotification(int type, int soundType, string message)
    {
        PushNotification(type, soundType, message, null);
    }

    public void PushNotification(int type, int soundType, string message, string notificationName)
    {
        Open(true);

        GameObject notification = Instantiate(notificationPrefab, contentParent);
        notifications.Add(notification);
        
        Sprite typeIcon = GetTypeIcon(type);
        notification.GetComponent<NotificationObject>().SetMessage(typeIcon, message);
        simpleMessage.GetComponent<NotificationObject>().SetMessage(typeIcon, message);

        if (notifications.Count * 32 >= contentParent.GetComponent<RectTransform>().rect.height)
        {
            Rect oldRect = contentParent.GetComponent<RectTransform>().rect;
            contentParent.GetComponent<RectTransform>().sizeDelta = new Vector2(oldRect.size.x, notifications.Count*32+24);
        }

        playerAudio.PlayGUISound(soundType, notificationName);
        BuildList();
    }

    Sprite GetTypeIcon(int type)
    {
        switch (type)
        {
            case 0:
                return infoIcon; 
            case 1:
                return queueIcon; 
            case 2:
                return cancelIcon; 
            case 3:
                return submitIcon;
            default:
                return queueIcon;
        }
    }

    void BuildList()
    {
        for (int n = 0; n < notifications.Count; n++)
        {
            notifications[n].transform.localPosition = new Vector3(136, (n * -32) - 24, 0);
        }
    }

    public void CleanList()
    {
        foreach (GameObject notification in notifications)
        {
            Destroy(notification);
        }

        notifications.Clear();
        Rect oldRect = contentParent.GetComponent<RectTransform>().rect;
        contentParent.GetComponent<RectTransform>().sizeDelta = new Vector2(oldRect.size.x, 200);
    }


}

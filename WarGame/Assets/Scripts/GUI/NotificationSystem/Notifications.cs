using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Notifications : MonoBehaviour
{
    public Transform contentParent;
    public GameObject notificationPrefab;
    public Sprite infoIcon, queueIcon, submitIcon;
    public float stayTimer = 2.5f;
    public Vector3 onPosition, offPosition;
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

    public void Open(bool timed)
    {
        if (timed)
        {
            lastTime = Time.time;
            doneTiming = false;
        }

        transform.position = new Vector3(onPosition.x, transform.position.y, 0);
        onButton.SetActive(false);
        offButton.SetActive(true);
    }

    public void Close()
    {
        transform.position = new Vector3(offPosition.x, transform.position.y, 0);
        doneTiming = true;
        offButton.SetActive(false);
        onButton.SetActive(true);
    }

    public void PushNotification(int type, string message)
    {
        Open(true);

        GameObject notification = Instantiate(notificationPrefab, contentParent);
        notifications.Add(notification);
        
        Sprite typeIcon = GetTypeIcon(type);
        notification.GetComponent<NotificationObject>().SetMessage(typeIcon, message);

        if (notifications.Count * 32 >= contentParent.GetComponent<RectTransform>().rect.height)
        {
            Rect oldRect = contentParent.GetComponent<RectTransform>().rect;
            contentParent.GetComponent<RectTransform>().sizeDelta = new Vector2(oldRect.size.x, notifications.Count*32);
        }

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
                return submitIcon;
            default:
                return infoIcon;
        }
    }

    void BuildList()
    {
        for (int n = 0; n < notifications.Count; n++)
        {
            notifications[n].transform.localPosition = new Vector3(0, (n * -32) - 16, 0);
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

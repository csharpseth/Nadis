using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationSystem : MonoBehaviour
{
    public Transform container;
    public GameObject prefab;
    public float timeout = 5f;
    public Notification[] notifications;


    private Dictionary<NotificationType, Image> notes;
    private Dictionary<NotificationType, float> waitToDestroy;

    private void Awake()
    {
        notes = new Dictionary<NotificationType, Image>();
        waitToDestroy = new Dictionary<NotificationType, float>();

        Events.Notification.New = Notify;
        Events.Notification.Remove = RemoveNotification;
    }

    private void Update()
    {
        return;
        if (waitToDestroy.Count == 0) return;

        foreach(var note in waitToDestroy)
        {
            float temp = note.Value;
            temp += Time.deltaTime;
            if (temp >= timeout)
            {
                RemoveNotification(note.Key);
                continue;
            }

            waitToDestroy[note.Key] = temp;
        }
    }

    private void Notify(NotificationType type, bool persistent = false)
    {
        if (type == NotificationType.None || notes.ContainsKey(type) || container == null || prefab == null) return;

        Image img = Instantiate(prefab, container).GetComponent<Image>();
        img.sprite = FindType(type);
        if(img.sprite == null)
        {
            Debug.LogError("ERROR, failed to create notification: no sprite defined for type: " + type);
            Destroy(img.gameObject);
            return;
        }

        notes.Add(type, img);
        if(persistent == false)
        {
            waitToDestroy.Add(type, 0f);
        }
    }
    private void RemoveNotification(NotificationType type)
    {
        if (notes.ContainsKey(type) == false) return;
        if (waitToDestroy.ContainsKey(type))
            waitToDestroy.Remove(type);
        
        Destroy(notes[type].gameObject);
        notes.Remove(type);
    }
    
    private Sprite FindType(NotificationType type)
    {
        Sprite s = null;

        for (int i = 0; i < notifications.Length; i++)
        {
            if(notifications[i].type == type)
            {
                s = notifications[i].img;
            }
        }

        return s;
    }

}

[System.Serializable]
public struct Notification
{
    public NotificationType type;
    public Sprite img;
}

public enum NotificationType
{
    None,
    NoPower,
    FullyCharged,
    Charging
}

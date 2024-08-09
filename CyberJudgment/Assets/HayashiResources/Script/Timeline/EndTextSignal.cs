using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class EndTextSignal : Marker, INotification, INotificationOptionProvider
{
    public PropertyName id { get; } = new PropertyName();

    public NotificationFlags flags => NotificationFlags.Retroactive | NotificationFlags.TriggerInEditMode;
}

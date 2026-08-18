using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TextSignal : Marker, INotification, INotificationOptionProvider
{
    public string CharacterName;
    public string TextToShow;

    public PropertyName id { get; } = new PropertyName();

    public NotificationFlags flags => NotificationFlags.Retroactive | NotificationFlags.TriggerInEditMode;
}

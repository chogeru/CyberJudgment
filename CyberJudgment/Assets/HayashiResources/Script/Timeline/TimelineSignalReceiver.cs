using UnityEngine;
using UnityEngine.Playables;

public class TimelineSignalReceiver : MonoBehaviour, INotificationReceiver
{
    [SerializeField]
    private TimelineTextManager _textManager;

    public void OnNotify(Playable origin, INotification notification, object context)
    {
        if (notification is TextSignal textSignal)
        {
            _textManager.ShowText(textSignal.CharacterName, textSignal.TextToShow);
        }
        else if (notification is EndTextSignal)
        {
            _textManager.HideText();
        }
    }
}

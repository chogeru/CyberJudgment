using AbubuResouse.Singleton;
using UnityEngine;

public class TimelineFadeSignal : MonoBehaviour
{
    public void FadeOutSignal(float fadeTime)
    {
        FadeManager.Instance.FadeOut(fadeTime);
    }

    public void FadeInSignal(float fadeTime)
    {
        FadeManager.Instance.FadeIn(fadeTime);
    }
}

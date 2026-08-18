using UnityEngine;

public class AttackEventManager : MonoBehaviour
{
    public delegate void AnimationEventAction();
    public delegate void AnimationEventActionWithIndex(int index);

    public static event AnimationEventAction OnEvent;
    public static event AnimationEventAction OffEvent;
    public static event AnimationEventActionWithIndex OnEffectEvent;
    public static event AnimationEventActionWithIndex OffEffectEvent;

    public void AttackStart()
    {
        OnEvent?.Invoke();
    }

    public void AttackEnd()
    {
        OffEvent?.Invoke();
    }

    public void AttackEffectStart(int index)
    {
        OnEffectEvent?.Invoke(index);
    }

    public void AttackEffectEnd(int index)
    {
        OffEffectEvent?.Invoke(index);
    }
}

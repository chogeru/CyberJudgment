using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackEventManager : MonoBehaviour
{
    public delegate void AnimationEventAction();
    public static event AnimationEventAction OnEvent;
    public static event AnimationEventAction OffEvent;

    /// <summary>
    /// アニメーションイベントから呼び出されるOn関数
    /// </summary>
    public void AttackStart()
    {
        OnEvent?.Invoke();
    }

    /// <summary>
    /// アニメーションイベントから呼び出されるOff関数
    /// </summary>
    public void AttackEnd()
    {
        OffEvent?.Invoke();
    }
}

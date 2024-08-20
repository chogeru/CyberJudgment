using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackEventManager : MonoBehaviour
{
    public delegate void AnimationEventAction();
    public static event AnimationEventAction OnEvent;
    public static event AnimationEventAction OffEvent;

    /// <summary>
    /// �A�j���[�V�����C�x���g����Ăяo�����On�֐�
    /// </summary>
    public void AttackStart()
    {
        OnEvent?.Invoke();
    }

    /// <summary>
    /// �A�j���[�V�����C�x���g����Ăяo�����Off�֐�
    /// </summary>
    public void AttackEnd()
    {
        OffEvent?.Invoke();
    }
}

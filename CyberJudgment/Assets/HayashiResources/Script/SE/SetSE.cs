using AbubuResouse.Singleton;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetSE : MonoBehaviour
{
    [SerializeField, Header("再生するSE")]
    private string _se;
    [SerializeField, Header("音量")]
    private float _volume;

    public void SESet()
    {
        SEManager.Instance.PlaySound(_se, _volume);
    }
}

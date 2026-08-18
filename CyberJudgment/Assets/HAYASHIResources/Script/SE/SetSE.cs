using AbubuResouse.Singleton;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetSE : MonoBehaviour
{
    [SerializeField, Header("çƒê∂Ç∑ÇÈSE")]
    private string _se;
    [SerializeField, Header("âπó ")]
    private float _volume;

    public void SESet()
    {
        SEManager.Instance.PlaySound(_se, _volume);
    }
}

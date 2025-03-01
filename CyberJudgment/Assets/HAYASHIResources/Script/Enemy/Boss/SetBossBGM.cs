using AbubuResouse.Singleton;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class SetBossBGM : MonoBehaviour
{
    [SerializeField, Header("ボスのBGM名")]
    private string _bossBattleBGM;

    [SerializeField, Header("音量")]
    private float _Volume;
    private void Start()
    {
        BGMManager.Instance.PlaySound(_bossBattleBGM,_Volume);
    }
}

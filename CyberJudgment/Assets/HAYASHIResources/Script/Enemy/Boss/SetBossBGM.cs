using AbubuResouse.Singleton;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class SetBossBGM : MonoBehaviour
{
    [SerializeField, Header("É{ÉXÇÃBGMñº")]
    private string _bossBattleBGM;

    [SerializeField, Header("âπó ")]
    private float _Volume;
    private void Start()
    {
        BGMManager.Instance.PlaySound(_bossBattleBGM,_Volume);
    }
}

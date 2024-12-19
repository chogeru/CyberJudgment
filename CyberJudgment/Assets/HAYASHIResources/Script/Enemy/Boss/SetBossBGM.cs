using AbubuResouse.Singleton;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class SetBossBGM : MonoBehaviour
{
    [SerializeField, Header("�{�X��BGM��")]
    private string _bossBattleBGM;

    [SerializeField, Header("����")]
    private float _Volume;
    private void Start()
    {
        BGMManager.Instance.PlaySound(_bossBattleBGM,_Volume);
    }
}

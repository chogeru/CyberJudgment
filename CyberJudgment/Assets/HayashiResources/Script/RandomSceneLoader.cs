using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public class RandomSceneLoader : MonoBehaviour
{
    [SerializeField] private string _targetSceneName;
    [SerializeField] private float _loadChance = 0.001f;

    private void Start()
    {
        TryLoadRandomSceneAsync().Forget();
    }

    private async UniTaskVoid TryLoadRandomSceneAsync()
    {
        if (UnityEngine.Random.value <= _loadChance)
        {
            await SceneManager.Instance.SceneTransitionAsync(_targetSceneName);
        }
    }

}

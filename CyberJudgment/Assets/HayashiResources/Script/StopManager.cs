using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopManager : SingletonMonoBehaviour<StopManager>
{
    private bool isStopped;

    public bool IsStopped
    {
        get => isStopped;
        set => isStopped = value;
    }
}

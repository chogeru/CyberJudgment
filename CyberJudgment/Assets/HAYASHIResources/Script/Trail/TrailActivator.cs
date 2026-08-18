using System.Collections.Generic;
using UnityEngine;

public class TrailActivator : ActivatorBase
{
    [SerializeField, Header("トレイルリスト")]
    private List<TrailRenderer> trailRenderers;

    protected override void ActivateItems()
    {
        foreach (var trail in trailRenderers)
        {
            if (trail != null)
            {
                trail.enabled = true;
            }
        }
    }

    protected override void DeactivateItems()
    {
        foreach (var trail in trailRenderers)
        {
            if (trail != null)
            {
                trail.enabled = false;
            }
        }
    }
}
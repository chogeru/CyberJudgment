using System.Collections.Generic;
using UnityEngine;

public class ColliderActivator : ActivatorBase
{
    [SerializeField, Header("コライダーリスト")]
    private List<Collider> colliders;

    protected override void ActivateItems()
    {
        foreach (var collider in colliders)
        {
            if (collider != null)
            {
                collider.enabled = true;
            }
        }
    }

    protected override void DeactivateItems()
    {
        foreach (var collider in colliders)
        {
            if (collider != null)
            {
                collider.enabled = false;
            }
        }
    }
}
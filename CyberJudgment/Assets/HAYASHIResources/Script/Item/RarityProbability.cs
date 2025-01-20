using UnityEngine;
using System;

[Serializable]
public struct RarityProbability
{
    public WeaponRarity rarity;

    [Range(0f, 100f), Header("確率(%)")]
    public float probability;
}
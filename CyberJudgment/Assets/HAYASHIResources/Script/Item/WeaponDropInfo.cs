using UnityEngine;
using System;

[Serializable]
public struct WeaponDropInfo
{
    [Header("武器ドロップ確率(%)")]
    [Range(0f, 100f)]
    public float dropRate;

    [Header("レアリティごとの確率")]
    public RarityProbability[] rarityProbabilities;

    [Header("レアリティごとの武器プレハブリスト")]
    public RarityWeaponPrefabs[] rarityWeaponPrefabs;
}
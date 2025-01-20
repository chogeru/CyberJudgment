using UnityEngine;
using System;

[Serializable]
public class RarityWeaponPrefabs
{
    [Header("武器のレアリティ")]
    public WeaponRarity rarity;

    [Header("対応するRarityOrbのプレハブ")]
    public GameObject rarityOrbPrefab;
}
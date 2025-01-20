using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponNamesByRarity", menuName = "Data/WeaponNamesByRarity")]
public class WeaponNamesByRarity : ScriptableObject
{
    [Serializable]
    public class RarityWeaponNames // struct から class に変更
    {
        public WeaponRarity rarity;
        public List<string> weaponNames;
    }

    [Header("レアリティごとの武器名リスト")]
    public List<RarityWeaponNames> rarityWeaponNamesList;
}

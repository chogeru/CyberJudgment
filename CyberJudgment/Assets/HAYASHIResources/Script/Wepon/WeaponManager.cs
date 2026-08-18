using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    private Dictionary<WeaponType, List<WeaponData>> weaponDataByType = new Dictionary<WeaponType, List<WeaponData>>();
    private List<int> ownedWeaponIDs = new List<int>();

    // 初期化メソッド（武器データを追加する）
    public void Initialize(List<WeaponData> allWeaponData)
    {
        // 初期化
        foreach (WeaponType type in System.Enum.GetValues(typeof(WeaponType)))
        {
            weaponDataByType[type] = new List<WeaponData>();
        }

        // 武器データをタイプごとに分類して追加
        foreach (WeaponData weaponData in allWeaponData)
        {
            weaponDataByType[weaponData._weaponType].Add(weaponData);
        }
    }

    // 武器を所持しているか判定するメソッド
    public bool IsWeaponOwned(int weaponID)
    {
        return ownedWeaponIDs.Contains(weaponID);
    }

    // 武器を所持リストに追加するメソッド
    public void AddWeapon(int weaponID)
    {
        if (!ownedWeaponIDs.Contains(weaponID))
        {
            ownedWeaponIDs.Add(weaponID);
        }
    }

    // 武器を所持リストから削除するメソッド
    public void RemoveWeapon(int weaponID)
    {
        if (ownedWeaponIDs.Contains(weaponID))
        {
            ownedWeaponIDs.Remove(weaponID);
        }
    }

    // 特定の武器タイプの武器リストを取得するメソッド
    public List<WeaponData> GetWeaponsByType(WeaponType weaponType)
    {
        if (weaponDataByType.ContainsKey(weaponType))
        {
            return weaponDataByType[weaponType];
        }
        return new List<WeaponData>();
    }
}

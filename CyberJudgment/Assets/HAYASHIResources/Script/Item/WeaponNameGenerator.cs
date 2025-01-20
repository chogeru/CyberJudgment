using UnityEngine;

public class WeaponNameGenerator : MonoBehaviour
{
    public static WeaponNameGenerator Instance { get; private set; }

    [Header("レアリティごとの武器名データ")]
    public WeaponNamesByRarity weaponNamesByRarity;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    /// <summary>
    /// 指定されたレアリティからランダムに武器名を取得する
    /// </summary>
    public string GetRandomWeaponName(WeaponRarity rarity)
    {
        if (weaponNamesByRarity == null)
        {
            Debug.LogError("WeaponNamesByRarity が設定されていません。");
            return null;
        }

        var rarityData = weaponNamesByRarity.rarityWeaponNamesList.Find(r => r.rarity == rarity);
        if (rarityData != null && rarityData.weaponNames != null && rarityData.weaponNames.Count > 0)
        {
            int index = Random.Range(0, rarityData.weaponNames.Count);
            return rarityData.weaponNames[index];
        }

        Debug.LogWarning($"指定されたレアリティ「{rarity}」に対応する武器名が見つかりません。");
        return null;
    }
}

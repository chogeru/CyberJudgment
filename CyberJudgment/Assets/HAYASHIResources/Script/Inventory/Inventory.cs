using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    public List<string> weaponNames = new List<string>(); // 武器名リスト

    private string saveFilePath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        saveFilePath = Path.Combine(Application.persistentDataPath, "inventory.json");
        LoadInventory();
    }

    /// <summary>
    /// 武器名をインベントリに追加し、保存する
    /// </summary>
    /// <param name="weaponName">追加する武器名</param>
    public void AddWeaponToInventory(string weaponName)
    {
        if (string.IsNullOrEmpty(weaponName))
            return;

        weaponNames.Add(weaponName);
        SaveInventory();
        Debug.Log($"武器名「{weaponName}」をインベントリに追加しました。");
    }

    /// <summary>
    /// インベントリをJSONに保存
    /// </summary>
    private void SaveInventory()
    {
        WeaponInventorySaveData saveData = new WeaponInventorySaveData
        {
            weaponNames = weaponNames
        };
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("インベントリを保存しました。");
    }

    /// <summary>
    /// JSONからインベントリを読み込む
    /// </summary>
    private void LoadInventory()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            WeaponInventorySaveData saveData = JsonUtility.FromJson<WeaponInventorySaveData>(json);
            if (saveData != null)
            {
                weaponNames = saveData.weaponNames;
                Debug.Log("インベントリを読み込みました。");
            }
        }
        else
        {
            Debug.Log("保存されたインベントリが見つかりませんでした。新規作成します。");
        }
    }

    /// <summary>
    /// インベントリをクリアする
    /// </summary>
    public void ClearInventory()
    {
        weaponNames.Clear();
        SaveInventory();
        Debug.Log("インベントリをクリアしました。");
    }

    /// <summary>
    /// インベントリ内の武器名リストを取得する
    /// </summary>
    /// <returns>武器名リスト</returns>
    public List<string> GetWeapons()
    {
        return new List<string>(weaponNames);
    }
}

[System.Serializable]
public class WeaponInventorySaveData
{
    public List<string> weaponNames;
}

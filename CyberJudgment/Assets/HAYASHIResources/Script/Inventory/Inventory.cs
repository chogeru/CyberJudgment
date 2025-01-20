using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    public List<string> weaponNames = new List<string>(); // ���햼���X�g

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
    /// ���햼���C���x���g���ɒǉ����A�ۑ�����
    /// </summary>
    /// <param name="weaponName">�ǉ����镐�햼</param>
    public void AddWeaponToInventory(string weaponName)
    {
        if (string.IsNullOrEmpty(weaponName))
            return;

        weaponNames.Add(weaponName);
        SaveInventory();
        Debug.Log($"���햼�u{weaponName}�v���C���x���g���ɒǉ����܂����B");
    }

    /// <summary>
    /// �C���x���g����JSON�ɕۑ�
    /// </summary>
    private void SaveInventory()
    {
        WeaponInventorySaveData saveData = new WeaponInventorySaveData
        {
            weaponNames = weaponNames
        };
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("�C���x���g����ۑ����܂����B");
    }

    /// <summary>
    /// JSON����C���x���g����ǂݍ���
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
                Debug.Log("�C���x���g����ǂݍ��݂܂����B");
            }
        }
        else
        {
            Debug.Log("�ۑ����ꂽ�C���x���g����������܂���ł����B�V�K�쐬���܂��B");
        }
    }

    /// <summary>
    /// �C���x���g�����N���A����
    /// </summary>
    public void ClearInventory()
    {
        weaponNames.Clear();
        SaveInventory();
        Debug.Log("�C���x���g�����N���A���܂����B");
    }

    /// <summary>
    /// �C���x���g�����̕��햼���X�g���擾����
    /// </summary>
    /// <returns>���햼���X�g</returns>
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

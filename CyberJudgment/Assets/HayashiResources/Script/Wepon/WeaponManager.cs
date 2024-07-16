using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    private Dictionary<WeaponType, List<WeaponData>> weaponDataByType = new Dictionary<WeaponType, List<WeaponData>>();
    private List<int> ownedWeaponIDs = new List<int>();

    // ���������\�b�h�i����f�[�^��ǉ�����j
    public void Initialize(List<WeaponData> allWeaponData)
    {
        // ������
        foreach (WeaponType type in System.Enum.GetValues(typeof(WeaponType)))
        {
            weaponDataByType[type] = new List<WeaponData>();
        }

        // ����f�[�^���^�C�v���Ƃɕ��ނ��Ēǉ�
        foreach (WeaponData weaponData in allWeaponData)
        {
            weaponDataByType[weaponData._weaponType].Add(weaponData);
        }
    }

    // ������������Ă��邩���肷�郁�\�b�h
    public bool IsWeaponOwned(int weaponID)
    {
        return ownedWeaponIDs.Contains(weaponID);
    }

    // ������������X�g�ɒǉ����郁�\�b�h
    public void AddWeapon(int weaponID)
    {
        if (!ownedWeaponIDs.Contains(weaponID))
        {
            ownedWeaponIDs.Add(weaponID);
        }
    }

    // ������������X�g����폜���郁�\�b�h
    public void RemoveWeapon(int weaponID)
    {
        if (ownedWeaponIDs.Contains(weaponID))
        {
            ownedWeaponIDs.Remove(weaponID);
        }
    }

    // ����̕���^�C�v�̕��탊�X�g���擾���郁�\�b�h
    public List<WeaponData> GetWeaponsByType(WeaponType weaponType)
    {
        if (weaponDataByType.ContainsKey(weaponType))
        {
            return weaponDataByType[weaponType];
        }
        return new List<WeaponData>();
    }
}

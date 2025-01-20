using System.Collections.Generic;
using UnityEngine;
using VInspector;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "����/����f�[�^�쐬")]
public class WeaponData : ScriptableObject
{
    [Tab("�ڍ�")]
    [SerializeField, Header("����Id")]
    public int _weaponID;

    [SerializeField, Header("����̖��O")]
    public string _weaponName;

    [Tab("�f�U�C��")]
    [SerializeField, Header("�G�t�F�N�g�̃v���n�u")]
    public List<GameObject> EffectPrefabs;

    [SerializeField, Header("�U����SE")]
    public List<string> SoundEffects;

    [SerializeField, Header("�N���e�B�J���q�b�g��SE")]
    public List<string> CriticalSoundEffects;

    [Tab("�X�e�[�^�X")]
    [SerializeField, Header("�ŏ��U����")]
    public int _minAttackPower;

    [SerializeField, Header("�ő�U����")]
    public int _maxAttackPower;

    [SerializeField, Header("�N���e�B�J���� (%) [0 ~ 100]")]
    [Range(0f, 100f)]
    public float _criticalRate;

    /// <summary>
    /// ���������̌Œ�U���͂𗘗p���Ă��Ȃ��ꍇ�͍폜���Ă�OK
    /// </summary>
    [SerializeField, Header("�Œ�U����")]
    public int _attackPower;

    [SerializeField, Header("�U�����x")]
    public float _attackSpeed;

    [SerializeField, Header("����d��")]
    public float _weaponWeight;

    [Tab("�h���b�v���")]
    [SerializeField,Header("����̃��A���e�B")]
    public WeaponRarity rarity;
    
    [EndTab]
    [Tab("���̑�")]
    [SerializeField, Header("����̃^�C�v")]
    public WeaponType _weaponType;

    [SerializeField, Header("����̑���")]
    public WeaponAttribute _weaponAttribute;

    [TextArea, SerializeField, Header("���������")]
    public string _weaponDescription;


}

public enum WeaponType
{
    Sword,
    Bow,
    Axe,
    Hammer,
    Magic,
    Gun
}

public enum WeaponAttribute
{
    None,
    Fire,
    Water,
    Wind,
    Darkness,
    Light,
    Earth
}

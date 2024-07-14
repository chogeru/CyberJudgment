using UnityEngine;

[CreateAssetMenu(fileName = "NewWepon", menuName = "����/����f�[�^�쐬")]
public class WeaponData : ScriptableObject
{
    [SerializeField,Header("����Id")]
    public int _weaponID;
    [SerializeField,Header("����̖��O")]
    public string _weaponName;
    [SerializeField, Header("�U����")]
    public float _attackPower;
    [SerializeField, Header("�U�����x")]
    public float _attackSpeed;
    [SerializeField, Header("����d��")]
    public float _weaponWeight;
    [SerializeField,Header("����̃^�C�v")]
    public WeaponType _weaponType;
    [SerializeField,Header("����̑���")]
    public WeaponAttribute _weaponAttribute;
    [TextArea,SerializeField,Header("���������")]
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

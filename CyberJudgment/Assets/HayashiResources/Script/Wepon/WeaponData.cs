using UnityEngine;

[CreateAssetMenu(fileName = "NewWepon", menuName = "武器/武器データ作成")]
public class WeaponData : ScriptableObject
{
    [SerializeField,Header("武器Id")]
    public int _weaponID;
    [SerializeField,Header("武器の名前")]
    public string _weaponName;
    [SerializeField, Header("攻撃力")]
    public float _attackPower;
    [SerializeField, Header("攻撃速度")]
    public float _attackSpeed;
    [SerializeField, Header("武器重量")]
    public float _weaponWeight;
    [SerializeField,Header("武器のタイプ")]
    public WeaponType _weaponType;
    [SerializeField,Header("武器の属性")]
    public WeaponAttribute _weaponAttribute;
    [TextArea,SerializeField,Header("武器説明文")]
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

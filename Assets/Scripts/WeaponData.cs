using UnityEngine;

public enum WeaponType
{
    Ranged,
    Melee
}

public enum WeaponGripType
{ 
    Rifle,
    Pistol,
    Melee
}

public enum weaponSlotType
{
    Primary,
    Secondary,
    Melee
}


[CreateAssetMenu(menuName = "Weapon Data")]
public class WeaponData : ScriptableObject
{

    [Header("Identity")]
    public string weaponName;
    public WeaponType weaponType;
    public WeaponGripType weaponGripType;

    [Header("Prefabs")]
    public GameObject equippedPrefab;
    public GameObject pickupPrefab;
    public GameObject projectilePrefab;

    [Header("General Stats")]
    public int damage;
    public int resellPrice;

    [Header("Ranged Stats")]
    public bool isAutomatic;
    public float fireRate;
    public float projectileSpeed;
    public float range;
    public int magazineSize;
    public float spread;

    [Header("Melee Stats")]
 
    public float attackSpeed;
    public float staminaCost;


}

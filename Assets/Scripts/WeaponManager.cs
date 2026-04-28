using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform weaponSocket;
    [SerializeField] private WeaponInventory weaponInventory;
    [SerializeField] private WeaponData startingWeapon;

    private WeaponData equippedWeaponData;
    private GameObject equippedWeaponObject;

    public WeaponData EquippedWeaponData => equippedWeaponData;

    private void Start()
    {
        if (startingWeapon != null)
        {
            EquipWeapon(startingWeapon);
        }
        else
        {
            RefreshEquippedWeapon();
        }
    }

    public void RefreshEquippedWeapon()
    {
        if (weaponInventory == null)
            return;

        EquipWeapon(weaponInventory.CurrentWeapon);
    }

    public void EquipWeapon(WeaponData newWeapon)
    {
        if (newWeapon == null || newWeapon.equippedPrefab == null)
            return;

        equippedWeaponData = newWeapon;

        if (equippedWeaponObject != null)
            Destroy(equippedWeaponObject);

        equippedWeaponObject = Instantiate(newWeapon.equippedPrefab, weaponSocket);
        
        
        

        WeaponBehaviour weaponBehaviour = equippedWeaponObject.GetComponent<WeaponBehaviour>();

        if (weaponBehaviour != null)
        {
            weaponBehaviour.weaponData = newWeapon;
        }
        
    
    }

    public GameObject GetEquippedWeaponObject()
    {
        return equippedWeaponObject;
    }
}
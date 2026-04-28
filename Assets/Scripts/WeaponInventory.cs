using System.Collections.Generic;
using UnityEngine;

public class WeaponInventory : MonoBehaviour
{
    [SerializeField] private List<WeaponData> weapons = new List<WeaponData>();

    private int currentWeaponIndex = -1;

    public List<WeaponData> OwnedWeapons => OwnedWeapons;
    public WeaponData CurrentWeapon => currentWeaponIndex >= 0 && currentWeaponIndex < weapons.Count ?
        weapons[currentWeaponIndex] : null;


    public void AddWeapon(WeaponData newWeapon)
    {
        if (newWeapon == null)
            return;

        if (!OwnedWeapons.Contains(newWeapon))
        {
            OwnedWeapons.Add(newWeapon);
        }

        if(currentWeaponIndex == -1)
        {
            currentWeaponIndex = 0;
        }

    }

    public void EquipWeaponByIndex(int index)
    {
        if (index < 0 || index >= OwnedWeapons.Count)
            return;

        currentWeaponIndex = index;
    }

    public bool HasWeapon(WeaponData weapon)
    {
        return OwnedWeapons.Contains(weapon);
    }


}

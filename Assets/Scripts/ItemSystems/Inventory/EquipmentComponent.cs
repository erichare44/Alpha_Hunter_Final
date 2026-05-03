using System.Collections.Generic;
using UnityEngine;

public class EquipmentComponent : MonoBehaviour
{
    [SerializeField] public GameObject primaryWeapon;
    [SerializeField] public GameObject secondaryWeapon;
    [SerializeField] public List<GameObject> consumables;
    [SerializeField] public PlayerInventory inventory;

    private Sprite primaryIcon;
    private Sprite secondaryIcon;

    private void Start()
    {
    }

    private void Update()
    {
        if (primaryWeapon != null)
        {
            primaryIcon = InventoryManager.instance.emptyWeaponIcon; 
            if (secondaryWeapon != null)
            { 
                secondaryIcon = InventoryManager.instance.emptyWeaponIcon;
            }    
        }
        if (gameManager.instance.User != null)
        {
            InitializeEquipment();
        }

        if (Input.GetButtonDown("Heal"))
        {


            if (gameManager.instance.User.GetComponent<PlayerHealth>().currentHealth >= gameManager.instance.User.GetComponent<PlayerHealth>().maxHealth)
            {
                return;
            } 
            if (consumables.Count > 0)
            { 
                if (consumables[0].GetComponent<Bandage>().GetHealthComponent() != null && consumables[0].GetComponent<Bandage>().GetPlayerEquipment() != null)
                { 

                    consumables[0].GetComponent<Bandage>().UseBandage();
                }
            }


        }
    }



    private void InitializeEquipment()
    {
        if (inventory == null || inventory.GetItemList() == null)
            return;

        if (inventory.GetItemList().Count > 0)
        {
            primaryWeapon = inventory.GetItemList()[0];
            primaryIcon = primaryWeapon.GetComponent<InGameItem>().itemInstance.icon;
        }
        else
        {
            primaryWeapon = null;
            primaryIcon = InventoryManager.instance.emptyWeaponIcon;
        }

        if (inventory.GetItemList().Count > 1)
        {
            secondaryWeapon = inventory.GetItemList()[1];
            secondaryIcon = secondaryWeapon.GetComponent<InGameItem>().itemInstance.icon;
        }
        else
        {
            secondaryWeapon = null;
            secondaryIcon = InventoryManager.instance.emptyWeaponIcon;
        }
    }

    public Sprite GetPrimaryIcon() 
    {
        return primaryIcon;
    }

    public Sprite GetSecondaryIcon()
    {
        return secondaryIcon;
    }
}

using UnityEngine;

[System.Serializable]


public enum WeaponModel
{ 
    AKM,
    M4,
    Glock,
    AUG,
    Famas,
    Revolver,
    USP,
    Deagle,
    Mossberg,
    Benelli,
    MP5,
    Mac10,
    AWP,
    Kar98K
}

public class InGameItem: MonoBehaviour, IInteract
{
    public ItemInstance itemInstance;
    [SerializeField] Sprite icon;
    [SerializeField] ItemDefinition itemDefinition;


    //These are variables passed around, for the sake of creating item data for a weapon based on weapon types. Not actual data referring to anything specific.
    private ItemDefinition weaponDefinition;


    public void PullItemData()
    {
        itemInstance = new ItemInstance(itemDefinition, 0, 0, itemDefinition.width, itemDefinition.height, icon, 1, false);
    }


    public ItemDefinition GetItemDefinition()
    {
        return itemDefinition;
    }

    public void Interact()
    {
        throw new System.NotImplementedException();
    }
}

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.VisualScripting;

public class PlayerInventory : MonoBehaviour, iPickup 
{

    public List<GameObject> playerInventory = new List<GameObject>();

    [SerializeField] public GameObject inventoryGrid;


    private GameObject representedItem; //To be passed to GridControllerUI.cs for usage in drag n drop logic, as well as planned destroy logic when something is picked up.

    // Update is called once per frame
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }


    


    public void AddItemToBag(GameObject obj)
    {
        if (obj != null)
        { 

            bool objectPlaced = InventoryManager.instance.playerGridReal.AutoPlaceWeapon(obj);
            if (objectPlaced)
            {
                DontDestroyOnLoad(obj);
                playerInventory.Add(obj);
                Debug.Log("Added Item: " + obj.name + "To bag");
                InventoryManager.instance.CreatePlayerItemUI(obj);

                if (obj.CompareTag("ConsumablePickup"))
                {
                    InventoryManager.instance.equip.consumables.Add(obj);
                }


                InventoryManager.instance.SaveInventoryState();

                //obj.SetActive(false);

            }
            else if (!objectPlaced)
            {
                Debug.LogError("Error adding to bag, object not placed");
            }
        }
    }


    public List<GameObject> GetItemList()
    {
        return playerInventory;
    }

    public GameObject GetRepresentedItem()
    {
        return representedItem;
    }
}

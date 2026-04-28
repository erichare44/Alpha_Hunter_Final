using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Interactable : MonoBehaviour, IInteract
{

    [SerializeField] GameObject @object;
    [SerializeField] int cost;
    [SerializeField] GameObject interactUI;

    bool canInteract;

    private void Awake()
    {
        if (@object != null && @object.GetComponent<InGameItem>())
        { 
            @object.GetComponent<InGameItem>().PullItemData();
        }
    }
    void Update()
    {
        if(Input.GetButtonDown("Interact") && canInteract)
        {
            Interact();
        }
    }   

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            canInteract = true;
            if (interactUI != null) interactUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            canInteract = false;
            if (interactUI != null) interactUI.SetActive(false);
        }
    }

    public void InitializeObject(GameObject obj)
    {
        @object = obj;
    }

    public void Interact()
    {
        if(@object.CompareTag("Bounty Board"))
        {
            gameManager.instance.OpenBountyBoard();
        }
        if (@object.CompareTag("XPUI"))
        {
            gameManager.instance.OpenXPUI();
        }
        if(@object.CompareTag("Exit"))
        {
            gameManager.instance.ExitHUB();
        }
        if (@object.CompareTag("ShopItem") && gameManager.instance.TestBuy(cost))
        {
            gameManager.instance.Buying(cost);
            InventoryManager.instance.plrInventoryReference.AddItemToBag(@object);
            //gameManager.instance.User.GetComponent<WeaponInventory>().AddWeapon(@object.GetComponent<ItemInstance>().GetComponent<ItemDefinition>().GetComponent<WeaponData>());
        }
        if (@object.CompareTag("WeaponPickup"))
        {
            Debug.Log("Interacting with: " + @object.name);
            InventoryManager.instance.plrInventoryReference.AddItemToBag(@object);
            InventoryManager.instance.ChangePrimarySprite(@object.GetComponent<InGameItem>());
            gameObject.SetActive(false);
            //gameManager.instance.User.GetComponent<WeaponInventory>().AddWeapon(@object.GetComponent<WeaponBehaviour>().GetComponent<WeaponData>());
        }
        if (@object.CompareTag("ConsumablePickup"))
        { 
            Debug.Log("Interacting with: " + @object.name);
            InventoryManager.instance.plrInventoryReference.AddItemToBag(@object);
            gameObject.SetActive(false);
        }
        if (@object.CompareTag("HubChest"))
        {
            gameManager.instance.OpenHubChest();
        }
        if(@object.CompareTag("Extraction"))
        {
            gameManager.instance.Extraction();
        }
        if(@object.CompareTag("ExitTown"))
        {
            gameManager.instance.BackToHUB();
        }
    }

}

using UnityEngine;

public class Bandage : MonoBehaviour
{
    [Header("References")]
    [SerializeField] EquipmentComponent playerEquipment;  
    [SerializeField] PlayerHealth playerHealth;              

    [Header("Bandage Settings")]
    [SerializeField] string bandageName = "Bandage";
    [SerializeField] int healAmount = 20;



    void Update()
    {
        if (playerHealth == null && gameManager.instance.User != null)
        {
            playerHealth = gameManager.instance.User.GetComponent<PlayerHealth>();

            if (playerEquipment == null && InventoryManager.instance.equip != null)
            {
                playerEquipment = InventoryManager.instance.equip;
            }
        }

    }

    public void UseBandage()
    {
        for (int i = 0; i < playerEquipment.consumables.Count; i++)
        {
            GameObject item = playerEquipment.consumables[i];

            if (item != null && item.name.Contains(bandageName))
            {
                playerHealth.TakeHealing(healAmount); 

                playerEquipment.consumables.RemoveAt(i);

                return;
            }
        }

    }


    public PlayerHealth GetHealthComponent()
    {
        return playerHealth;
    }

    public EquipmentComponent GetPlayerEquipment()
    {
        return playerEquipment;
    }
}
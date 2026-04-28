using UnityEngine;

public class ShopPopup : MonoBehaviour
{
    [SerializeField] GameObject interact;


    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            interact.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            interact.SetActive(false);
        }
    }
}

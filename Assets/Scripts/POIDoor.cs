using UnityEngine;
using UnityEngine.SceneManagement;

public class POIDoor : MonoBehaviour, IInteract
{
    [Header("Door Identity")]
    [SerializeField] private string thisPOIName;
    [SerializeField] private GameObject interactUI;

    private bool isPlayerInRange;

    private void Update()
    {
        if (isPlayerInRange && Input.GetButtonDown("Interact")) Interact();
    }
    public void Interact()
    {
        CheckAndEnter();
    }

    private void CheckAndEnter()
    {
        if (gameManager.instance == null) return;

        if(gameManager.instance.selectedPOI == thisPOIName)
        {
            SceneManager.LoadScene(thisPOIName);
        }
        else
        {
            //add door locked stuff here
            Debug.Log("Door is locked: Not contract target.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            if(interactUI != null) interactUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if(interactUI != null) interactUI.SetActive(false);
        }
    }

   
}

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditScroll : MonoBehaviour
{
    [SerializeField] float scrollSpeed;
    void Update()
    {
        transform.Translate(Camera.main.transform.up * scrollSpeed * Time.deltaTime);
        if(Input.GetButtonDown("Cancel"))
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    
}

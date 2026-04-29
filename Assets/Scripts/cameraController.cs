using UnityEngine;

public class cameraController : MonoBehaviour
{

    [SerializeField] float Sensitivity;
    [SerializeField] int lockVertMin, lockVertMax;
    [SerializeField] bool invertY;
    [SerializeField] Transform player;

    float camRotX;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * Sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * Sensitivity * Time.deltaTime;

        if(invertY)
        {
            camRotX += mouseY;
        }
        else
        {
            camRotX -= mouseY;
        }

        camRotX = Mathf.Clamp(camRotX, lockVertMin, lockVertMax);
        transform.localRotation = Quaternion.Euler(camRotX, 0, 0);

        player.transform.Rotate(Vector3.up * mouseX);
    }
}

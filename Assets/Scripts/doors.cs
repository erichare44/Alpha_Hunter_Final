using UnityEngine;

public class doors : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    [SerializeField] Transform hinge;
    [SerializeField] GameObject button;
    [SerializeField] float openAngle;
    [SerializeField] float openSpeed;

    bool isOpen;

    Quaternion closedRotation;
    Quaternion openRotation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        closedRotation = hinge.rotation;
        openRotation = Quaternion.Euler(0, openAngle, 0) * closedRotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (isOpen)
        {
            hinge.rotation = Quaternion.Lerp(hinge.rotation, openRotation, Time.deltaTime * openSpeed);
        }
        else
        {
            hinge.rotation = Quaternion.Lerp(hinge.rotation, closedRotation, Time.deltaTime * openSpeed);
        }
    }

    public void Interact()
    {
        isOpen = !isOpen;
    }
}

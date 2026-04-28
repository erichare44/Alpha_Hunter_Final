using UnityEngine;

public class ThirdPersonCameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;

    [Header("Camera Settings")]
    [SerializeField] private float mouseSensitivity = 140f;
    [SerializeField] private float shoulderOffset = 0.6f;
    [SerializeField] private float lookAtHeight = 1.45f;
    [SerializeField] private float minPitch = -35f;
    [SerializeField] private float maxPitch = 45f;
    [SerializeField] private float followSmoothSpeed = 12f;

    [Header("Aim Camera")]
    [SerializeField] private ThirdPersonMotor playerMotor;
    [SerializeField] private float normalDistance = 1.55f;
    [SerializeField] private float aimDistance = 1.0f;
    [SerializeField] private float normalScreenOffset = 0.35f;
    [SerializeField] private float aimScreenOffset = 0.55f;
    [SerializeField] private float cameralLerpSpeed = 12f;

    [Header("Shake")]
    [SerializeField] private float shakeDuration;
    [SerializeField] private float shakeMagnitude;


    private float currentShakeTime;
    private float yaw;
    private float pitch;
    private float currentDistance;
    private float currentScreenOffset;

    public Vector3 FlatForward
    {
        get
        {
            Vector3 forward = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
            return forward.normalized;
        }
    }

    public Vector3 FlatRight
    {
        get
        {
            Vector3 right = Quaternion.Euler(0f, yaw, 0f) * Vector3.right;
            return right.normalized;
        }
    }

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
        currentDistance = normalDistance;
        currentScreenOffset = normalScreenOffset;
    }

    private void LateUpdate()
    {
        float targetDistance = normalDistance;
        float targetScreenOffset = normalScreenOffset;
        if (target == null)
            return;

        if (playerMotor != null && playerMotor.IsAiming)
        {
            targetDistance = aimDistance;
            targetScreenOffset = aimScreenOffset;
        }

        currentDistance = Mathf.Lerp(currentDistance, targetDistance, cameralLerpSpeed * Time.deltaTime);
        currentScreenOffset = Mathf.Lerp(currentScreenOffset, targetScreenOffset, cameralLerpSpeed * Time.deltaTime);

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 baseLookTarget = target.position + Vector3.up * lookAtHeight;
        Vector3 shiftedLookTarget = baseLookTarget + (rotation * Vector3.right) * currentScreenOffset;
        Vector3 desiredCameraPosition = baseLookTarget + rotation * new Vector3(shoulderOffset, 0f, -currentDistance);
        Vector3 shakeOffset = Vector3.zero;

        if (currentShakeTime > 0f)
        {
            shakeOffset = Random.insideUnitSphere * shakeMagnitude;
            currentShakeTime -= Time.deltaTime;
        }

        Vector3 finalPosition = desiredCameraPosition + shakeOffset;

        transform.position = Vector3.Lerp(
            transform.position,
            finalPosition,
            followSmoothSpeed * Time.deltaTime
        );

        transform.rotation = Quaternion.LookRotation(shiftedLookTarget - transform.position);
    }

    public void TriggerShake(float duration, float magnitude)
    {
        currentShakeTime = duration;
        shakeMagnitude = magnitude;
    }

}
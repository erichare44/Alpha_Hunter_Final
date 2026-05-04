using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonMotor : MonoBehaviour 
{

    [Header("ShootingComponent.cs")]
    //[SerializeField] private ShootingComponent shootincComponentRef;

    [Header("References")]
    [SerializeField] private ThirdPersonCameraController cameraController;
    [SerializeField] private Animator animator;

    [Header("Movement")]
    [SerializeField] public float moveSpeed = 4f;
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] public float sprintSpeed = 6f;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Animation Damping")]
    //[SerializeField] private float animationDampTime = 0.1f;

    [Header("Dodge")]
    [SerializeField] private float dodgeSpeed = 8f;
    [SerializeField] private float dodgeDuration = 0.3f;
    [SerializeField] private float dodgeCooldown = 1f;
    private bool isDodging;
    private bool isInvincible;
    private float invincibilityTimer;

    [SerializeField] private float dodgeInvincibilityDuration = 0.2f;
    [SerializeField] private KeyCode dodgeKey = KeyCode.LeftAlt;

    [Header("Aim")]
    [SerializeField] private KeyCode aimkey = KeyCode.Mouse1;
    [SerializeField] private bool isAiming;
    [SerializeField] private Camera aimCamera;
    [SerializeField] private KeyCode shootKey = KeyCode.Mouse0;
    [SerializeField] WeaponManager weaponManager;

    [Header("NPC Interaction")]
    [SerializeField] float interactDistance;
    [SerializeField] LayerMask interactLayer;
    [SerializeField] Transform cameraTransform;

    [Header("Flashlight")]
    [SerializeField] private GameObject flashlightObject;
    [SerializeField] private KeyCode flashlightKey = KeyCode.F;
    private bool isFlashlightOn = true;

    [Header("Jump")]
    [SerializeField] float jumpHeight = 5f;
    [SerializeField] KeyCode jumpKey = KeyCode.Space;

    private CharacterController controller;
    private float verticalVelocity;
    //private float currentMoveX;
    //private float currentMoveY;
    private bool isSprinting;
  
    private float dodgeTimer;
    private Vector3 dodgeDirection;
    public Vector2 MoveInput { get; private set; }
    public Vector3 MoveDirection { get; private set; }
    public bool IsMoving => MoveInput.sqrMagnitude > 0.01f;
    public bool IsSprinting => isSprinting;
    public bool IsAiming => isAiming;

    private void Awake()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        if (players.Length > 1)
        {
            if (gameManager.instance.User != this.gameObject)
            {
                Destroy(this.gameObject);
                return;
            }
        }
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        if (gameManager.instance.SpawnPos != null)
        {
            SpawnPlayer(); 
        }
    }


    public void SpawnPlayer()
    {
        if(gameManager.instance.SpawnPos == null)
        {
            //Debug.LogWarning("Spawn PLayer failed.");
            return;
        }

        controller.enabled = false;
        transform.position = gameManager.instance.SpawnPos.transform.position;
        transform.rotation = gameManager.instance.SpawnPos.transform.rotation;

        Physics.SyncTransforms();
        controller.enabled = true;
        
        
    }

    private void Update()
    {
        if (cameraController == null)
        {
           // Debug.LogWarning("ThirdPersonMotor: No camera controller assigned.");
            return;
        }
        ReadMovementInput();
        HandleAim();
        HandleSprint();
        HandleFlashlight();
        HandleShoot();
        HandleJump();
        Interaction();
        HandleInvincibility();

        if (!isDodging && Input.GetKeyDown(dodgeKey))
        {
            StartDodge();
        }

        if(isDodging)
        {
            HandleDodge();
        }
        else
        {
            HandleRotation();
            HandleMovement();
        }
        UpdateAnimator();
    }

    private void ReadMovementInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector2 rawInput = new Vector2(horizontal, vertical);

        if (rawInput.magnitude > 1f)
            rawInput.Normalize();

        MoveInput = rawInput;
    }

    private void HandleRotation()
    {
        Vector3 forward = cameraController.FlatForward;

        if (forward.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(forward);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void HandleMovement()
    {
        Vector3 forward = cameraController.FlatForward;
        Vector3 right = cameraController.FlatRight;

        MoveDirection = forward * MoveInput.y + right * MoveInput.x;

        if (MoveDirection.magnitude > 1f)
            MoveDirection.Normalize();

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;

        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;
        Vector3 finalMove = MoveDirection * currentSpeed;
        finalMove.y = verticalVelocity;

        controller.Move(finalMove * Time.deltaTime);
    }

    private void UpdateAnimator()
    {
        if (animator == null)
            return;


        animator.SetFloat("MoveX", MoveInput.x);
        animator.SetFloat("MoveY", MoveInput.y);
        animator.SetBool("IsMoving", IsMoving);
        animator.SetBool("IsSprinting", isSprinting);
        animator.SetBool("IsAiming", isAiming);
        if(weaponManager != null)
        {
            GameObject equippedWeapon = weaponManager.GetEquippedWeaponObject();

            if (equippedWeapon != null)
            {
                WeaponBehaviour weapon = equippedWeapon.GetComponent<WeaponBehaviour>();

                if (weapon != null && weapon.weaponData != null)
                {
                    animator.SetInteger("WeaponGripType", (int)weapon.weaponData.weaponGripType);
                }
            }
        }
   
    }

    private void StartDodge()
    {
        isDodging = true;
        isInvincible = true;
        invincibilityTimer = dodgeInvincibilityDuration;

        dodgeTimer = dodgeDuration;
        Vector3 forward = cameraController.FlatForward;
        Vector3 right = cameraController.FlatRight;
        Vector3 inputDirection = forward * MoveInput.y + right * MoveInput.x;
        bool noInput = inputDirection.sqrMagnitude < 0.01f;

        if (!noInput)
        {
            dodgeDirection = inputDirection.normalized;
        }
        else
        {
            dodgeDirection = -forward; // Default to backward dodge if no input
        }
        if (animator != null)
        {
            if (noInput || MoveInput.y < -0.1f)
            {
                animator.SetTrigger("DodgeBackward");
            }
            else if (MoveInput.y > 0.1f)
            {
                animator.SetTrigger("DodgeForward");
            }
            else
            {
                if (MoveInput.x > 0)
                    animator.SetTrigger("DodgeRight");
                else
                    animator.SetTrigger("DodgeLeft");

               
            }
        }
    }

    private void HandleDodge()
    {
        dodgeTimer -= Time.deltaTime;

        if (controller.isGrounded&& verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;
        Vector3 dodgeMove = dodgeDirection * dodgeSpeed;
        dodgeMove.y = verticalVelocity;
        controller.Move(dodgeMove * Time.deltaTime);
        if (dodgeTimer <= 0f)
        {
            isDodging = false;
        }
    }

    private void HandleSprint()
    {
        bool pressingSprint = Input.GetKey(sprintKey);
        bool movingForward = MoveInput.y > 0.1f;
        bool notStrafingMuch = Mathf.Abs(MoveInput.x) < 0.1f;

        isSprinting = pressingSprint && movingForward && notStrafingMuch && !isDodging;
    }
    
    private void HandleAim()
    {
    
        isAiming = Input.GetKey(aimkey) && !isDodging;
    
    }

    private void HandleShoot()
    {

        if (!isAiming)
        {
            return;
        }

        if (!Input.GetKeyDown(shootKey))
        {
            return;
        }

        if (aimCamera == null || weaponManager == null)
        {
            return;
        }
        GameObject equippedWeapon = weaponManager.GetEquippedWeaponObject();

        if (equippedWeapon == null)
        {
            return;
        }
        WeaponBehaviour weapon = equippedWeapon.GetComponent<WeaponBehaviour>();

        if (weapon == null)
        {
            return;
        }

        bool automatic = weapon.weaponData.isAutomatic 
            ? Input.GetKey(KeyCode.Mouse0) : Input.GetKeyDown(KeyCode.Mouse0);
        Ray ray = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, weapon.weaponData.range))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.origin + ray.direction * weapon.weaponData.range;
        }
        weapon.UseWeapon(targetPoint);
    }

        void Interaction()
    {
        if (Input.GetButtonDown("Interact"))
        {
            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactDistance, interactLayer))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();

                if (interactable != null)
                {
                    interactable.Interact();
                }
            }
        }
    }
    private void HandleJump()
    {
        if(animator != null)
        {
            animator.SetBool("IsGrounded", controller.isGrounded);
            
        }
        if (!controller.isGrounded)
            return;
        if (Input.GetKeyDown(jumpKey))
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

            if (animator != null)
            {
                animator.SetTrigger("Jump");
            }
        }
    }
    private void HandleFlashlight()
    {
        if(Input.GetKeyDown(flashlightKey))
        {
            if(flashlightObject != null)
            {
                isFlashlightOn = !isFlashlightOn;
                flashlightObject.SetActive(isFlashlightOn);
            }
        }
    }
private void HandleInvincibility()
    {
        if(!isInvincible)
        {
            return;
        }

        invincibilityTimer -= Time.deltaTime;
        if(invincibilityTimer <= 0f)
        {
            isInvincible = false;
        }
    }

}
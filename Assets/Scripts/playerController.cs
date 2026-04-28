using System.Collections;
using UnityEngine;


public class playerController : MonoBehaviour, IDamage
{

    [SerializeField] CharacterController controller;

    [Header("----- Player Stats -----")]
    [SerializeField] float HP;
    [SerializeField] float Speed;
    [SerializeField] float sprintMod;
    [SerializeField] int jumpSpeed;
    [SerializeField] int jumpTimesMax;
    [SerializeField] int gravity;

    //---WEAPON STATS FOR TESTING--
    [SerializeField] float shootDamage;
    [SerializeField] float shootDist;
    [SerializeField] float shootRate;
    [SerializeField] GameObject flashLightObject;
    bool isShooting;
    public bool isFlashLightOn;
    //--ABOVE FOR TESTING--

    int jumpCount;
    //public int skillPointAmount;
    //public int currentLevel;
    //public int currentMoneyAmount;
    //public int possibleMoney;

    //float HPOriginal;
    //float XPHas;            //Amount player has outside level
    //public float XPPotential;      //Amount player could leave level with
    //float XPLevelAmount;    //Amount needed to reach next level
    //float XPModifierExtract;
    //float XPModifierDied;


    bool isSprinting;
    //bool alphaKilled;
    //bool hasExtracted;

    Vector3 moveDir;
    Vector3 playerVel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //HPOriginal = HP;
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
        Sprint();

        //--TOGGLE FLASHLIGHT FOR TESTING--
        if(Input.GetKeyDown(KeyCode.F))
        {
            isFlashLightOn = !isFlashLightOn;
            if (flashLightObject != null) flashLightObject.SetActive(isFlashLightOn);
        }
        //--COROUTINE FOR SHOOT TESTING
        if (Input.GetButton("Fire1") && !isShooting)
        {
            StartCoroutine(shoot());
        }
    }
    //--SHOOT FUNCTION FOR TESTING--
    IEnumerator shoot()
    {
        isShooting = true;

        RaycastHit hit;
        // Shoots from the center of the screen/camera forward
        if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f)), out hit, shootDist))
        {
            Debug.Log("Hit: " + hit.collider.name);

            // Check if what we hit has the IDamage interface
            IDamage dmg = hit.collider.GetComponent<IDamage>();

            if (dmg != null)
            {
                dmg.TakeDamage(shootDamage);
            }
        }

        yield return new WaitForSeconds(shootRate);
        isShooting = false;
    }
    void Movement()
    {
        if(controller.isGrounded)
        {
            jumpCount = 0;
            playerVel.y = 0;
        }

        moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        controller.Move(moveDir * Speed * Time.deltaTime);

        Jump();
        controller.Move(playerVel * Time.deltaTime);
        playerVel.y -= gravity * Time.deltaTime;
    }

    void Jump()
    {
        if(Input.GetButtonDown("Jump") && jumpCount < jumpTimesMax)
        {
            playerVel.y = jumpSpeed;
            jumpCount++;
        }
    }

    void Sprint()
    {
        if(Input.GetButtonDown("Sprint"))
        {
            Speed *= sprintMod;
            isSprinting = true;            
        }
        if(Input.GetButtonUp("Sprint"))
        {
            Speed /= sprintMod;
            isSprinting = false;            
        }
    }

    

    
    //--PLAYER TAKE DAMAGE AND DIE FOR TESTING--
    public void TakeDamage(float amount)
    {
        HP -= amount;
        Debug.Log("Player took damage. Current HP: " + HP);

        if(HP <= 0)
        {
            Die();
        }
    }
    void Die()
    {
        Debug.Log("Player has died.");
        controller.enabled = false;
        controller.enabled = false;
    }


}

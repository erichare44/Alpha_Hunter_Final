using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class werewolfController : monsterAI
{
    public enum WolfState { Idle, Stalking, Charging, Combo, Fleeing, }

    [Header("Behavior State")]
    public WolfState currentState = WolfState.Idle;
    private float stateTimer;
    private Vector3? alphaCombatPos = null;
    private Vector3 patrolTarget;
    private Vector3 chargeDirection;
    bool hasDealtDamage = false;
    private bool isDead = false;
    private int comboStep = 0;
    private float comboTimer;

    [Header("Stagger Settings")]
    public float staggerThreshold = 30f;
    private float currentStaggerDamage = 0f;

    [Header("Animations")]
    private Animator animator;

    

    public override void SetupMonster(BoxCollider zone, bool alpha, bool guard)
    {
        base.SetupMonster(zone, alpha, guard);
        animator = GetComponent<Animator>();

        agent.angularSpeed = 1000f;
        agent.autoBraking = true;
        agent.stoppingDistance = 0f; //for smooth circling

        
        
    }
    private void Update()
    {
        // safety
        if (isDead || data == null || player == null) return;

        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            if (agent.pathStatus == NavMeshPathStatus.PathPartial || agent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                agent.ResetPath(); 
            }
        }

        stateTimer += Time.deltaTime;
        
        //patrol zone if not active
        if (!isZoneActive && alphaCombatPos == null)
        {
            HandlePatrol();
            UpdateAnimations();
            return;
        }

        //if not in alpha event, check to deactivate
        if(alphaCombatPos == null)
        {
            float distToZone = Vector3.Distance(transform.position, myZoneCollider.transform.position);

            //player moves from zone, deactivate
            if (distToZone > 40f)
            {
                isZoneActive = false;
                SwitchState(WolfState.Idle);
                return;
            }
        }

        

        //behavior switch statement
        switch(currentState)
        {
            case WolfState.Idle: SwitchState(WolfState.Stalking); break;
            case WolfState.Stalking: HandleStalking(); break;
            case WolfState.Charging: HandleCharge(); break;
            case WolfState.Combo: HandleCombo(); break;           
        }
        HandleFootstepLogic(animator);
        UpdateAnimations();
    }

    public override void SetZoneActivity(bool active)
    {
        if(active && !isZoneActive)
        {
            isZoneActive = true;
            if(isAlpha)
            {
                PlayAlertSound();
                if(nestManager.instance != null)
                {
                    nestManager.instance.CallAllWolvesToAlpha(transform.position);
                }
            }
            SwitchState(WolfState.Stalking);
        }
        else if(!active && alphaCombatPos == null)
        {
            isZoneActive = false;
            SwitchState(WolfState.Idle);
        }
    }

    // BEHAVIORS

    void HandleStalking()
    {
        agent.speed = data.runSpeed;
        float distToPlayer = Vector3.Distance(transform.position, player.position);

        //stalk with offset
        Vector3 offsetDir = Quaternion.Euler(0, 45f, 0) * (transform.position - player.position).normalized;
        Vector3 targetPos = player.position + (offsetDir * data.aggroRadius);

        if (Vector3.Distance(agent.destination, targetPos) > 1.5f)
        {
            agent.SetDestination(targetPos);
        }

        //alpha combo trigger
        if(isAlpha && stateTimer >= data.patienceMeter)
        {
            //50% chance to do combo
            float dist = Vector3.Distance(transform.position, player.position);
            if(dist < data.attackRange + 2f && Random.value > 0.5f)
            {
                SwitchState(WolfState.Combo);
                return;
            }
        }
        //transition to lunge
        if (stateTimer >= data.patienceMeter)
        {
            SwitchState(WolfState.Charging);
        }
    }

    void HandleCharge()
    {
        if (isDead) return;

        if (stateTimer < 0.1f)
        {
            chargeDirection = (player.position - transform.position).normalized;
        }
        //allow for minor steering during lunge
        if (stateTimer < 0.5f && !agent.isStopped)
        {
            Vector3 realTimeDir = (player.position - transform.position).normalized;
            chargeDirection = Vector3.Slerp(chargeDirection, realTimeDir, Time.deltaTime * 4f);
        }
        if (!agent.isStopped)
        {
            agent.velocity = chargeDirection * agent.speed;
        }
        else agent.velocity = Vector3.zero;

        // agent.SetDestination(player.position);

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        //check for hit on player
        if (distToPlayer <= data.attackRange && !hasDealtDamage)
        {
            hasDealtDamage = true;
            animator.SetTrigger("Attack");
            //stop wolf to play animation
            agent.isStopped = true;
            agent.velocity = Vector3.zero;

            IDamage health = player.GetComponent<IDamage>();

            if (health != null)
            {
                health.TakeDamage(data.attackDamage);
            }

            if (data.attackSound != null)
            {
                AudioSource.PlayClipAtPoint(data.attackSound, transform.position);
            }
                        
            //give time for animation to finish
            Invoke("FinishAttack", 1.0f);


        }
        if (stateTimer > 1.5f)
        {
            SwitchState(WolfState.Stalking);
        }
    }

    //void HandleFlee()
    //{
    //    agent.speed = data.runSpeed;
    //    Vector3 fleeDir = (transform.position - player.position).normalized;
    //    agent.SetDestination(transform.position + fleeDir * 25f);

    //    //if not wounded return to pack, if wounded flee longer( in takedamage)
    //    if (stateTimer > 5f && currentHealth > (data.health * 0.3f)) SwitchState(WolfState.Circling);

    //}

    void HandleCombo()
    {
        //stop moving during the combo
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        //face the player so he doesn't whiff the hits
        Vector3 lookPos = player.position;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);

        if (stateTimer < 0.1f)
        {
            animator.SetTrigger("ComboAttack"); //trigger your 3-hit animation

            //schedule the 3 hits (adjust these times to match your animation)
            Invoke("ApplyComboDamage", 0.2f);
            Invoke("ApplyComboDamage", 1.1f);
            Invoke("ApplyComboDamage", 2.05f);
        }

        //exit state after the animation finishes (e.g., 2 seconds)
        if (stateTimer > 2.0f)
        {
            agent.isStopped = false;
            SwitchState(WolfState.Stalking);
        }
    }
    void ApplyComboDamage()
    {
        if (isDead) return;

        float dist = Vector3.Distance(transform.position, player.position);
        // Only deal damage if player is still in front of the Alpha
        if (dist <= data.attackRange + 1.5f)
        {
            player.GetComponent<IDamage>()?.TakeDamage(data.attackDamage * 0.7f); // Hits for slightly less since there are 3

            if (data.attackSound != null)
                generalAudioSource.PlayOneShot(data.attackSound, 0.5f);
        }
    }

    
    protected override void HandlePatrol()
    {
        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh) return;

        agent.speed = data.runSpeed;

        if (!agent.pathPending && agent.remainingDistance < 1f)
        {
            patrolTarget = monsterAI.GetRandomNavMeshPos(myZoneCollider);
            agent.SetDestination(patrolTarget);

        }
    }
    public void SetForcedAlphaCombat(Vector3 pos)
    {
        alphaCombatPos = pos;
        isZoneActive = true;
        SwitchState(WolfState.Stalking);
    }

    private void SwitchState(WolfState newState)
    {
        if (currentState == WolfState.Combo)
        {
            CancelInvoke("ApplyComboDamage");
        }
        currentState = newState;
        stateTimer = 0;
        //currentStaggerDamage = 0f; //reset stagger progress on state change
       
        hasDealtDamage = false;
        //dont have lazy bad guys
        if (agent.isActiveAndEnabled && agent.isActiveAndEnabled)
        {
            agent.isStopped = false;
            if(newState != WolfState.Charging)
            {
                agent.ResetPath();
            }
            
        }
                
    }
   
   
    void UpdateAnimations()
    {
        if (animator == null) return;

        //tell animator how fast wolf is moving
        float currentSpeed = agent.desiredVelocity.magnitude;
        animator.SetFloat("Speed", currentSpeed);

        animator.SetBool("isPatrolling", !isZoneActive);
        //triggers for states
        animator.SetBool("isCharging", currentState == WolfState.Charging);      

    }
    void FinishAttack()
    {
        hasDealtDamage = false;
        if(agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
        
        SwitchState(WolfState.Stalking);
    }

   
    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount);

        if(currentHealth > 0)
        {
            //if below 30% enter persistent fleeing state
            if (currentHealth < (data.health * 0.3f)) SwitchState(WolfState.Fleeing);
            else if(currentState == WolfState.Charging)
            {
                //stagger logic
                currentStaggerDamage += amount;

                if(currentStaggerDamage >= staggerThreshold) SwitchState(WolfState.Fleeing);
            }
        }
    }
    public override void Die()
    {
        //trigger animation
        if (animator != null) animator.SetTrigger("Die");
        //disable wolf
        agent.enabled = false;

        base.Die();        
        this.enabled = false;

        GetComponent<Collider>().enabled = false;
    }
   
    //private void OnDestroy()
    //{
    //    //if vamp is destroyed for any reason and was attacker, attacker slot needs to open up
    //    if (isAttacker && nestManager.instance != null)
    //    {
    //        nestManager.instance.ReleaseAttackerSlot();
    //    }
    //}

}

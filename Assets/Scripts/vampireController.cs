using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;


public class vampireController : monsterAI
{
    public enum VampireState { Idle, Prowling, Lunging, Fleeing, Drain }  
    
    [Header("Behavior State")]
    public VampireState currentState = VampireState.Idle;    

    [Header("Timing/Location Settings")]    
    private float stateTimer;
    private float outOfSightTimer;    
    private Vector3 lungeDirection;
    private Vector3 patrolTarget;
    private bool hasDealtDamage = false;
    private bool isDead = false;

    [Header("Animations")]
    private Animator animator;

    [Header("Alpha Settings")]
    public float drainRange = 15f;
    public float drainTickRate = 0.5f;
    private float nextDrainTick;

    public override void SetupMonster( BoxCollider zone, bool alpha, bool guard)
    {
        base.SetupMonster(zone, alpha, guard);
        animator = GetComponent<Animator>();


        //vampire specific variation
        agent.angularSpeed = 1500f;
        agent.autoBraking = true;
        agent.stoppingDistance = 0f;
               
    }
       
    void Update()
    {
        //safety check
        if(isDead || data == null || player == null) return;

        bool isSeen = CheckIfSeen();
        stateTimer += Time.deltaTime;

        //wake up if looked at
        if (isSeen && !isZoneActive) SetZoneActivity(true);
        //patrol if player is not in zone
        if(!isZoneActive)
        {
            HandlePatrol();
            UpdateAnimations();
            return;
        }
        //if seen flee
        if (isSeen && currentState == VampireState.Prowling)
            if (stateTimer > 0.3f)
            {
                if (isAttacker) SwitchState(VampireState.Lunging);
                else SwitchState(VampireState.Fleeing);
            }
                        
               
        switch(currentState)
        {
            case VampireState.Idle: SwitchState(VampireState.Prowling); break;
            case VampireState.Prowling: HandleProwling(); break;
            case VampireState.Lunging: HandleLunge(); break;
            case VampireState.Fleeing: HandleFlee(isSeen); break;   
            case VampireState.Drain: HandleDrain(); break;
        }
        HandleFootstepLogic(animator);
        UpdateAnimations();
    }

    //OVERRIDES FROM MONSTER AI
    public override void SetZoneActivity(bool active)
    {
        //once zone is active vamp stays active
        if(active && !isZoneActive)
        {
            isZoneActive = true;
            stateTimer = 0;

            SwitchState(VampireState.Prowling);
        }   
    }
    protected override void HandlePatrol()
    {
        if(CheckIfSeen())
        {
            SwitchState(VampireState.Fleeing);
            return;
        }
        agent.speed = data.walkSpeed;
        if (agent.remainingDistance < 1f)
        {
            patrolTarget = monsterAI.GetRandomNavMeshPos(myZoneCollider);
             
            agent.SetDestination(patrolTarget);

        }
    }
    
    //BEHAVIOR LOGIC
    void HandleProwling()
    {
        if(!isAttacker)
        {
            if (nestManager.instance.RequestAttackerSlot())
            {
                isAttacker = true;
                agent.ResetPath();
            }
        }
        //maintain distance behind player
        float stalkDist = isAttacker ? data.aggroRadius : data.sightRadius;
        Vector3 targetPos = player.position - (player.forward * stalkDist);

        if (Vector3.Distance(agent.destination, targetPos) > 1.5f)
        {
            agent.SetDestination(targetPos);
        }
        //transition to lunge
        if(isAttacker && stateTimer >= data.patienceMeter)
        {
            SwitchState(VampireState.Lunging);
        }
        //chance for alpha to drain instead of prowl
        if (isAlpha && !CheckIfSeen() && stateTimer > 3f)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist < drainRange && dist > data.attackRange + 5f)
            {
                SwitchState(VampireState.Drain);
                return;
            }
        }
    }
    void HandleFlee(bool isSeen)
    {
        if (isSeen)
        {
            outOfSightTimer = 0;
            //run to a point away from player
            if(agent.remainingDistance < 2f || !agent.hasPath)
            {
                Vector3 fleeDir = (transform.position - player.position).normalized;
                Vector3 desiredPos = transform.position + fleeDir * 25f;

                NavMeshHit hit;
                if(NavMesh.SamplePosition(desiredPos, out hit, 15f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
                else
                {
                    //if invalid try within zone
                    agent.SetDestination(monsterAI.GetRandomNavMeshPos(myZoneCollider));
                }
            }             
        }
        else
        {
            outOfSightTimer += Time.deltaTime;
            //break los and blink then prowl
            if(outOfSightTimer >= 1.5f)
            {
                Blink();
                SwitchState(VampireState.Prowling);
            }
        }
    }     

    void HandleLunge()
    {
        if (isDead) return;

        if (stateTimer < 0.1f)
        {
            lungeDirection = (player.position - transform.position).normalized;
        }
        //allow for minor steering during lunge
        if(stateTimer < 0.5f && !agent.isStopped)
        {
            Vector3 realTimeDir = (player.position - transform.position).normalized;
            lungeDirection = Vector3.Slerp(lungeDirection, realTimeDir, Time.deltaTime * 4f);
        }
        if (!agent.isStopped)
        {
            agent.velocity = lungeDirection * agent.speed;
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
            //release from isattacekr so another wolf can become attacker
            if (isAttacker)
            {
                nestManager.instance.ReleaseAttackerSlot();
                isAttacker = false;
            }
            //give time for animation to finish
            Invoke("FinishAttack", 1.0f);

        }
        if (stateTimer > 1.5f)
        {
            if (isAttacker)
            {
                nestManager.instance.ReleaseAttackerSlot();
                isAttacker = false;
            }
            SwitchState(VampireState.Fleeing);
        }
    }
    void HandleDrain()
    {
        //stop the Vampire from moving
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        float dist = Vector3.Distance(transform.position, player.position);
        bool hasLOS = !Physics.Linecast(transform.position + Vector3.up, player.position + Vector3.up, LayerMask.GetMask("Environment"));

        // BREAK CONDITIONS: Too far, player hides, or state timer ends
        if (dist > drainRange || !hasLOS || stateTimer > 4.0f)
        {
            agent.isStopped = false;
            SwitchState(VampireState.Fleeing);
            return;
        }

        //facing the player
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        //tick damage and heal
        if (Time.time >= nextDrainTick)
        {
            nextDrainTick = Time.time + drainTickRate;

            //damage Player
            float damage = 5f;
            player.GetComponent<IDamage>()?.TakeDamage(damage);

            //heal self (Alpha)
            currentHealth = Mathf.Min(currentHealth + (damage * 1.5f), data.health);

            //play Sound
            if (data.attackSound != null)
                generalAudioSource.PlayOneShot(data.attackSound, 0.5f);
            
            animator.SetTrigger("Drain");
        }
        LineRenderer lr = GetComponent<LineRenderer>();
        if (lr != null)
        {
            lr.enabled = true;
            lr.positionCount = 2;
            lr.SetPosition(0, transform.position + Vector3.up * 1.5f); //vamp Chest
            lr.SetPosition(1, player.position + Vector3.up * 1f);     //player Chest
        }
    }
    void Blink()
    {        
        //warp behind player
        
        Vector3 blinkPos = player.position - (player.forward * Random.Range(data.aggroRadius, data.sightRadius));
        NavMeshHit hit;
        if (NavMesh.SamplePosition(blinkPos, out hit, 10f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            transform.LookAt(player.position);
        }
    }
    
    void FinishAttack()
    {
        hasDealtDamage = false;
        agent.isStopped = false;
        SwitchState(VampireState.Fleeing);
    }

    bool CheckIfSeen()
    {  
        //use camera for fov
        Camera mainCam = Camera.main;
        //ingnore player with cast
        int layerMask = ~LayerMask.GetMask("Player");
        
        Vector3 camPos = mainCam.transform.position;
        Vector3 camForward = mainCam.transform.forward;

        Vector3 playerDir = (transform.position - camPos).normalized;
        float dot = Vector3.Dot(camForward, playerDir);

        if (dot > 0.5f)
        {
            Vector3[] checkpoints =
            {
                transform.position + Vector3.up * 1.8f, //head
                transform.position + Vector3.up * 1.0f, //chest
                transform.position + Vector3.up * 0.2f, //feet
            }; 
            foreach (Vector3 point in checkpoints)
            {
                RaycastHit hit;
                //lets try a spere cast
                if (Physics.Linecast(camPos, transform.position + Vector3.up * 1.5f, out hit, layerMask))
                {
                    if (hit.transform.root == transform.root) return true;
                }
            }
           
           
        }
        return false;
    }
    void SwitchState(VampireState newState)
    {
        currentState = newState;
        stateTimer = 0; 
        outOfSightTimer = 0;
        agent.ResetPath();
        
        
        if (newState == VampireState.Prowling)
        {
            agent.speed = data.walkSpeed * 2f;
            agent.acceleration = 50f;
        }
        else if (newState == VampireState.Fleeing)
        {
            if(isAttacker)
            {
                nestManager.instance.ReleaseAttackerSlot();
                isAttacker = false;
            }
            agent.speed = data.runSpeed * 1.8f;
            agent.acceleration = 120f;
        }
        else if (newState == VampireState.Lunging)
        {
            agent.speed = data.runSpeed * 2f;
            agent.acceleration = 150f;
        }
        LineRenderer lr = GetComponent<LineRenderer>();
        if (lr != null)
        {
            lr.enabled = false;
            lr.positionCount = 0;
        }
    }
    void UpdateAnimations()
    {
        if (animator == null || isDead) return;

        float speedForAnimator = hasDealtDamage ? 0f : agent.velocity.magnitude;
        animator.SetFloat("Speed", speedForAnimator);
        animator.SetBool("isLunging", currentState == VampireState.Lunging);       
    }
    //void FinishAttack()
    //{
    //    hasDealtDamage = false;
    //    agent.isStopped = false;
    //    SwitchState(VampireState.Prowling);
    //}

    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount);

        if (currentHealth > 0)
        {
            agent.speed = data.runSpeed;
            agent.acceleration = 250f;
            agent.angularSpeed = 1500f;

            SwitchState(VampireState.Fleeing);
        }
    }
    public override void Die()
    {
        if(isDead) return;
        isDead = true;

        if(agent != null)
        {
            agent.velocity = Vector3.zero;
            agent.isStopped = true;
            agent.enabled = false;
        }
        CancelInvoke("FinishAttack");
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetTrigger("Die");
        }
        if(animator != null)
        {
            animator.SetBool("isLunging", false);
            animator.SetFloat("Speed", 0f);
            animator.Play("Idle");
            animator.SetTrigger("Die");
        }
        
        base.Die();        
        this.enabled = false;
        GetComponent<Collider>().enabled = false;
    }
   
    private void OnDestroy()
    {
        //if vamp is destroyed for any reason and was attacker, attacker slot needs to open up
        if(isAttacker && nestManager.instance != null)
        {
            nestManager.instance.ReleaseAttackerSlot();
        }
    }
}

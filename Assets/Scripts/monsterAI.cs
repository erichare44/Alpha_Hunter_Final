using UnityEngine;
using UnityEngine.AI;

public abstract class monsterAI : MonoBehaviour, IDamage
{
    [Header("Base References")]
    public monsterData data;
    [HideInInspector] public BoxCollider myZoneCollider;    
    [HideInInspector] public NavMeshAgent agent;
    protected Transform player;

    [Header("Universal Rank & State")]
    public bool isAlpha = false;
    public bool isGuard = false;
    [HideInInspector] public bool isZoneActive = false;
    [HideInInspector] public bool isAttacker = false;
    public float currentHealth;

    [Header("Footstep Logic")]
    protected AudioSource footstepAudioSource;
    protected AudioSource generalAudioSource;
    private bool hasStepped;

    float OGhealth;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        
        // create source for general sounds
        generalAudioSource = GetComponent<AudioSource>();
        if(generalAudioSource == null) generalAudioSource = gameObject.AddComponent<AudioSource>();
        //create source for footsteps
        footstepAudioSource = gameObject.AddComponent<AudioSource>();
        //3d sound
        footstepAudioSource.spatialBlend = 1.0f;
        footstepAudioSource.playOnAwake = false;
        //find player
        GameObject plyr = GameObject.FindGameObjectWithTag("Player");
        if (plyr != null) player = plyr.transform;

        if(data != null) OGhealth = data.health;
    }
    public virtual void SetupMonster( BoxCollider zone, bool alpha, bool guard)
    {
        myZoneCollider = zone;
        isAlpha = alpha;
        isGuard = guard;

        if (agent == null) agent = GetComponent<NavMeshAgent>();

        if (data !=null && agent != null)
        {
            currentHealth = data.health;
            agent.speed = data.walkSpeed;
            agent.stoppingDistance = data.attackRange;
        }        
    }
    protected void HandleFootstepLogic(Animator anim)
    {
        if (anim == null || data == null) return;

        float footValue = anim.GetFloat("Footstep");

        if (footValue > 0.8f && !hasStepped)
        {
            PlayFootstepSound();
            hasStepped = true;
        }
        else if (footValue < 0.1f) hasStepped = false;
        
    }
    private void PlayFootstepSound()
    {
        if(data.footstepClips != null && data.footstepClips.Length > 0)
        {
            int index = Random.Range(0, data.footstepClips.Length);
            footstepAudioSource.PlayOneShot(data.footstepClips[index], data.footstepVolume);
        }
    }
    public virtual void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) Die();
    }
    public virtual void Die()
    {
        if(isAttacker && nestManager.instance != null)
        {
            nestManager.instance.ReleaseAttackerSlot();            
        }
        //remove from registry
        if(myZoneCollider != null && nestManager.instance.zoneRegistry.ContainsKey(myZoneCollider.gameObject))
        {
            nestManager.instance.zoneRegistry[myZoneCollider.gameObject].Remove(this);
        }
        float tempXP = OGhealth / 10;
        gameManager.instance.shopSystem.XPPotential += tempXP;
        Debug.Log("Enemy Killed: " + tempXP + " Earned");
        //if(isAlpha)
        //{
        //    gameManager.instance.shopSystem.alphaKilled = true;
        //}
        
    }
    protected bool IsPackEliminated()
    {
        if (myZoneCollider == null) return true;

        GameObject zoneObj = myZoneCollider.gameObject;
        if (nestManager.instance.zoneRegistry.ContainsKey(zoneObj))
        {
            foreach (monsterAI m in nestManager.instance.zoneRegistry[zoneObj])
            {
                if (m == null || m == this) continue;
                if (!m.isAlpha && !m.isGuard) return false;
            }
        }

        return true;
    }
    public static Vector3 GetRandomNavMeshPos(BoxCollider zoneCollider)
    {
        if (zoneCollider == null) return Vector3.zero;

        Bounds b = zoneCollider.bounds;

        int obstacleMask = LayerMask.GetMask("Default");

        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPos = new Vector3(Random.Range(b.min.x, b.max.x), b.min.y, Random.Range(b.min.z, b.max.z));

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPos, out hit, 5f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }
        NavMeshHit fallbackHit;
        NavMesh.SamplePosition(zoneCollider.transform.position, out fallbackHit, 10f, NavMesh.AllAreas);
        return fallbackHit.position;
    }
    public virtual void PlayAlertSound()
    {
        if (data != null && data.howlSound != null)
        {
            generalAudioSource.PlayOneShot(data.howlSound, data.howlVolume);
        }
    }
    //every monster defines its own wake up and patrol logic
    public abstract void SetZoneActivity(bool active);
    protected abstract void HandlePatrol();
    
    public virtual void AlphaDied()
    {
        if(isAlpha && data.health >= 0)
        {
            gameManager.instance.shopSystem.alphaKilled = true;
        }
    }
    
}

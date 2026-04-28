using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class nestManager : MonoBehaviour
{
    public static nestManager instance;

    [Header("Spawning Configuration")]
    public int alphaGuardCount = 2;
    public int ambientBetaCount = 1;
    public float betaSpawnRadius = 10f;

    [Header("Global Attacker Settings")]
    public int maxAttackers = 2;
    private int currentAttackers = 0;
    private float attackTimer;
    public float attackBuffer = 5f;

    //internal tracking
    private List<GameObject> nestZones = new List<GameObject>(); 
    public Dictionary<GameObject, List<monsterAI>> zoneRegistry = new Dictionary<GameObject, List<monsterAI>>();
    private Transform player;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        //find the player
        player = GameObject.FindGameObjectWithTag("Player").transform;

        //find all objects tagged nestzone
        GameObject[] foundZones = GameObject.FindGameObjectsWithTag("NestZone");
        List<GameObject> potentialAlphaNests = new List<GameObject>();

        foreach(GameObject zone in foundZones)
        {
            nestZones.Add(zone);
            zoneRegistry.Add(zone, new List<monsterAI>());

            //check if alpha nestzone
            if(zone.transform.Find("Alpha_Spawner") != null )
            {
                potentialAlphaNests.Add(zone);
            }
        }
        if (nestZones.Count == 0) return;

        //pick alpha nest
        GameObject chosenAlphaNest = null;
        if(potentialAlphaNests.Count > 0)
        {
            chosenAlphaNest = potentialAlphaNests[Random.Range(0, potentialAlphaNests.Count)];
        }

        //spawn monsters for every zone
        foreach(GameObject zone in nestZones)
        {
            bool isAlphaNest = (zone == chosenAlphaNest);
            int betasToSpawn = isAlphaNest ? alphaGuardCount : ambientBetaCount;

            if(isAlphaNest)
            {
                Transform alphaAnchor = zone.transform.Find("Alpha_Spawner");
                SpawnMonster(gameManager.instance.selectedMonster.alphaPrefab, alphaAnchor.position, zone, true, false);
            }

            for(int i = 0; i < betasToSpawn; i++)
            {
                Vector3 spawnPos = monsterAI.GetRandomNavMeshPos(zone.GetComponent<BoxCollider>());
                SpawnMonster(gameManager.instance.selectedMonster.betaPrefab, spawnPos, zone,false, isAlphaNest);
            }
        }        
    }
    private void Update()
    {
        if (player == null)
        {
            if(gameManager.instance != null && gameManager.instance.User != null)
            {
                player = gameManager.instance.User.transform;
            }
            return;
        }
        //central decection for zone triggers
        foreach(GameObject zone in nestZones)
        {
            BoxCollider col = zone.GetComponent<BoxCollider>();
            if (col == null) continue;
            
            Bounds bounds = col.bounds;
            bounds.Expand(0.5f);

            if (bounds.Contains(player.position + Vector3.up))
            {
                foreach (monsterAI monster in zoneRegistry[zone])
                {
                    if (monster != null && !monster.isZoneActive)
                    {
                        monster.SetZoneActivity(true);
                    }
                }
            }
        }
    }

    void SpawnMonster(GameObject prefab, Vector3 pos, GameObject zone, bool isAlpha, bool isGuard)
    {
        if (prefab == null) return;

        GameObject obj = Instantiate(prefab, pos, Quaternion.identity);
        monsterAI controller = obj.GetComponent<monsterAI>();

        if(controller != null)
        {
            zoneRegistry[zone].Add(controller);

            //set ranks
            controller.SetupMonster(zone.GetComponent<BoxCollider>(), isAlpha, isGuard);
        }
    }
    public bool RequestAttackerSlot()
    {
        if(currentAttackers < maxAttackers && Time.time > attackTimer)
        {
            currentAttackers++;
            attackTimer = Time.time + attackBuffer;
            return true;
        }
        return false;
    }
    public void ReleaseAttackerSlot()
    {
        currentAttackers = Mathf.Max(0, currentAttackers - 1);
    }
    public void CallAllWolvesToAlpha(Vector3 alphaPosition)
    {
        // Find every wolf in the scene
        werewolfController[] allWolves = Object.FindObjectsByType<werewolfController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (werewolfController wolf in allWolves)
        {
            // Don't tell the Alpha to go to their own position
            if (wolf.isAlpha) continue;

            // Wake them up and send them to the Alpha
            wolf.isZoneActive = true;
            wolf.SetForcedAlphaCombat(alphaPosition);
        }

        Debug.Log("<color=red>THE ALPHA HAS CALLED THE PACK.</color> All wolves are converging.");
    }
    public void AlertNearbyZones(Vector3 pos, float dist)
    {
        //loop through zones registered in dictionary
        foreach (GameObject zone in nestZones)
        {
            //calc dist between howl and center of zone
            float distToZone = Vector3.Distance(pos, zone.transform.position);
            //if within earshot
            if(distToZone <= dist)
            {
                //get list of wolves registered with that zone
                if(zoneRegistry.ContainsKey(zone))
                {
                    foreach(monsterAI monster in zoneRegistry[zone])
                    {
                        //wake up wolves if not already hunting
                        if(monster != null && !monster.isZoneActive)
                        {
                            //trigger switchstate
                            monster.SetZoneActivity(true);
                            //play alert sound
                            monster.PlayAlertSound();
                        }
                    }
                }
            }
        }
        Debug.Log($"<color=orange>Pack Alert!</color> Monsters within {dist}m of {pos} have been alerted.");
    }
    
}

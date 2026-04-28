using UnityEngine;

[CreateAssetMenu(fileName = "NewMonsterData", menuName = "Monster Data")]
public class monsterData : ScriptableObject
{
    [Header("Base Stats")]
    public string monsterName;
    public float health;
    public float walkSpeed;
    public float runSpeed;

    [Header("The Investigation")]
    //TODO: this tells the game upon load to determine what assests to load 

    [Header("Detection Settings")]
    public float sightRadius; //how far it sees
    public float aggroRadius; //how close before it attacks

    [Header("Visuals & Sound")]
    public GameObject modelPrefab;
    public AudioClip ambientSound;
    public AudioClip attackSound;
    public AudioClip howlSound;
    public AudioClip[] footstepClips;
    public float footstepVolume = 0.5f;
    public float howlVolume = 0.5f;

    [Header("Combat Stats")]
    public float attackDamage;
    public float attackRange;
    public float attackCooldown;
    

    [Header("Special Ability")]
    public float abilityCooldown; //how often can they use special ability.
    public float abilityRange;

    [Header("Vampire Stats")]
    [Range(0, 1)] public float lifeStealPercent; //percent health restore to vamp on special attacks
    public float blinkCooldown = 3f;

    [Header("Werewolf Pin")]
    public float pinDuration; //how long player is pinned to the ground
    public float breakoutDifficulty; //multiplier for button mash

    [Header("Poise")]
    public float staggerHealth; //damage required to interrupt Alpha attack
    public float armorValue; //flat damage reduction

    [Header("Weakness")]
    //TODO: need a damage type weakness
    public float weaknessMultiplier = 2.0f;
    public float recoveryRate; //how fast does monster regenerate health

    [Header("AI Intelligence")]
    //TODO: need to be able to script in monster behavior, lurk or chase
    public float patienceMeter; //how long stalk before attacking




}

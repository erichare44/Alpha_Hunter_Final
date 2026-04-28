using UnityEngine;

[CreateAssetMenu(fileName = "NewMonsterGroup", menuName = "Monster Group")]
public class monsterGroup : ScriptableObject
{
    public string groupName;
    public GameObject alphaPrefab;
    public GameObject betaPrefab;
    public monsterData stats;

    [Header("Ambient Atmosphere")]
    public AudioClip groupAmbientSound; 
    [Range(0, 1)] public float ambientVolume = 0.5f;
}

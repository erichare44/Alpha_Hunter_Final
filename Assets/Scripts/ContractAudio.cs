using UnityEngine;

[System.Serializable]
public class MonsterAudioEntry
{
    [Header("Monster Match")]
    public string monsterGroup;

    [Header("Monster Audio")]
    public AudioClip audioClip;
}

[RequireComponent(typeof(Collider))]
[RequireComponent (typeof(AudioSource))]
public class ContractAudio : MonoBehaviour
{
    [Header("Location Match")]
    public string poiLocation;

    [Header("Monster Audio List")]
    public MonsterAudioEntry[] monsterAudioEntries;

    [Header("Settings")]
    public bool playOnlyOnce = true;
    private bool hasPlayed = false;
    [Range(0f, 1f)] public float volume = 1f;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = volume;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;
        if (playOnlyOnce && hasPlayed)
            return;
        if (gameManager.instance == null)
            return;
        if (gameManager.instance.selectedMonster == null)
            return;
        if (gameManager.instance.selectedPOI != poiLocation)
            return;

        string currentMonster = gameManager.instance.selectedMonster.groupName;

        foreach (MonsterAudioEntry entry in monsterAudioEntries)
        {
            if (entry.monsterGroup == currentMonster)
            {
                if (entry.audioClip != null)
                {
                    audioSource.clip = entry.audioClip;
                    audioSource.Play();

                    hasPlayed = true;
                }
                return;
            }
        }
    }

}

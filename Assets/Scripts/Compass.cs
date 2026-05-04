using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[System.Serializable]
public class CompassPOI
{
    [Header("World Points")]
    public Transform worldTarget;
    public RectTransform uiMarker;

    [Header("Glow Effect")]
    public bool causesCompassGlow;
    public float glowDist;
}

public class Compass : MonoBehaviour
{
    [Header("Compass Movement")]
    [SerializeField] private RectTransform compassStrip;
    [SerializeField] private float compassWidth;

    [Header("Bar Glow")]
    [SerializeField] private Image compassBarImage;

    [Header("Compass Colors")]
    [SerializeField] private Color baseCompassColor;
    [SerializeField] private Color glowCompassColor;

    [Header("POI Tracking")]
    [SerializeField] private string poiTag = "POI";
    [SerializeField] private float compassHalfWidth;

    private List<CompassPOI> poiTargets = new List<CompassPOI>();
    private Transform player;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetPlayer();
        FindAllPOIs();
    }

    // Update is called once per frame
    void Update()
    {
        GetPlayer();

        if (player == null)
            return;

        updateCompass();
        updatePOIMarkers();
        updateCompassGlow();
    }
    void GetPlayer()
    {
        if (gameManager.instance != null)
        {
            player = gameManager.instance.player;
        }
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GetPlayer();
        FindAllPOIs();
    }

    void updateCompass()
    {
        float rotation = player.eulerAngles.y;

        float xPos = (rotation / 360f) * compassWidth;

        xPos -= compassWidth * 0.5f;

        compassStrip.anchoredPosition = new Vector2(-xPos, 0f);
    }
    void FindAllPOIs()
    {
        poiTargets.Clear();

        GameObject[] pois = GameObject.FindGameObjectsWithTag(poiTag);

        foreach (GameObject obj in pois)
        {
            CompassPOI poi = new CompassPOI();
            poi.worldTarget = obj.transform;

            Transform marker = transform.Find(obj.name + "Marker");

            if (marker != null)
            {
                poi.uiMarker = marker.GetComponent<RectTransform>();
            }

            poi.causesCompassGlow = true;

            poiTargets.Add(poi);
        }
    }

    void updatePOIMarkers()
    {
        foreach (CompassPOI poi in poiTargets)
        {
            if (poi.worldTarget == null || poi.uiMarker == null)
                continue;

            Vector3 direction = poi.worldTarget.position - player.position;
            direction.y = 0f;

            float angle = Vector3.SignedAngle(player.forward, direction, Vector3.up);

            float markerX = (angle / 180f) * compassHalfWidth;

            poi.uiMarker.anchoredPosition = new Vector2(markerX, 0f);
        }
    }

    void updateCompassGlow()
    {
        if (compassBarImage == null)
            return;

        float strongestGlow = 0f;

        foreach (CompassPOI poi in poiTargets)
        {
            if (poi.worldTarget == null || !poi.causesCompassGlow)
                continue;

            float distance = Vector3.Distance(
                player.position,
                poi.worldTarget.position
            );

            float glow = Mathf.Clamp01(1f - (distance / poi.glowDist));

            if (glow > strongestGlow)
                strongestGlow = glow;
        }
        compassBarImage.color = Color.Lerp(baseCompassColor, glowCompassColor, strongestGlow);
    }
}

using System.Data;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CompassPOI
{
    [Header("World Points")]
    public Transform worldTarget;
    public RectTransform uiMarker;

    [Header("Glow Effect")]
    public bool CompassGlow;
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
    [SerializeField] private CompassPOI[] poiTargets;

    private Transform player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
            return;

        updateCompass();
        updatePOI();
        updateGlow();
    }

    void updateCompass()
    {
        float rotation = player.eulerAngles.y;

        float xPos = (rotation / 360f) * compassWidth;

        xPos -= compassWidth * 0.5f;

        compassStrip.anchoredPosition = new Vector2(-xPos, 0f);
    }

    void updatePOI()
    {
        float compassHalfWidth = 400f;

        foreach (CompassPOI poi in poiTargets)
        {
            if (poi.worldTarget == null)
                continue;
            if (poi.uiMarker == null)
                continue;

            Vector3 directionToTarget = poi.worldTarget.position - player.position;
            directionToTarget.y = 0f;
            float angleToTarget = Vector3.SignedAngle(player.forward, directionToTarget, Vector3.up);
            float markerX = (angleToTarget / 180f) * compassHalfWidth;
            poi.uiMarker.anchoredPosition = new Vector2(markerX, 0f);
        }
    }

    void updateGlow()
    {
        if (compassBarImage == null)
            return;

        float strongestGlow = 0f;   

        foreach(CompassPOI poi in poiTargets)
        {
            if (poi.worldTarget == null || !poi.CompassGlow)
                continue;
            float distance = Vector3.Distance(player.position, poi.worldTarget.position);
            float glowStrength = Mathf.Clamp01(1f - (distance / poi.glowDist));
            if (glowStrength > strongestGlow)
            {
                strongestGlow = glowStrength;
            }
        }
        compassBarImage.color = Color.Lerp(baseCompassColor, glowCompassColor, strongestGlow);
    }
}

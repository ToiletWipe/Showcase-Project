using UnityEngine;

public class PlayerDetection : MonoBehaviour
{
    [Header("Light Settings")]
    [Tooltip("Color of the searchlight.")]
    public Color lightColor = Color.white;
    [Tooltip("Intensity of the searchlight.")]
    public float lightIntensity = 2f;
    [Tooltip("Spot angle of the searchlight. Should roughly match the cone angle.")]
    public float lightSpotAngle = 45f;

    [Header("Debug Settings")]
    [Tooltip("Show gizmos for the cone detection.")]
    public bool drawGizmos = true;

    // Components
    private Light spotLight;

    void Awake()
    {
        // Disable any MeshRenderer so the cone does not appear in-game.
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null)
            mr.enabled = false;

        SetupSearchlight();
    }

    void SetupSearchlight()
    {
        // Look for an existing Light in children.
        spotLight = GetComponentInChildren<Light>();
        if (spotLight == null)
        {
            GameObject lightObj = new GameObject("ConeSearchlightLight");
            lightObj.transform.SetParent(transform, false);
            spotLight = lightObj.AddComponent<Light>();
        }

        spotLight.type = LightType.Spot;
        // Ideally, set the spotlight's angle to match or slightly exceed the cone angle.
        spotLight.spotAngle = lightSpotAngle;
        spotLight.color = lightColor;
        spotLight.intensity = lightIntensity;

        // Place the spotlight at the cone's tip (which is at the object's origin)
        // so that it shines outward along the cone's direction.
        spotLight.transform.localPosition = Vector3.zero;
        // Set its forward direction to match the GameObject's forward.
        spotLight.transform.localRotation = Quaternion.identity;
    }
}

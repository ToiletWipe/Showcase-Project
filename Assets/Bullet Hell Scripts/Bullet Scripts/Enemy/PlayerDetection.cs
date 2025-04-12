using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
public class PlayerDetection : MonoBehaviour
{
    [Header("Cone Detection Settings")]
    [Tooltip("The range of the cone detection area (i.e., the distance from the tip to the base plane).")]
    public float height = 10f;
    [Tooltip("The full cone angle (in degrees) of the searchlight.")]
    public float coneAngle = 45f;

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
    private MeshCollider meshCollider;
    private Light spotLight;
    private Mesh coneMesh;

    void Awake()
    {
        // Set up the detection cone.
        meshCollider = GetComponent<MeshCollider>();
        coneMesh = CreateConeMesh();
        meshCollider.sharedMesh = coneMesh;
        meshCollider.convex = true;
        meshCollider.isTrigger = true;

        // Disable any MeshRenderer so the cone does not appear in-game.
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null)
            mr.enabled = false;

        SetupSearchlight();
    }

    // Create a cone mesh with the tip at the origin and the base circle in the plane Z = height.
    Mesh CreateConeMesh()
    {
        int segments = 20; // Increase for a smoother cone.
        Vector3[] vertices = new Vector3[segments + 1];
        int[] triangles = new int[segments * 3];

        // Tip vertex at the origin.
        vertices[0] = Vector3.zero;

        // The cone's half angle, in radians.
        float halfAngleRad = (coneAngle * 0.5f) * Mathf.Deg2Rad;
        // Determine the base circle radius given the cone angle and height.
        float baseRadius = height * Mathf.Tan(halfAngleRad);

        float angleStep = 360f / segments;
        for (int i = 0; i < segments; i++)
        {
            float angleRad = Mathf.Deg2Rad * (i * angleStep);
            // Base circle vertices lie in the plane Z = height.
            float x = baseRadius * Mathf.Cos(angleRad);
            float y = baseRadius * Mathf.Sin(angleRad);
            vertices[i + 1] = new Vector3(x, y, height);
        }

        // Build triangles: each triangle connects the tip (vertex 0) and two adjacent base vertices.
        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3 + 0] = 0;
            triangles[i * 3 + 1] = (i + 1) % segments + 1;
            triangles[i * 3 + 2] = i + 1;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
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
        spotLight.range = height;
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

    // Draw the cone detection area as a wireframe in the Scene view.
    void OnDrawGizmos()
    {
        if (!drawGizmos)
            return;

        Gizmos.color = Color.yellow;
        if (coneMesh != null)
            Gizmos.DrawWireMesh(coneMesh, transform.position, transform.rotation, transform.lossyScale);
    }
}

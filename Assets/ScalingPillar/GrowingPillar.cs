using UnityEngine;

public class GrowingPillar : MonoBehaviour
{
    [Header("Pillar Settings")]
    public float scaleSpeed = 1.0f; // Speed at which the pillar scales up
    public float maxScale = 5.0f; // Maximum scale of the pillar
    private Vector3 initialScale; // Initial scale of the pillar
    private bool isScaling = false; // Flag to check if the pillar is scaling

    [Header("Wall Destruction")]
    public float destructionForce = 10.0f; // Force applied to walls when destroyed
    public LayerMask wallLayer; // LayerMask to filter which objects are considered walls

    [Header("Turret Settings")]
    public Transform turret; // Reference to the turret Transform
    public Transform turretAnchor; // Reference to the anchor point Transform

    void Start()
    {
        // Store the initial scale of the pillar
        initialScale = transform.localScale;

        // Start with the pillar scaled down (hidden)
        transform.localScale = new Vector3(initialScale.x, 0, initialScale.z); // Only scale Y to zero
    }

    void Update()
    {
        if (isScaling)
        {
            // Gradually increase the pillar's scale on the Y axis
            if (transform.localScale.y < maxScale)
            {
                transform.localScale += Vector3.up * scaleSpeed * Time.deltaTime;

                // Update the turret's position to follow the anchor point (if assigned)
                if (turret != null && turretAnchor != null)
                {
                    turret.position = turretAnchor.position;
                }
            }
        }
    }

    // Public method to start scaling the pillar
    public void StartScaling()
    {
        isScaling = true;
    }

    // Apply force to walls when the pillar collides with them
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collided object is on the wall layer
        if (((1 << collision.gameObject.layer) & wallLayer) != 0)
        {
            Rigidbody wallRb = collision.gameObject.GetComponent<Rigidbody>();
            if (wallRb != null)
            {
                // Calculate the direction of the force (away from the pillar)
                Vector3 forceDirection = (collision.transform.position - transform.position).normalized;

                // Apply force to the wall
                wallRb.isKinematic = false; // Ensure the wall is not kinematic
                wallRb.AddForce(forceDirection * destructionForce, ForceMode.Impulse);
            }
        }
    }
}
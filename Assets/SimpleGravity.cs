using UnityEngine;

public class SimpleGravity : MonoBehaviour
{
    [Tooltip("Gravitational acceleration (typically a negative value).")]
    public float gravity = -9.81f;

    [Tooltip("Maximum downward speed (terminal velocity).")]
    public float terminalVelocity = -50f;

    [Tooltip("Distance from the object's position to check for ground.")]
    public float groundCheckDistance = 0.2f;

    [Tooltip("LayerMask to determine which layers are considered ground.")]
    public LayerMask groundMask;

    // Current vertical speed.
    private float verticalVelocity = 0f;

    void Update()
    {
        // Cast a ray downward from the object's position
        RaycastHit hit;
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance, groundMask);

        if (isGrounded)
        {
            // Optionally, if the enemy is falling, reset vertical velocity when grounded.
            if (verticalVelocity < 0)
            {
                verticalVelocity = 0f;

                // Snap the enemy's position to just above the ground.
                // Adjust the offset (e.g. 0.1f) based on your enemy's collider size.
                transform.position = new Vector3(transform.position.x, hit.point.y + 0.1f, transform.position.z);
            }
        }
        else
        {
            // Apply gravity over time.
            verticalVelocity += gravity * Time.deltaTime;
            verticalVelocity = Mathf.Max(verticalVelocity, terminalVelocity);
            transform.position += new Vector3(0, verticalVelocity * Time.deltaTime, 0);
        }
    }
}
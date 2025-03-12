using UnityEngine;

public class ThrownObjectDamage : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the object has a Health component
        Health health = collision.gameObject.GetComponent<Health>();
        if (health != null)
        {
            // Apply damage to the object
            health.TakeDamage(20f); // Adjust the damage value as needed
        }

        // Call the method to destroy the object after 1 second
        Invoke("DestroyObject", 0.5f); // 1 second delay before destroying the object
    }

    private void DestroyObject()
    {
        // Destroy the object after the delay
        Destroy(gameObject);
    }
}

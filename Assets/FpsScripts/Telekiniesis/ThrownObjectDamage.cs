using UnityEngine;
using System.Collections;

public class ThrownObjectDamage : MonoBehaviour
{
    private bool isThrown = false; // Flag to track if the object has been thrown

    public void SetThrown(bool thrown)
    {
        isThrown = thrown; // Set the thrown state
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Only proceed if the object has been thrown
        if (!isThrown)
        {
            return;
        }

        // Check if the object has a Health component
        Health health = collision.gameObject.GetComponent<Health>();
        if (health != null)
        {
            // Apply damage to the object
            health.TakeDamage(50f); // Adjust the damage value as needed
        }

        // Call the method to destroy the object after 1 second
        Invoke("DestroyObject", 0.4f); // 1 second delay before destroying the object
    }

    private void DestroyObject()
    {
        // Destroy the object after the delay
        Destroy(gameObject);
    }
}
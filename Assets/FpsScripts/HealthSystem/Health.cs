using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100; // Maximum health of the object
    private float currentHealth;  // Current health of the object
    private Renderer objectRenderer; // Renderer component of the object
    private Color originalColor; // Original color of the object

    // Public property to access currentHealth
    public float CurrentHealth => currentHealth;

    private void Start()
    {
        currentHealth = maxHealth; // Initialize health

        // Get the Renderer component and store the original color
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalColor = objectRenderer.material.color;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage; // Reduce health by the damage amount

        Debug.Log(gameObject.name + " took " + damage + " damage. Current health: " + currentHealth);

        // Flash red when taking damage
        if (objectRenderer != null)
        {
            objectRenderer.material.color = Color.red;
            Invoke("ResetColor", 0.1f); // Reset the color after 0.1 seconds
        }

        // Check if the object is dead
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void ResetColor()
    {
        // Reset the object's color to its original color
        if (objectRenderer != null)
        {
            objectRenderer.material.color = originalColor;
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " has died.");
        Destroy(gameObject); // Destroy the object
    }
}
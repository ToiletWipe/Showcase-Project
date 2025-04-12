using UnityEngine;

namespace Project.Scripts.Enemy
{
    public class HellBullet : MonoBehaviour
    {
        [SerializeField] private float damage = 10f; // Damage dealt by the turret

        public void Initialize(float damage)
        {
            this.damage = damage;
        }

        // Use OnTriggerEnter so that the bullet will pass through objects tagged as "Enemy"
        // while still interacting with other objects.
        private void OnTriggerEnter(Collider other)
        {
            // If the colliding object is tagged "Enemy", do nothing (pass through)
            if (other.CompareTag("Enemy"))
            {
                return;
            }

            Debug.Log("HellBullet triggered with: " + other.name);

            // Apply damage if the hit object has a Health component.
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
                Debug.Log("Hit " + other.name + " for " + damage + " damage!");
            }

            // Apply force to objects with a Rigidbody (useful for breakable objects).
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 forceDirection = transform.forward * 150f; // Adjust force as needed.
                rb.AddForce(forceDirection, ForceMode.Impulse);
                Debug.Log("Applied force to: " + other.name);
            }

            // Destroy the bullet after impact.
            Destroy(gameObject);
        }
    }
}

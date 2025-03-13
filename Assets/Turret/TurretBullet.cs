using UnityEngine;

namespace Project.Scripts.Enemy
{
    public class TurretBullet : MonoBehaviour
    {
        private float damage;

        public void Initialize(float damage)
        {
            this.damage = damage;
        }

        //  Change from OnTriggerEnter to OnCollisionEnter
        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log("Bullet collided with: " + collision.collider.name);

            // Apply damage if the hit object has a Health component
            var health = collision.collider.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
                Debug.Log("Hit " + collision.collider.name + " for " + damage + " damage!");
            }

            // Apply force to objects with Rigidbody (breakable walls)
            Rigidbody rb = collision.collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 forceDirection = transform.forward * 50f; // Adjust force as needed
                rb.AddForce(forceDirection, ForceMode.Impulse);
                Debug.Log("Applied force to: " + collision.collider.name);
            }

            // Destroy bullet on impact
            Destroy(gameObject);
        }
    }
}

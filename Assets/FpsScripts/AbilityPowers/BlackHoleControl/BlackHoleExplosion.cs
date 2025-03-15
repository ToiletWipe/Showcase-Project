using UnityEngine;

public class BlackHoleExplosion : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius; // Radius of the explosion
    [SerializeField] private float explosionForce; // Force of the explosion
    [SerializeField] private float explosionDelay; // Delay before explosion
    [SerializeField] private GameObject explosionParticles; // Particle effect for the explosion
    [SerializeField] private float explosionDamage; // Damage dealt by the explosion

    private bool hasExploded = false;

    public void Initialize(float radius, float force, float delay, GameObject particles, float damage)
    {
        explosionRadius = radius;
        explosionForce = force;
        explosionDelay = delay;
        explosionParticles = particles;
        explosionDamage = damage; // Set the explosion damage
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasExploded)
        {
            hasExploded = true;
            Invoke("Explode", explosionDelay);
        }
    }

    private void Explode()
    {
        // Find all colliders within the explosion radius
        Collider[] surroundingObjects = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (var obj in surroundingObjects)
        {
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Apply explosion force
                Vector3 direction = (obj.transform.position - transform.position).normalized;
                float distance = Vector3.Distance(obj.transform.position, transform.position);
                float forceScale = 1 - (distance / explosionRadius); // Force scales with distance
                rb.AddForce(direction * explosionForce * forceScale, ForceMode.Impulse);
            }

            // Apply damage to objects with a Health component
            Health health = obj.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(explosionDamage); // Apply damage
            }
        }

        // Instantiate the particle effect
        if (explosionParticles != null)
        {
            GameObject particlesInstance = Instantiate(explosionParticles, transform.position, Quaternion.identity);
            Destroy(particlesInstance, 2f); // Destroy particles after 2 seconds
        }

        // Destroy the black hole
        Destroy(gameObject);
    }
}
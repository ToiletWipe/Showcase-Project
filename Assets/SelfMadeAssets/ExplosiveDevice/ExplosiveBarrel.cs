using UnityEngine;

public class ExplosiveBarrel : MonoBehaviour
{
    [SerializeField] private float _triggerForce = 0.5f; // Minimum collision force to trigger the explosion
    [SerializeField] private float _explosionRadius = 5; // Radius of the explosion
    [SerializeField] private float _explosionForce = 1000; // Force of the explosion
    [SerializeField] private int _damage = 50; // Damage dealt by the explosion
    [SerializeField] private float _explosionDelay = 0.3f; // Delay before explosion (0.3 seconds)
    [SerializeField] private GameObject _particles = null; // Particle effect for the explosion
    [SerializeField] private LayerMask _layerMask = ~0; // Layer mask to filter which objects are affected (default: all layers)

    private bool _hasExploded = false;

    private void OnCollisionEnter(Collision collision)
    {
        // Trigger the explosion if the collision force is strong enough
        if (collision.relativeVelocity.magnitude >= _triggerForce && !_hasExploded)
        {
            _hasExploded = true; // Prevent multiple explosions
            Invoke("Explode", _explosionDelay); // Trigger explosion after delay
        }
    }

    private void Explode()
    {
        // Find all colliders within the explosion radius
        Collider[] surroundingObjects = Physics.OverlapSphere(transform.position, _explosionRadius, _layerMask);

        foreach (var obj in surroundingObjects)
        {
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Apply explosion force
                Vector3 direction = (obj.transform.position - transform.position).normalized;
                float distance = Vector3.Distance(obj.transform.position, transform.position);
                float forceScale = 1 - (distance / _explosionRadius); // Force scales with distance
                rb.AddForce(direction * _explosionForce * forceScale, ForceMode.Impulse);
            }

            // Apply damage (if the object has a Health component)
            Health health = obj.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(_damage);
            }
        }

        // Instantiate the particle effect
        if (_particles != null)
        {
            GameObject particlesInstance = Instantiate(_particles, transform.position, Quaternion.identity);
            Destroy(particlesInstance, 2f); // Destroy particles after 2 seconds
        }

        // Destroy the explosive object
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Draw the explosion radius in the editor for debugging
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _explosionRadius);
    }
}
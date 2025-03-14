using UnityEngine;

public class Explosive : MonoBehaviour
{
    [SerializeField] private float _triggerForce = 0.5f; // Minimum collision force to trigger the explosion
    [SerializeField] private float _explosionRadius = 5; // Radius of the explosion
    [SerializeField] private float _explosionForce = 500; // Force of the explosion
    [SerializeField] private GameObject _particles = null; // Particle effect for the explosion
    #pragma warning disable CS0649
    [SerializeField] private LayerMask _layerMask; // Layer mask to filter which objects are affected
    #pragma warning restore CS0649

    private void OnCollisionEnter(Collision collision)
    {
        // Trigger the explosion if the collision force is strong enough
        if (collision.relativeVelocity.magnitude >= _triggerForce)
        {
            Explode();
        }
    }

    private void Explode()
    {
        // Find all colliders within the explosion radius
        Collider[] surroundingObjects = Physics.OverlapSphere(transform.position, _explosionRadius, _layerMask);

        foreach (var obj in surroundingObjects)
        {
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb == null) continue; // Skip objects without a Rigidbody

            // Calculate the direction from the explosion center to the object
            Vector3 direction = obj.transform.position - transform.position;
            float distance = direction.magnitude;

            // Normalize the direction and calculate the force scale (force decreases with distance)
            direction.Normalize();
            float forceScale = 1 - (distance / _explosionRadius); // Force scales from 1 (at center) to 0 (at edge)

            // Apply the force to the object
            rb.AddForce(direction * _explosionForce * forceScale, ForceMode.Impulse);
        }

        // Instantiate the particle effect
        if (_particles != null)
        {
            GameObject particlesInstance = Instantiate(_particles, transform.position, Quaternion.identity);

            // Destroy the particle effect after it finishes playing
            ParticleSystem ps = particlesInstance.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                Destroy(particlesInstance, ps.main.duration + ps.main.startLifetime.constantMax);
            }
            else
            {
                Destroy(particlesInstance, 2f); // Fallback if no ParticleSystem is found
            }
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
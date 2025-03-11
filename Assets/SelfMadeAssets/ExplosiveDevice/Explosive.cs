using UnityEngine;

public class Explosive : MonoBehaviour
{
    [SerializeField] private float _triggerForce = 0.5f;
    [SerializeField] private float _explosionRadius = 5;
    [SerializeField] private float _explosionForce = 500;
    [SerializeField] private GameObject _particles;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude >= _triggerForce)
        {
            var surroundingObjects = Physics.OverlapSphere(transform.position, _explosionRadius);

            foreach (var obj in surroundingObjects)
            {
                var rb = obj.GetComponent<Rigidbody>();
                if (rb == null) continue;

                rb.AddExplosionForce(_explosionForce, transform.position, _explosionRadius, 1);
            }

            // Instantiate the particle effect
            GameObject particlesInstance = Instantiate(_particles, transform.position, Quaternion.identity);

            // Get the ParticleSystem component and destroy it after it finishes playing
            ParticleSystem ps = particlesInstance.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                Destroy(particlesInstance, ps.main.duration + ps.main.startLifetime.constantMax);
            }
            else
            {
                Destroy(particlesInstance, 2f); // Fallback if no ParticleSystem is found
            }

            Destroy(gameObject);
        }
    }
}

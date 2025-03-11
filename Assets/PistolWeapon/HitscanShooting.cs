using System.Collections;
using UnityEngine;

public class HitscanShooting : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem ShootingSystem; // Muzzle flash particle system
    [SerializeField]
    private Transform BulletSpawnPoint;
    [SerializeField]
    private ParticleSystem ImpactParticleSystem;
    [SerializeField]
    private TrailRenderer BulletTrail;
    [SerializeField]
    private float ShootDelay = 0.1f;
    [SerializeField]
    private float Speed = 100;
    [SerializeField]
    private LayerMask Mask;
    [SerializeField]
    private bool BouncingBullets;
    [SerializeField]
    private float BounceDistance = 10f;
    [SerializeField]
    private float BulletForce = 10f; // Force applied to objects when hit

    private float LastShootTime;

    private void Update()
    {
        // Check for left mouse button click
        if (Input.GetMouseButtonDown(0)) // 0 = left mouse button
        {
            Debug.Log("Left mouse button clicked!"); // Debug: Confirm left-click is detected
            Shoot();
        }
    }

    public void Shoot()
    {
        if (LastShootTime + ShootDelay < Time.time)
        {
            Debug.Log("Shoot method called!"); // Debug: Confirm Shoot method is executed

            // Play the muzzle flash particle system
            if (ShootingSystem != null)
            {
                Debug.Log("Playing muzzle flash!"); // Debug: Confirm muzzle flash is being played
                ShootingSystem.Stop(); // Ensure the particle system is reset
                ShootingSystem.Play();
            }
            else
            {
                Debug.LogError("ShootingSystem is not assigned!"); // Debug: Warn if ShootingSystem is missing
            }

            Vector3 direction = transform.forward;
            TrailRenderer trail = Instantiate(BulletTrail, BulletSpawnPoint.position, Quaternion.identity);

            if (Physics.Raycast(BulletSpawnPoint.position, direction, out RaycastHit hit, float.MaxValue, Mask))
            {
                // Apply force to the object if it has a Rigidbody
                ApplyForceToObject(hit.collider, direction);

                StartCoroutine(SpawnTrail(trail, hit.point, hit.normal, BounceDistance, true));
            }
            else
            {
                StartCoroutine(SpawnTrail(trail, BulletSpawnPoint.position + direction * 100, Vector3.zero, BounceDistance, false));
            }

            LastShootTime = Time.time;
        }
    }

    private void ApplyForceToObject(Collider collider, Vector3 direction)
    {
        // Check if the object has a Rigidbody
        Rigidbody rb = collider.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Apply force to the Rigidbody
            rb.AddForce(direction * BulletForce, ForceMode.Impulse);
            Debug.Log("Force applied to object: " + collider.name);
        }
    }

    private IEnumerator SpawnTrail(TrailRenderer Trail, Vector3 HitPoint, Vector3 HitNormal, float BounceDistance, bool MadeImpact)
    {
        Vector3 startPosition = Trail.transform.position;
        Vector3 direction = (HitPoint - Trail.transform.position).normalized;

        float distance = Vector3.Distance(Trail.transform.position, HitPoint);
        float startingDistance = distance;

        while (distance > 0)
        {
            Trail.transform.position = Vector3.Lerp(startPosition, HitPoint, 1 - (distance / startingDistance));
            distance -= Time.deltaTime * Speed;

            yield return null;
        }

        Trail.transform.position = HitPoint;

        if (MadeImpact)
        {
            Instantiate(ImpactParticleSystem, HitPoint, Quaternion.LookRotation(HitNormal));

            if (BouncingBullets && BounceDistance > 0)
            {
                Vector3 bounceDirection = Vector3.Reflect(direction, HitNormal);

                if (Physics.Raycast(HitPoint, bounceDirection, out RaycastHit hit, BounceDistance, Mask))
                {
                    // Apply force to the bounced object if it has a Rigidbody
                    ApplyForceToObject(hit.collider, bounceDirection);

                    yield return StartCoroutine(SpawnTrail(
                        Trail,
                        hit.point,
                        hit.normal,
                        BounceDistance - Vector3.Distance(hit.point, HitPoint),
                        true
                    ));
                }
                else
                {
                    yield return StartCoroutine(SpawnTrail(
                        Trail,
                        HitPoint + bounceDirection * BounceDistance,
                        Vector3.zero,
                        0,
                        false
                    ));
                }
            }
        }

        Destroy(Trail.gameObject, Trail.time);
    }
}
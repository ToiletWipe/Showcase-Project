using UnityEngine;

namespace Project.Scripts.Enemy
{
    public class Turret : MonoBehaviour
    {
        [SerializeField] private Transform barrelEnd = null;
        [SerializeField] private float radius = 0.1f; // Reduced size of the projectile
        [SerializeField] private float velocity = 1000f;
        [SerializeField] private float mass = .5f;
        [SerializeField] private float fireRate = 1f; // Time between shots
        [SerializeField] private float detectionRange = 10f; // Range within which the turret detects the player
        [SerializeField] private float damage = 10f; // Damage dealt by the turret

        [SerializeField] private ParticleSystem muzzleFlashPrefab = null; // Muzzle flash effect
        [SerializeField] private TrailRenderer bulletTrailPrefab = null; // Bullet trail effect
        [SerializeField] private Material bulletMaterial = null; // Custom URP material for the bullet

        private Transform player;
        private float nextFireTime;

        void Start()
        {
            // Find the player by tag (make sure your player GameObject is tagged as "Player")
            player = GameObject.FindGameObjectWithTag("Player").transform;

            if (player == null)
            {
                Debug.LogError("Player not found! Make sure the player is tagged as 'Player'.");
            }
        }

        void Update()
        {
            if (player == null) return;

            // Check if the player is within detection range
            if (Vector3.Distance(transform.position, player.position) <= detectionRange)
            {
                // Rotate the turret to face the player
                RotateTowardsPlayer();

                // Shoot at the player if enough time has passed since the last shot
                if (Time.time >= nextFireTime)
                {
                    FireBullet();
                    nextFireTime = Time.time + 1f / fireRate;
                }
            }
        }

        private void RotateTowardsPlayer()
        {
            // Calculate the direction to the player
            Vector3 direction = (player.position - transform.position).normalized;

            // Calculate the rotation to look at the player
            Quaternion lookRotation = Quaternion.LookRotation(direction);

            // Smoothly rotate towards the player
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

        private void FireBullet()
        {
            // Muzzle Flash
            if (muzzleFlashPrefab != null)
            {
                var muzzleFlash = Instantiate(muzzleFlashPrefab, barrelEnd.position, barrelEnd.rotation);
                muzzleFlash.transform.localScale = Vector3.one * 0.3f;
                muzzleFlash.Play();
                Destroy(muzzleFlash.gameObject, muzzleFlash.main.duration);
            }

            // Create Bullet
            var bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bullet.transform.position = barrelEnd.position;
            bullet.transform.localScale = Vector3.one * radius;

            // Apply URP material
            var renderer = bullet.GetComponent<Renderer>();
            if (renderer != null && bulletMaterial != null)
            {
                renderer.material = bulletMaterial;
            }

            // Add Rigidbody
            var rb = bullet.AddComponent<Rigidbody>();
            rb.linearVelocity = barrelEnd.forward * velocity;
            rb.mass = mass;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.isKinematic = false;

            // Add Collider
            var collider = bullet.GetComponent<SphereCollider>();
            collider.isTrigger = false; //  Make sure it's NOT a trigger!

            // Ensure it collides with EVERYTHING
            bullet.layer = 0; // Default layer (will collide with all layers)

            // Add bullet trail
            if (bulletTrailPrefab != null)
            {
                var trail = Instantiate(bulletTrailPrefab, barrelEnd.position, Quaternion.identity);
                trail.transform.SetParent(bullet.transform);
            }

            // Attach bullet logic
            var turretBullet = bullet.AddComponent<TurretBullet>();
            turretBullet.Initialize(damage);

            // Destroy bullet after 1 second
            Destroy(bullet, 1f);
        }


    }
}
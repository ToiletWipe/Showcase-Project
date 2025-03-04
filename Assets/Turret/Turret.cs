using UnityEngine;

namespace Project.Scripts.Enemy
{
    public class Turret : MonoBehaviour
    {
        [SerializeField] private Transform barrelEnd;
        [SerializeField] private float radius = 0.1f;
        [SerializeField] private float velocity = 1000f;
        [SerializeField] private float mass = .5f;
        [SerializeField] private float fireRate = 1f; // Time between shots
        [SerializeField] private float detectionRange = 10f; // Range within which the turret detects the player

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
            // Create a bullet
            var bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bullet.transform.position = barrelEnd.position;
            bullet.transform.localScale = Vector3.one * radius;

            // Set bullet color and emission
            var mat = bullet.GetComponent<Renderer>().material;
            mat.color = Color.red;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.white);

            // Add Rigidbody and set velocity
            var rb = bullet.AddComponent<Rigidbody>();
            rb.linearVelocity = barrelEnd.forward * velocity;
            rb.mass = mass;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            // Destroy the bullet after 5 seconds
            Destroy(bullet, 5);
        }
    }
}
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public enum BossState
{
    Idle,
    Patrol,
    PlayerDetected,
    AttackPattern1,
    AttackPattern2,
    AttackPattern3,
    AttackPattern4,
    PlayerLost,
    Retreat
}

public class BossController : MonoBehaviour
{
    [Header("Boss States")]
    public BossState currentState;

    [Header("Player & Detection")]
    public Transform player;  // The player's position
    [SerializeField] private float detectionRange = 10f; // Range within which the turret detects the player
    public float attackDistance = 10f;  // Distance at which the boss will start attacking
    public float detectionDelay = 2f;   // Delay after player is detected before attacking
    public float playerLostDelay = 2f;  // Delay after player is lost before returning to idle/patrol

    [Header("Attack Pattern Timing")]
    public float attackPatternDuration = 5f;  // Duration of each attack pattern

    [Header("Movement")]
    public bool canPatrol = false;      // Whether the boss can patrol when in idle state
    [Tooltip("Speed at which the boss rotates to face the player.")]
    public float rotationSpeed = 5f;
    [Tooltip("If enabled, the boss will move toward the player until within attackDistance.")]
    public bool moveTowardsPlayer = false;
    [Tooltip("Movement speed when moving toward the player.")]
    public float movementSpeed = 3f;

    private float stateTimer;           // Timer to track delays for detection and lost state
    private BulletSpawnerTuesday bulletSpawner;  // Reference to the bullet spawner
    private bool isPlayerInRange = false;  // Tracks if the player is inside the detection zone
    private bool lastPatternWas1 = false;  // Tracks if the last pattern was Pattern 1

    [Header("Summoning")]
    public GameObject summonablePrefab3; // Summonable entity for Attack Pattern 3
    public GameObject summonablePrefab4; // Summonable entity for Attack Pattern 4
    public Transform summonPoint;
    private bool hasSummonedEntityPattern3 = false;  // Flag for pattern 3 summon
    private bool hasSummonedEntityPattern4 = false;  // Flag for pattern 4 summon

    private float lostTimer;
    private int playerColliderCount = 0;

    private Rigidbody rb;

    void Start()
    {
        currentState = BossState.Idle;
        bulletSpawner = GetComponent<BulletSpawnerTuesday>();  // Assuming the BulletSpawner is on the same GameObject
                                                               // Find the player by tag (make sure your player GameObject is tagged as "Player")
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (player == null)
        {
            Debug.LogError("Player not found! Make sure the player is tagged as 'Player'.");
        }

        rb = GetComponent<Rigidbody>();
        if (rb == null)
            Debug.LogError("No Rigidbody found on BossController GameObject!");

    }

    void Update()
    {
        if (player == null) return;

        // ----- Detection Based on Distance -----
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= detectionRange)
        {
            if (!isPlayerInRange)
            {
                isPlayerInRange = true;
                Debug.Log("Player entered detection zone.");
            }

            // Rotate the enemy to face the player.
            RotateTowardsPlayer();

            // If moveTowardsPlayer is enabled and the player is beyond attackDistance,
            // move toward the player (only on the horizontal plane).
            if (moveTowardsPlayer && distance > attackDistance)
            {
                Vector3 targetPosition = new Vector3(player.position.x, transform.position.y, player.position.z);
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);
                Debug.Log("Moving towards player.");
            }
        }
        else
        {
            if (isPlayerInRange)
            {
                isPlayerInRange = false;
                Debug.Log("Player exited detection zone.");
            }
        }

        // ---- Attack Decision Based on Attack Distance ----
        // Only if the player is within the attackDistance do we allow the state machine to transition to an attack state.
        if (isPlayerInRange && distance > attackDistance)
        {
            // Optionally, if the player is detected but not close enough, you might choose to remain in a "detected" state.
            // For this example, we simply do nothing extra here.
        }

        // If the player is not in range, do nothing (thus preserving the current rotation).

        // === Update Lost Timer ===
        // Update the lost timer every frame based on whether the player is in range.
        if (isPlayerInRange)
        {
            lostTimer = playerLostDelay;  // Reset lost timer because the player is still here
        }
        else
        {
            lostTimer -= Time.deltaTime;
            if (lostTimer <= 0f)
            {
                ChangeState(BossState.PlayerLost);
            }
        }

        // === State Machine ===
        switch (currentState)
        {
            case BossState.Idle:
                HandleIdleState();
                break;

            case BossState.Patrol:
                HandlePatrolState();
                break;

            case BossState.PlayerDetected:
                HandlePlayerDetectedState();
                break;

            case BossState.AttackPattern1:
                HandleAttackPattern1State();
                break;

            case BossState.AttackPattern2:
                HandleAttackPattern2State();
                break;

            case BossState.AttackPattern3:
                HandleAttackPattern3State();
                break;

            case BossState.AttackPattern4:
                HandleAttackPattern4State();
                break;

            case BossState.PlayerLost:
                HandlePlayerLostState();
                break;

            case BossState.Retreat:
                HandleRetreatState();
                break;
        }
    }

    // ----- FixedUpdate: Override Horizontal Velocity -----
    // This is where we "lock" the enemy's horizontal velocity to our desired movement
    // so that external forces (like bullet impulses) are ignored.
    void FixedUpdate()
    {
        if (player == null) return;

        // If moveTowardsPlayer is enabled:
        if (moveTowardsPlayer && isPlayerInRange)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            // Only move toward the player if we're farther than attackDistance.
            if (distance > attackDistance)
            {
                // Compute the target horizontal position.
                Vector3 targetPosition = new Vector3(player.position.x, transform.position.y, player.position.z);
                // Calculate direction from the enemy to the target.
                Vector3 direction = (targetPosition - transform.position).normalized;
                // Compute desired horizontal velocity.
                Vector3 desiredHorizontalVelocity = direction * movementSpeed;
                // Override the rigidbody's horizontal velocity while preserving vertical velocity.
                rb.linearVelocity = new Vector3(desiredHorizontalVelocity.x, rb.linearVelocity.y, desiredHorizontalVelocity.z);
            }
            else
            {
                // If within attackDistance, zero out horizontal velocity.
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }
        }
        else
        {
            // If not moving toward the player, you may opt to zero horizontal velocity to ignore external forces.
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    void RotateTowardsPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;  // Only rotate horizontally.
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void HandleIdleState()
    {
        // Idle logic, could include animation, waiting, etc.
        // If the player is detected, change to "PlayerDetected" state
        if (isPlayerInRange)
        {
            stateTimer = detectionDelay;  // Start the detection delay timer
            ChangeState(BossState.PlayerDetected);
        }
    }

    void HandlePatrolState()
    {
        // Add patrol movement logic here if desired
        if (isPlayerInRange)
        {
            stateTimer = detectionDelay;
            ChangeState(BossState.PlayerDetected);
        }
    }

    void HandlePlayerDetectedState()
    {
        // Only choose an attack state if the player is within the closer attack distance.
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > attackDistance)
        {
            // Remain in detected state until the player comes close enough.
            stateTimer = detectionDelay;
            return;
        }

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            int chosenPattern = Random.Range(1, 3);  // Random number between 1 or 2
            // Start with either AttackPattern1 or AttackPattern2
            if (chosenPattern == 1)
            {
                lastPatternWas1 = true;
                ChangeState(BossState.AttackPattern1);
            }
            else
            {
                lastPatternWas1 = false;
                ChangeState(BossState.AttackPattern2);
            }

            stateTimer = attackPatternDuration;
        }
    }

    void HandleAttackPattern1State()
    {
        stateTimer -= Time.deltaTime;

        if (bulletSpawner != null && !bulletSpawner.isFiringPattern1)
        {
            bulletSpawner.StartFiringPattern1();  // Start firing pattern 1
        }

        // After the pattern duration, switch to attack pattern 2
        if (stateTimer <= 0f)
        {
            bulletSpawner.StopFiringPattern1();  // Stop Pattern 1
            stateTimer = attackPatternDuration;  // Reset the timer for the second pattern
            ChangeState(BossState.AttackPattern2);  // Switch to the second attack pattern
        }
    }

    void HandleAttackPattern2State()
    {
        stateTimer -= Time.deltaTime;

        if (bulletSpawner != null && !bulletSpawner.isFiringPattern2)
        {
            bulletSpawner.StartFiringPattern2();  // Start firing pattern 2
        }

        // After the pattern duration, switch back to attack pattern 1
        if (stateTimer <= 0f)
        {
            bulletSpawner.StopFiringPattern2();  // Stop Pattern 2
            stateTimer = attackPatternDuration;
            ChangeState(BossState.AttackPattern1);  // Loop back to pattern 1
        }
    }

    // Handle the state for Attack Pattern 3 (summoning one type of entity)
    void HandleAttackPattern3State()
    {
        stateTimer -= Time.deltaTime;

        if (bulletSpawner != null && !bulletSpawner.isFiringPattern3)
        {
            bulletSpawner.StartFiringPattern3();  // Start firing pattern 3

            // Summon the entity only once
            if (!hasSummonedEntityPattern3)
            {
                SummonEntity();
                hasSummonedEntityPattern3 = true;  // Set the flag so it doesn't summon again
            }
        }

        // After the pattern duration, switch back to attack pattern 1
        if (stateTimer <= 0f)
        {
            bulletSpawner.StopFiringPattern3();  // Stop Pattern 3
            stateTimer = attackPatternDuration;
            ChangeState(BossState.AttackPattern1);  // Loop back to pattern 1
            hasSummonedEntityPattern3 = false;  // Reset the flag when switching patterns
        }
    }

    // Summon entity at a specific point
    void SummonEntity()
    {
        if (summonablePrefab3 != null && summonPoint != null)
        {
            Instantiate(summonablePrefab3, summonPoint.position, Quaternion.identity);
        }
    }

    public void HandleAttackPattern4State()
    {
        stateTimer -= Time.deltaTime;

        if (bulletSpawner != null && !bulletSpawner.isFiringPattern3)
        {
            bulletSpawner.StartFiringPattern3();  // Start firing pattern 3

            // Summon entities only once
            if (!hasSummonedEntityPattern4)
            {
                List<float> initialAngles = new List<float> { 0f, 60f, 120f, 180f, 240f, 300f };  // Example angles for a circle
                SummonEntitiesInCircle(6, initialAngles);  // Spawn 6 summonable entities in a circle
                hasSummonedEntityPattern4 = true;  // Set the flag so it doesn't summon again
            }
        }

        // After the pattern duration, switch back to attack pattern 1
        if (stateTimer <= 0f)
        {
            bulletSpawner.StopFiringPattern3();  // Stop Pattern 3
            stateTimer = attackPatternDuration;
            ChangeState(BossState.AttackPattern1);  // Loop back to pattern 1
            hasSummonedEntityPattern4 = false;  // Reset the flag when switching patterns
        }
    }

    // Summon entities in a circular formation around the boss or a specific point, with custom initial angles
    void SummonEntitiesInCircle(int numberOfEntities, List<float> initialAngles)
    {
        float radius = 1f;  // Distance from the center (summon point or boss)

        // Ensure we have the correct number of angles provided, or default to evenly spaced angles
        if (initialAngles == null || initialAngles.Count != numberOfEntities)
        {
            // Generate evenly spaced angles if custom angles are not provided or the count doesn't match
            initialAngles = new List<float>();
            float angleStep = 360f / numberOfEntities;  // Angle between each entity
            for (int i = 0; i < numberOfEntities; i++)
            {
                initialAngles.Add(i * angleStep);
            }
        }

        for (int i = 0; i < numberOfEntities; i++)
        {
            // Convert the custom initial angle to radians
            float angle = initialAngles[i] * Mathf.Deg2Rad;

            // Calculate the spawn position based on the angle and radius
            Vector3 spawnPosition = summonPoint.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;

            // Instantiate the summonable entity
            GameObject summonedEntity = Instantiate(summonablePrefab4, spawnPosition, Quaternion.identity);

            // Get the SummonableMovement script on the summoned entity
            SummonableMovement summonableMovement = summonedEntity.GetComponent<SummonableMovement>();
            if (summonableMovement != null)
            {
                // Set movement type to Orbit and configure its orbit settings
                summonableMovement.movementType = MovementType.Orbit;
                summonableMovement.orbitSpeed = -18f;  // Set the orbit speed
                summonableMovement.offsetDistance = radius;  // Distance from the target (same as radius)
                summonableMovement.initialAngle = initialAngles[i];  // Set the initial angle
            }
        }
    }

    void HandlePlayerLostState()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            bulletSpawner.StopAllFiring();  // Stop all bullet patterns
            ChangeState(canPatrol ? BossState.Patrol : BossState.Idle);  // Return to patrol or idle state
        }
    }

    void HandleRetreatState()
    {
        // Logic for retreat state, if needed
    }

    void ChangeState(BossState newState)
    {
        currentState = newState;
    }
}
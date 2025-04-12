using System.Collections;
using UnityEngine;

public abstract class BulletPatternBase : ScriptableObject
{
    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 5f;              // Initial bullet speed from ScriptableObject
    public float acceleration = 0f;             // Rate of acceleration
    public float bulletLifespan = 5f;           // Lifespan of the bullet (in seconds)


    [Header("Offset and Size")]
    [SerializeField]
    public float objectWidth = 1f;   // Width of the bullet prefab
    [SerializeField]
    public float objectHeight = 1f;  // Height of the bullet prefab
    [SerializeField]
    public float objectDepth = 1f;  // Height of the bullet prefab
    [SerializeField]
    public float xOffset = 0f;       // X offset for bullet spawn position
    [SerializeField]
    public float yOffset = 0f;       // Y offset for bullet spawn position
    [SerializeField]
    public float zOffset = 0f;       // Y offset for bullet spawn position


    [Header("Movement Settings")]
    public bool moveToPlayer = true;            // Toggle to decide if the bullet should move towards the player
    [Range(0f, 359f)] public float fireYaw = 0f; // Angle (0-359) to fire in a specific direction if not moving to the player
    [Range(-180f, 180f)] public float firePitch = 0f; // Pitch angle (vertical, -180 to 180)
    [Range(0f, 359f)] public float fireRoll = 0f;    // Roll angle (rotation about forward axis)
    public float fireOffsetDistance = 1f;

    [Header("Bullet Array Settings")]
    [SerializeField, Range(1, 10)]
    public int numberOfBulletsPerArray = 5;  // Number of bullets in a single array (1-10)
    [SerializeField, Range(1, 360)]
    public float individualArraySpread = 30f;  // Spread between bullets in the same array (1-360)
    [SerializeField, Range(1, 10)]
    public int totalBulletArrays = 3;  // Total number of bullet arrays (1-10)
    [SerializeField, Range(0, 360)]
    public float totalArraySpread = 90f;  // Spread between bullet arrays (1-360)

    [Header("Spin Settings")]
    public bool enableSpin = false;                  // Enable spin functionality
    [Range(-360f, 360f)] public float currentSpinSpeed = 0f; // Current spin speed (degrees per second)
    [Range(-180f, 180f)] public float spinSpeedChangeRate = 0f; // Rate of change of spin speed
    [Range(0f, 360f)] public float maxSpinSpeed = 180f; // Maximum spin speed (0-360)
    public bool spinReversal = false;                // Reverse spin when max/min speed is reached

    [Header("Sine Wave Settings")]
    [Range(-5f, 5f)] public float sineAmplitude = 1f;   // Sine wave amplitude (range: -5 to 5)
    [Range(-10f, 10f)] public float sineFrequency = 1f;  // Sine wave frequency (range: -10 to 10)
    public bool enableSineWave = false;                // Enable sine wave movement

    [Header("Cosine Wave Settings")]
    [Range(-5f, 5f)] public float cosineAmplitude = 1f;   // Cosine wave amplitude (range: -5 to 5)
    [Range(-10f, 10f)] public float cosineFrequency = 1f;  // Cosine wave frequency (range: -10 to 10)
    public bool enableCosineWave = false;              // Enable cosine wave movement

    [Header("Spiral Settings")]
    public bool enableSpiral = false;                  // Enable spiral movement
    [Range(0f, 360f)] public float spiralSpeed = 45f;  // Speed of the spiral rotation (degrees per second)
    public bool spiralClockwise = true;                // Toggle for spiral direction: clockwise or anti-clockwise
    public bool enableCone = false;
    [Range(0f, 360f)] public float conalSpeed = 45f;
    public bool coneClockwise = true;
    public float coneAngleDegrees = 30f;

    [Header("Homing Settings")]
    public bool enableHoming = false;                  // Enable or disable homing behavior
    public int maxStops = 1;                           // Number of times the bullet can stop and redirect
    public float stopDuration = 1f;                    // Time the bullet stops before redirecting
    public float initialMovementTime = 1f;             // Time to move in the initial direction before stopping
    public float curveDuration = 0.5f;                 // Time it takes to curve to the new direction

    private bool useSineWave = true;  // Tracks whether the current bullet should use sine or cosine wave

    public void SetTotalBulletArrays(int arrays)
    {
        totalBulletArrays = arrays;
    }

    public void SetIndividualArraySpread(float spread)
    {
        individualArraySpread = spread;
    }

    public virtual void Fire(Transform firePoint, Transform player = null)
    {
        SpawnBullets(firePoint, player); // Respect the pattern
    }

    public void SpawnBullets(Transform firePoint, Transform player)
    {
        // Loop through each bullet array.
        for (int arrayIndex = 0; arrayIndex < totalBulletArrays; arrayIndex++)
        {
            // Calculate an offset for multiple arrays.
            float arrayRotationOffset = (arrayIndex - (totalBulletArrays / 2f)) * totalArraySpread;

            for (int bulletIndex = 0; bulletIndex < numberOfBulletsPerArray; bulletIndex++)
            {
                float bulletAngle = (numberOfBulletsPerArray == 1)
                    ? 0f
                    : bulletIndex * (individualArraySpread / (numberOfBulletsPerArray - 1));

                Quaternion baseRotation;
                // If moveToPlayer is enabled and a player is provided, use the player's direction.
                if (moveToPlayer && player != null)
                {
                    // Compute the normalized direction from firePoint toward the player.
                    Vector3 toPlayer = (player.position - firePoint.position).normalized;
                    // Get the rotation that directs the firePoint toward the player.
                    Quaternion playerRotation = Quaternion.LookRotation(toPlayer);
                    // Apply the array offset to the yaw (rotating about the vertical axis).
                    baseRotation = playerRotation * Quaternion.Euler(0, arrayRotationOffset, 0);
                }
                else
                {
                    // Otherwise, use the inspector pitch/yaw.
                    baseRotation = Quaternion.Euler(firePitch, fireYaw + arrayRotationOffset + bulletAngle, 0);
                }

                // --- Compute the ball spawn origin ---
                // Use a default local offset that places the bullet at the top of the ball.
                Vector3 localOffset = new Vector3(0, fireOffsetDistance, 0);
                // Apply fireRoll: rotate the local offset about the baseRotation’s forward axis.
                // Negative fireRoll means:
                //   0°: spawn at top; 90°: right; 180°: bottom; 270°: left.
                Quaternion rollRotation = Quaternion.AngleAxis(-fireRoll, baseRotation * Vector3.forward);
                Vector3 rotatedLocalOffset = rollRotation * localOffset;
                // Transform the rotated offset by baseRotation to obtain a world-space offset.
                Vector3 worldOffset = baseRotation * rotatedLocalOffset;

                // Apply extra offsets purely as a translation.
                Vector3 extraOffset = baseRotation * new Vector3(xOffset, yOffset, zOffset);
                // Final spawn position: firePoint plus the ball offset and extra translation.
                Vector3 spawnPosition = firePoint.position + worldOffset + extraOffset;

                // Determine the bullet's travel direction from the firePoint to the ball spawn origin.
                // Note: extra offsets do NOT alter the computed firing direction.
                Vector3 directionToFire = (firePoint.position + worldOffset - firePoint.position).normalized;

                // Instantiate the bullet.
                GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
                bullet.transform.localScale = new Vector3(objectWidth, objectHeight, objectDepth);


                //// The final horizontal rotation (yaw) for this bullet.
                //float finalYaw = fireYaw + arrayRotationOffset + bulletAngle;

                //// Create the base rotation from the pitch and yaw.
                //// This rotation represents the overall orientation of the ball.
                //Quaternion baseRotation = Quaternion.Euler(firePitch, finalYaw, 0);

                //// Define a local offset corresponding to the "top" of the ball.
                //// When fireOffsetDistance = 1, the bullet spawns at (0,1,0) in local space.
                //Vector3 localOffset = new Vector3(0, fireOffsetDistance, 0);

                //// Apply the roll to the local offset.
                //// We want a fireRoll of 0 to leave the offset unchanged,
                //// 90 to rotate it so it appears on the right, 180 at bottom, 270 on the left.
                //// To do this, rotate localOffset around the forward axis in local space.
                //// One way to do this is to compute:
                ////   worldOffset = baseRotation * (Quaternion.AngleAxis(-fireRoll, Vector3.forward) * localOffset)
                //Quaternion rollRotation = Quaternion.AngleAxis(-fireRoll, Vector3.forward);
                //Vector3 rotatedLocalOffset = rollRotation * localOffset;

                //// Now, transform the rotated local offset by the base rotation to get the proper world-space offset.
                //Vector3 worldOffset = baseRotation * rotatedLocalOffset;

                //// If you have extra offsets, transform them by the same base rotation.
                //Vector3 extraOffset = baseRotation * new Vector3(xOffset, yOffset, zOffset);

                //// Final spawn position: add the computed offsets to the firePoint position.
                //Vector3 spawnPosition = firePoint.position + worldOffset + extraOffset;

                //// Compute the bullet's travel direction as the normalized vector from firePoint to spawnPosition.
                //Vector3 directionToFire = (spawnPosition - firePoint.position).normalized;

                //// Instantiate the bullet at spawnPosition.
                //GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
                //bullet.transform.localScale = new Vector3(objectWidth, objectHeight, objectDepth);

                // Optionally, adjust the bullet’s SphereCollider.
                SphereCollider collider = bullet.GetComponent<SphereCollider>();
                if (collider != null)
                {
                    float colliderRadius = Mathf.Min(objectWidth, Mathf.Min(objectHeight, objectDepth)) / 2f;
                    collider.radius = colliderRadius;
                }
                else
                {
                    Debug.LogWarning("Bullet prefab does not have a SphereCollider component.");
                }

                // Call your initialization routines (set velocity, start acceleration, etc.)
                InitializeBullet(bullet, firePoint, player, directionToFire);

                // Set the bullet’s initial velocity.
                Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
                if (bulletRb != null)
                {
                    bulletRb.linearVelocity = directionToFire * bulletSpeed;
                }

                // Destroy the bullet after its lifespan expires.
                Destroy(bullet, bulletLifespan);
            }

            // Optional spin logic: update fireYaw if spin is enabled.
            if (enableSpin)
            {
                fireYaw += currentSpinSpeed;
                currentSpinSpeed += spinSpeedChangeRate;
                currentSpinSpeed = Mathf.Clamp(currentSpinSpeed, -maxSpinSpeed, maxSpinSpeed);
                if (spinReversal && (Mathf.Abs(currentSpinSpeed) >= maxSpinSpeed))
                {
                    spinSpeedChangeRate = -spinSpeedChangeRate;
                }
                fireYaw = fireYaw % 360f;
                if (fireYaw < 0f)
                    fireYaw += 360f;
            }
        }
    }

    // Method to get the direction based on a specific angle (in degrees) relative to the firing point or player
    protected virtual Vector3 GetDirectionFromAngle(float yaw, Transform firePoint, Transform player)
    {
        if (moveToPlayer && player != null)
        {
            Vector3 toPlayer = player.position - firePoint.position;
            return toPlayer.normalized;
        }
        else
        {
            // Build a rotation from the provided yaw and the current firePitch.
            Quaternion rotation = Quaternion.Euler(firePitch, yaw, 0);
            return rotation * Vector3.forward;
        }
    }

    // Initialize the bullet with a custom direction
    protected void InitializeBullet(GameObject bullet, Transform firePoint, Transform player, Vector3 direction)
    {
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction * bulletSpeed;
            MonoBehaviour monoBehaviour = bullet.GetComponent<MonoBehaviour>();
            if (monoBehaviour != null)
            {
                monoBehaviour.StartCoroutine(AccelerateBullet(rb, direction, bulletSpeed));

                if (enableSineWave && enableCosineWave)
                {
                    if (useSineWave)
                    {
                        monoBehaviour.StartCoroutine(MoveBulletWithSineWave(bullet.transform, direction));
                    }
                    else
                    {
                        monoBehaviour.StartCoroutine(MoveBulletWithCosineWave(bullet.transform, direction));
                    }
                    useSineWave = !useSineWave;
                }
                if (enableSineWave)
                    monoBehaviour.StartCoroutine(MoveBulletWithSineWave(bullet.transform, direction));
                if (enableCosineWave)
                    monoBehaviour.StartCoroutine(MoveBulletWithCosineWave(bullet.transform, direction));
                if (enableSpiral)
                {
                    Vector3 origin = bullet.transform.position;
                    monoBehaviour.StartCoroutine(SpiralBulletOutward(bullet.transform, origin, direction, bulletSpeed, spiralSpeed, spiralClockwise));
                }
                if (enableCone)
                {
                    Vector3 origin = bullet.transform.position;
                    // 'direction' here is the axis along which the bullet will progress.
                    monoBehaviour.StartCoroutine(ConeBulletOutward(bullet.transform, origin, direction, bulletSpeed, conalSpeed, coneAngleDegrees, coneClockwise));
                }

                if (enableHoming)
                    monoBehaviour.StartCoroutine(HomingRoutine(bullet.transform, firePoint, player, rb));
            }
        }
    }

    // Get the initial direction based on moveToPlayer or fireAngle settings
    protected Vector3 GetInitialBulletDirection(Transform firePoint, Transform player)
    {
        if (moveToPlayer && player != null)
        {
            Vector3 toPlayer = player.position - firePoint.position;
            return toPlayer.normalized;
        }
        else
        {
            return GetDirectionFromAngle(fireYaw, firePoint, player);
        }
    }

    // Coroutine for homing behavior with stops and redirects
    protected IEnumerator HomingRoutine(Transform bulletTransform, Transform firePoint, Transform player, Rigidbody rb)
    {
        int currentStopCount = 0;
        float currentBulletSpeed = bulletSpeed;
        Vector3 direction = rb.linearVelocity.normalized;
        float elapsedTime = 0f;

        while (elapsedTime < initialMovementTime)
        {
            if (bulletTransform == null) yield break;
            elapsedTime += Time.deltaTime;
            currentBulletSpeed += acceleration * Time.deltaTime;
            rb.linearVelocity = direction * currentBulletSpeed;
            yield return null;
        }

        while (currentStopCount < maxStops)
        {
            if (bulletTransform == null || player == null) yield break;
            rb.linearVelocity = Vector3.zero;
            float stopElapsedTime = 0f;
            while (stopElapsedTime < stopDuration)
            {
                stopElapsedTime += Time.deltaTime;
                rb.linearVelocity = Vector3.zero;
                yield return null;
            }
            Vector3 newDirection = (player.position - bulletTransform.position).normalized;
            direction = newDirection;
            elapsedTime = 0f;
            while (elapsedTime < initialMovementTime && bulletTransform != null)
            {
                elapsedTime += Time.deltaTime;
                currentBulletSpeed += acceleration * Time.deltaTime;
                rb.linearVelocity = direction * currentBulletSpeed;
                yield return null;
            }
            currentStopCount++;
        }

        while (bulletTransform != null)
        {
            currentBulletSpeed += acceleration * Time.deltaTime;
            rb.linearVelocity = direction * currentBulletSpeed;
            yield return null;
        }
    }


    // This method smoothly curves the bullet toward the new direction during stops
    protected IEnumerator CurveBulletDirection(Transform bulletTransform, Vector3 oldDirection, Vector3 newDirection, float currentBulletSpeed, Rigidbody rb, Transform player)
    {
        float curveElapsedTime = 0f;
        while (curveElapsedTime < curveDuration)
        {
            if (bulletTransform == null) yield break;
            curveElapsedTime += Time.deltaTime;
            newDirection = (player.position - bulletTransform.position).normalized;
            Vector3 curvedDirection = Vector3.Lerp(oldDirection, newDirection, curveElapsedTime / curveDuration).normalized;
            currentBulletSpeed += acceleration * Time.deltaTime;
            rb.linearVelocity = curvedDirection * currentBulletSpeed;
            yield return null;
        }
    }

    // Coroutine for accelerating the bullet instance over time
    protected IEnumerator AccelerateBullet(Rigidbody rb, Vector3 moveDirection, float currentBulletSpeed)
    {
        while (rb != null)
        {
            currentBulletSpeed += acceleration * Time.deltaTime;
            rb.linearVelocity = moveDirection * currentBulletSpeed;

            // Update rotation to face the movement direction, including the roll offset.
            if (rb.linearVelocity.sqrMagnitude > 0.001f)
            {
                Quaternion lookRot = Quaternion.LookRotation(rb.linearVelocity);
                rb.transform.rotation = lookRot * Quaternion.Euler(0, 0, fireRoll);
            }

            yield return null;
        }
    }

    // Coroutine for cosine wave movement
    protected IEnumerator MoveBulletWithCosineWave(Transform bulletTransform, Vector3 moveDirection)
    {
        float elapsedTime = 0f;
        while (bulletTransform != null)
        {
            elapsedTime += Time.deltaTime;
            float cosineWaveOffset = Mathf.Cos(elapsedTime * cosineFrequency) * cosineAmplitude;
            Vector3 perpendicularDirection = new Vector3(-moveDirection.z, 0, moveDirection.x);
            Vector3 cosineWaveMovement = perpendicularDirection * cosineWaveOffset;
            bulletTransform.position += cosineWaveMovement * Time.deltaTime;
            yield return null;
        }
    }


    // Sine wave movement coroutine
    protected IEnumerator MoveBulletWithSineWave(Transform bulletTransform, Vector3 moveDirection)
    {
        float elapsedTime = 0f;
        while (bulletTransform != null)
        {
            elapsedTime += Time.deltaTime;
            float sineWaveOffset = Mathf.Sin(elapsedTime * sineFrequency) * sineAmplitude;
            Vector3 perpendicularDirection = new Vector3(-moveDirection.z, 0, moveDirection.x);
            Vector3 sineWaveMovement = perpendicularDirection * sineWaveOffset;
            bulletTransform.position += sineWaveMovement * Time.deltaTime;
            yield return null;
        }
    }

    // Spiral movement coroutine
    protected IEnumerator SpiralBulletOutward(Transform bulletTransform, Vector3 origin, Vector3 directionToPlayer, float currentBulletSpeed, float spiralSpeed, bool spiralClockwise)
    {
        float elapsedTime = 0f;
        float currentAngle = 0f;
        while (bulletTransform != null)
        {
            elapsedTime += Time.deltaTime;
            currentAngle += spiralClockwise ? spiralSpeed * Time.deltaTime : -spiralSpeed * Time.deltaTime;
            float radius = currentBulletSpeed * elapsedTime;
            Vector3 rotatedDirection = Quaternion.Euler(0, currentAngle, 0) * directionToPlayer;
            bulletTransform.position = origin + rotatedDirection * radius;
            yield return null;
        }
    }

    protected IEnumerator ConeBulletOutward(Transform bulletTransform, Vector3 origin, Vector3 axis, float bulletSpeed, float conalSpeed, float coneAngleDegrees, bool coneClockwise)
    {
        // Ensure our axis is normalized.
        Vector3 axisN = axis.normalized;
        float elapsedTime = 0f;
        while (bulletTransform != null)
        {
            elapsedTime += Time.deltaTime;
            // Compute the axial (forward) displacement along the cone's axis.
            float h = bulletSpeed * elapsedTime * Mathf.Cos(coneAngleDegrees * Mathf.Deg2Rad);
            // Compute the radial distance (distance from the axis).
            float r = bulletSpeed * elapsedTime * Mathf.Sin(coneAngleDegrees * Mathf.Deg2Rad);

            // Determine the rotation angle around the axis.
            float angle = (coneClockwise ? 1 : -1) * conalSpeed * elapsedTime;

            // Determine a perpendicular vector to the axis.
            Vector3 perp = Vector3.Cross(axisN, Vector3.up);
            if (perp.sqrMagnitude < 0.001f)
            {
                // If axis is nearly parallel to Vector3.up, choose an alternate.
                perp = Vector3.Cross(axisN, Vector3.right);
            }
            perp.Normalize();

            // Create a rotation about the cone axis.
            Quaternion rot = Quaternion.AngleAxis(angle, axisN);
            // Apply the rotation to the perpendicular vector scaled by r.
            Vector3 radialOffset = rot * (r * perp);

            // Calculate the final position as the sum of the axial displacement and the rotated radial offset.
            Vector3 pos = origin + (h * axisN) + radialOffset;
            bulletTransform.position = pos;

            yield return null;
        }
    }

    // New method to fire bullets in a specific direction
    public virtual void FireInDirection(Transform firePoint, Vector3 direction)
    {
        if (bulletPrefab == null) return;

        Vector3 spawnPosition = firePoint.position + new Vector3(xOffset, yOffset, zOffset);
        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
        bullet.transform.localScale = new Vector3(objectWidth, objectHeight, objectDepth);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * bulletSpeed;
            bullet.transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 0, fireRoll);
        }

        Destroy(bullet, bulletLifespan);
    }

    public void DrawFiringGizmo(Transform firePoint)
    {
        if (firePoint == null)
            return;

        // The origin point.
        Vector3 origin = firePoint.position;

        // Build a "base" rotation from firePitch and fireYaw.
        Quaternion baseRotation = Quaternion.Euler(firePitch, fireYaw, 0);

        // Draw base axes at the firePoint.
        float axisLength = fireOffsetDistance * 0.5f;

        // Forward axis (green).
        Gizmos.color = Color.green;
        Gizmos.DrawLine(origin, origin + baseRotation * Vector3.forward * axisLength);
        // Up axis (blue).
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(origin, origin + baseRotation * Vector3.up * axisLength);
        // Right axis (red).
        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin, origin + baseRotation * Vector3.right * axisLength);

        // Default offset vector representing the spawn point when fireRoll = 0.
        // By convention, (0, fireOffsetDistance, 0) is “top” of the ball.
        Vector3 defaultOffset = new Vector3(0, fireOffsetDistance, 0);

        // Apply the roll rotation: rotate the offset around the base forward axis.
        // Using a negative angle here so that:
        // • 0°: spawn at top,
        // • 90°: spawns to the right,
        // • 180°: at the bottom,
        // • 270°: to the left.
        Quaternion rollRotation = Quaternion.AngleAxis(-fireRoll, baseRotation * Vector3.forward);
        Vector3 rotatedOffset = rollRotation * defaultOffset;

        // Compute the final spawn position by transforming the rotated offset by the base rotation.
        // (Extra offsets, if any, could be added here in a similar local fashion.)
        Vector3 spawnPosition = origin + baseRotation * rotatedOffset;

        // Draw the final firing direction as a yellow line.
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, spawnPosition);

        // Draw a small sphere at the spawn point.
        Gizmos.DrawSphere(spawnPosition, 0.1f);

        // (Optional) Draw an arrow to indicate the final bullet direction.
        // Here we assume that the bullet’s travel direction is from the firePoint toward the spawnPosition.
        Vector3 finalDirection = (spawnPosition - origin).normalized;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(spawnPosition, spawnPosition + finalDirection * axisLength);
    }

}
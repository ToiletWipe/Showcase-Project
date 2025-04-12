using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "BurstPattern", menuName = "Bullet Patterns/Burst")]
public class BurstPattern : BulletPatternBase
{
    [Header("Burst Settings")]
    public int bulletsPerLocation = 2;  // Number of bullets to fire from the same location before switching
    public int burstCount = 6;          // Total number of burst sequences to fire
    public float burstFireRate = 0.1f;    // Time between bullets in a burst
    public float burstCountRate = 0.5f;   // Time between each burst sequence

    [Header("Offset Settings")]
    // Offsets for alternating burst positions.
    // Here, x represents horizontal burst offset and y represents burst depth offset.
    public Vector2[] fireOffsets;

    public override void Fire(Transform firePoint, Transform player = null)
    {
        // When moveToPlayer is enabled, require a valid player Transform.
        if (moveToPlayer && player == null) return;

        MonoBehaviour monoBehaviour = firePoint.GetComponent<MonoBehaviour>();
        if (monoBehaviour != null)
        {
            monoBehaviour.StartCoroutine(FireBurst(firePoint, player));
        }
    }

    private IEnumerator FireBurst(Transform firePoint, Transform player)
    {
        int currentOffsetIndex = 0;

        for (int i = 0; i < burstCount; i++)
        {
            // Handle spin if enabled.
            if (enableSpin)
            {
                fireYaw += currentSpinSpeed;
                currentSpinSpeed += spinSpeedChangeRate;
                currentSpinSpeed = Mathf.Clamp(currentSpinSpeed, -maxSpinSpeed, maxSpinSpeed);
                if (spinReversal && Mathf.Abs(currentSpinSpeed) >= maxSpinSpeed)
                {
                    spinSpeedChangeRate = -spinSpeedChangeRate;
                }
                fireYaw = fireYaw % 360f;
                if (fireYaw < 0f)
                {
                    fireYaw += 360f;
                }
            }

            for (int arrayIndex = 0; arrayIndex < totalBulletArrays; arrayIndex++)
            {
                // Calculate burst array offset.
                float arrayRotationOffset = (arrayIndex - (totalBulletArrays / 2f)) * totalArraySpread;

                // Build base rotation from firePitch and (fireYaw + array offset).
                Quaternion baseRotation = Quaternion.Euler(firePitch, fireYaw + arrayRotationOffset, 0);

                // --- Compute the ball's spawn origin ---
                // Default local offset: (0, fireOffsetDistance, 0) places the bullet at the "top" of the ball.
                Vector3 defaultOffset = new Vector3(0, fireOffsetDistance, 0);
                // Apply fireRoll: rotate the default offset about the baseRotation's forward axis.
                // Negative fireRoll: 0° -> top, 90° -> right, 180° -> bottom, 270° -> left.
                Quaternion rollRotation = Quaternion.AngleAxis(-fireRoll, baseRotation * Vector3.forward);
                Vector3 rotatedOffset = rollRotation * defaultOffset;
                // Transform that offset by baseRotation and add it to firePoint.position.
                Vector3 ballSpawnOrigin = firePoint.position + baseRotation * rotatedOffset;

                // --- Compute extra offsets as a pure world-space translation ---
                // Extra positional offsets are not rotated—they’re added directly.
                Vector3 worldExtraOffset = new Vector3(xOffset, yOffset, zOffset);
                // Additionally, add the burst-specific offset from fireOffsets (map its X to X and Y to Z).
                Vector3 burstOffset = new Vector3(fireOffsets[currentOffsetIndex].x, 0, fireOffsets[currentOffsetIndex].y);

                // Final spawn position is the ball spawn origin plus the extra world translation.
                Vector3 spawnPosition = ballSpawnOrigin + worldExtraOffset + burstOffset;

                // The firing direction is determined solely by the vector from firePoint to ballSpawnOrigin.
                // (Extra offsets only translate the spawn without affecting direction.)
                Vector3 directionToFire = (ballSpawnOrigin - firePoint.position).normalized;

                // Fire the designated number of bullets from this current offset.
                for (int j = 0; j < bulletsPerLocation; j++)
                {
                    GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
                    bullet.transform.localScale = new Vector3(objectWidth, objectHeight, objectDepth);

                    // Initialize bullet behavior (velocity, acceleration, etc.)
                    InitializeBullet(bullet, firePoint, player, directionToFire);
                    Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
                    if (bulletRb != null)
                    {
                        bulletRb.linearVelocity = directionToFire * bulletSpeed;
                    }
                    Destroy(bullet, bulletLifespan);

                    yield return new WaitForSeconds(burstFireRate);
                }
            }

            // Cycle to the next offset in the fireOffsets array.
            currentOffsetIndex = (currentOffsetIndex + 1) % fireOffsets.Length;
            yield return new WaitForSeconds(burstCountRate);
        }
    }

    // Initialize the bullet via the base class method.
    private void InitializeBurstBullet(GameObject bullet, Vector3 directionToFire, bool spiralClockwise, Transform firePoint, Transform player)
    {
        // Set up any pattern-specific parameters (for example, spiral direction).
        // Here we simply call InitializeBullet.
        InitializeBullet(bullet, firePoint, player, directionToFire);
    }

    // Override GetDirectionFromAngle for 3D horizontal movement.
    // (This method is less used in BurstPattern since we compute the firing direction from the ball spawn origin.)
    protected override Vector3 GetDirectionFromAngle(float angle, Transform firePoint, Transform player)
    {
        float rad = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad)).normalized;
    }
}

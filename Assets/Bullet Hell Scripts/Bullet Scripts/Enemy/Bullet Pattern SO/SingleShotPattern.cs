using UnityEngine;

[CreateAssetMenu(fileName = "SingleShotPattern", menuName = "Bullet Patterns/Single Shot")]
public class SingleShotPattern : BulletPatternBase
{
    // When moveToPlayer is true, use this offset (in local space) as the base spawn offset.
    // For example, (0, 1, 1) will position the spawn point in front and above the firePoint.
    [Header("Aiming Settings")]
    public Vector3 aimSpawnOffset = new Vector3(0, 1f, 1f);

    public override void Fire(Transform firePoint, Transform player = null)
    {
        // If moveToPlayer is enabled, require a valid player Transform.
        if (moveToPlayer && player == null)
            return;

        // --- Spin Logic ---
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

        // --- Loop through each bullet array ---
        for (int arrayIndex = 0; arrayIndex < totalBulletArrays; arrayIndex++)
        {
            float arrayRotationOffset = (arrayIndex - (totalBulletArrays / 2f)) * totalArraySpread;

            // --- Loop through each bullet in the array ---
            for (int bulletIndex = 0; bulletIndex < numberOfBulletsPerArray; bulletIndex++)
            {
                float bulletAngle = (numberOfBulletsPerArray == 1)
                    ? 0f
                    : bulletIndex * (individualArraySpread / (numberOfBulletsPerArray - 1));

                Quaternion baseRotation;
                Vector3 defaultOffset;

                if (moveToPlayer && player != null)
                {
                    // Determine the direction to the player.
                    Vector3 toPlayer = (player.position - firePoint.position).normalized;
                    // Create a rotation that points toward the player.
                    Quaternion playerRotation = Quaternion.LookRotation(toPlayer);
                    // Apply the extra yaw offsets (array offset + bullet spread).
                    baseRotation = playerRotation * Quaternion.Euler(0, arrayRotationOffset + bulletAngle, 0);
                    // When aiming at the player, use aimSpawnOffset (this offset should not be collinear with forward).
                    defaultOffset = aimSpawnOffset;
                }
                else
                {
                    float finalYaw = fireYaw + arrayRotationOffset + bulletAngle;
                    baseRotation = Quaternion.Euler(firePitch, finalYaw, 0);
                    defaultOffset = new Vector3(0, fireOffsetDistance, 0);
                }

                // --- Apply Roll ---
                // Rotate the default offset by fireRoll about the baseRotation�s forward axis.
                // Negative fireRoll rotates the spawn point so that:
                //   0� -> no change ("top" of ball),
                //   90� -> spawns to the right,
                //   180� -> spawns at the bottom,
                //   270� -> spawns to the left.
                Quaternion rollRotation = Quaternion.AngleAxis(-fireRoll, baseRotation * Vector3.forward);
                Vector3 rotatedOffset = rollRotation * defaultOffset;

                // --- Compute the Ball Spawn Origin ---
                // This position is calculated solely by the baseRotation and rotated offset.
                Vector3 ballSpawnOrigin = firePoint.position + baseRotation * rotatedOffset;

                // --- Apply Extra Offsets ---
                // Extra offsets are applied purely as a translation in world space.
                Vector3 extraOffset = new Vector3(xOffset, yOffset, zOffset);
                // Final spawn position: add extra offsets to the ball spawn origin.
                Vector3 spawnPosition = ballSpawnOrigin + extraOffset;

                // --- Compute Firing Direction ---
                // When moveToPlayer is true, force the bullet's travel direction to be from the spawn point toward the player.
                // Otherwise, use the direction from firePoint to the ball spawn origin.
                Vector3 directionToFire = (moveToPlayer && player != null)
                    ? (player.position - spawnPosition).normalized
                    : (ballSpawnOrigin - firePoint.position).normalized;

                // Instantiate the bullet.
                GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
                bullet.transform.localScale = new Vector3(objectWidth, objectHeight, objectDepth);

                // Optionally adjust the SphereCollider.
                SphereCollider collider = bullet.GetComponent<SphereCollider>();
                if (collider != null)
                {
                    float colliderRadius = Mathf.Min(objectWidth, objectHeight) * 0.5f;
                    collider.radius = colliderRadius;
                }
                else
                {
                    Debug.LogWarning("Bullet prefab does not have a SphereCollider component.");
                }

                // Initialize bullet behavior (velocity, acceleration, etc.)
                InitializeBullet(bullet, firePoint, player, directionToFire);

                Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
                if (bulletRb != null)
                {
                    bulletRb.linearVelocity = directionToFire * bulletSpeed;
                }

                Destroy(bullet, bulletLifespan);
            }
        }
    }
}

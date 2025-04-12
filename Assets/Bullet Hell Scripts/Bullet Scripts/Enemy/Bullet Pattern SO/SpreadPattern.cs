using UnityEngine;

[CreateAssetMenu(fileName = "SpreadPattern", menuName = "Bullet Patterns/Spread")]
public class SpreadPattern : BulletPatternBase
{
    [Header("Spread Settings")]
    public int bulletCount = 5;               // Number of bullets in the spread
    public float spreadAngle = 45f;           // Total cone angle of the spread (degrees)
    public float spreadDistance = 1f;         // Distance along the bullet's direction for its spawn

    public override void Fire(Transform firePoint, Transform player = null)
    {
        // Update fireYaw if spin is enabled.
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

        // Loop through each bullet array.
        for (int arrayIndex = 0; arrayIndex < totalBulletArrays; arrayIndex++)
        {
            // Calculate array offset.
            float arrayRotationOffset = (arrayIndex - (totalBulletArrays / 2f)) * totalArraySpread;
            float finalYaw = fireYaw + arrayRotationOffset;
            // Base rotation from pitch and yaw.
            Quaternion baseRotation = Quaternion.Euler(firePitch, finalYaw, 0);

            // ----- Compute the "ball" spawn origin (ignoring extra offsets) -----
            // When fireOffsetDistance > 0, the default spawn is at the top of the ball.
            Vector3 defaultOffset = new Vector3(0, fireOffsetDistance, 0);
            // Apply roll: rotate the default offset about the base rotation's forward axis.
            // Negative fireRoll yields: 0 -> top, 90 -> right, 180 -> bottom, 270 -> left.
            Quaternion rollRotation = Quaternion.AngleAxis(-fireRoll, baseRotation * Vector3.forward);
            Vector3 rotatedOffset = rollRotation * defaultOffset;
            // Ball spawn origin is firePoint plus the rotated offset transformed by baseRotation.
            Vector3 ballSpawnOrigin = firePoint.position + baseRotation * rotatedOffset;

            // Compute the base firing direction from firePoint to ballSpawnOrigin.
            Vector3 baseDirection = (ballSpawnOrigin - firePoint.position).normalized;

            // ----- Compute extra world-space offsets (applied as pure translation) -----
            Vector3 worldExtraOffset = new Vector3(xOffset, yOffset, zOffset);

            // ----- Apply spread: Fire multiple bullets around the base direction -----
            float startAngle = -spreadAngle / 2f;
            float angleStep = (bulletCount > 1) ? spreadAngle / (bulletCount - 1) : 0f;

            for (int i = 0; i < bulletCount; i++)
            {
                // Additional rotation angle for this bullet.
                float currentSpreadAngle = startAngle + (i * angleStep);
                // Rotate the baseDirection around the base up-axis.
                Quaternion spreadRotation = Quaternion.AngleAxis(currentSpreadAngle, baseRotation * Vector3.up);
                Vector3 bulletDirection = spreadRotation * baseDirection;

                // Final spawn position:
                // - Start with the firePoint position.
                // - Add the extra world-space offset.
                // - Then offset by spreadDistance along the bullet's direction.
                Vector3 spawnPosition = firePoint.position + worldExtraOffset + bulletDirection.normalized * spreadDistance;

                // Instantiate the bullet.
                GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
                bullet.transform.localScale = new Vector3(objectWidth, objectHeight, objectDepth);

                // Initialize bullet movement.
                InitializeBullet(bullet, firePoint, player, bulletDirection);
                Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
                if (bulletRb != null)
                {
                    bulletRb.linearVelocity = bulletDirection * bulletSpeed;
                }
                Destroy(bullet, bulletLifespan);
            }
        }
    }
}

using UnityEngine;

[CreateAssetMenu(fileName = "ConePattern", menuName = "Bullet Patterns/Cone")]
public class ConePattern : BulletPatternBase
{
    [Header("Cone Settings")]
    public int coneLayers = 4;                // Number of layers in the cone
    public int bulletsPerLayer = 6;           // Number of bullets per layer
    public float tipSpreadAngle = 15f;        // Spread angle at the tip (narrow)
    public float baseSpreadAngle = 45f;       // Spread angle at the base (wider)
    public float tipDistance = 1f;            // Distance of the tip layer from the fire point
    public float baseDistance = 3f;           // Distance of the base layer from the fire point

    public override void Fire(Transform firePoint, Transform player = null)
    {
        // If moveToPlayer is enabled but no player is provided, exit.
        if (moveToPlayer && player == null) return;

        // --- Update fireYaw if spin is enabled ---
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
            if (fireYaw < 0f) fireYaw += 360f;
        }

        // Loop through each bullet array.
        for (int arrayIndex = 0; arrayIndex < totalBulletArrays; arrayIndex++)
        {
            // Compute an array offset.
            float arrayRotationOffset = (arrayIndex - (totalBulletArrays / 2f)) * totalArraySpread;
            float finalYaw = fireYaw + arrayRotationOffset;
            // Build the base rotation from firePitch (vertical) and finalYaw (horizontal).
            Quaternion baseRotation = Quaternion.Euler(firePitch, finalYaw, 0);

            // --- Compute the ball spawn origin ---
            // This defines the point on the ball's surface where bullets originate.
            // Default local offset: (0, fireOffsetDistance, 0) => "top" of the ball.
            Vector3 defaultOffset = new Vector3(0, fireOffsetDistance, 0);
            // Apply the roll: rotate the default offset about the base rotation's forward axis.
            // Using a negative angle: 0� -> top, 90� -> right, 180� -> bottom, 270� -> left.
            Quaternion rollRotation = Quaternion.AngleAxis(-fireRoll, baseRotation * Vector3.forward);
            Vector3 rotatedOffset = rollRotation * defaultOffset;
            // The ball spawn origin is the firePoint plus the rotated offset (transformed by baseRotation).
            Vector3 ballSpawnOrigin = firePoint.position + baseRotation * rotatedOffset;

            // The base firing direction is from the firePoint toward the ballSpawnOrigin.
            Vector3 baseDirection = (ballSpawnOrigin - firePoint.position).normalized;

            // --- Extra world-space offsets as pure translation ---
            Vector3 worldExtraOffset = new Vector3(xOffset, yOffset, zOffset);

            // --- Now, for each cone layer ---
            for (int layer = 0; layer < coneLayers; layer++)
            {
                // Layer position factor: 0 for tip, 1 for base.
                float layerFactor = (coneLayers > 1) ? (float)layer / (coneLayers - 1) : 0f;
                // Interpolate spread angle and layer distance between tip and base.
                float currentSpreadAngle = Mathf.Lerp(tipSpreadAngle, baseSpreadAngle, layerFactor);
                float layerDistance = Mathf.Lerp(tipDistance, baseDistance, layerFactor);

                // Compute the spread: distribute bullets across the cone width.
                float startAngle = -currentSpreadAngle / 2f;
                float angleStep = (bulletsPerLayer > 1) ? (currentSpreadAngle / (bulletsPerLayer - 1)) : 0f;

                for (int bulletIndex = 0; bulletIndex < bulletsPerLayer; bulletIndex++)
                {
                    float currentSpread = startAngle + (bulletIndex * angleStep);
                    // Apply an additional rotation around the base up-axis (from baseRotation) to spread the bullets.
                    Quaternion spreadRotation = Quaternion.AngleAxis(currentSpread, baseRotation * Vector3.up);
                    Vector3 bulletDirection = spreadRotation * baseDirection;

                    // Final spawn position: start at firePoint, then add the extra world-space offset,
                    // then move along the bulletDirection for the current layer's distance.
                    Vector3 spawnPosition = firePoint.position + worldExtraOffset + bulletDirection * layerDistance;

                    // Instantiate the bullet.
                    GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
                    bullet.transform.localScale = new Vector3(objectWidth, objectHeight, objectDepth);

                    // Initialize bullet movement (setting velocity, acceleration, etc.)
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
}

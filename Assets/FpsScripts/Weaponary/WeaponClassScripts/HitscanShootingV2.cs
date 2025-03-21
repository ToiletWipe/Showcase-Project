using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class HitscanShootingV2 : MonoBehaviour
{
    public WeaponManager weaponManager; // Reference to the WeaponManager
    public Transform bulletSpawnPoint; // Spawn point for bullets
    public LayerMask mask; // Layer mask for raycasting
    public RicochetTrajectoryVisualizer trajectoryVisualizer; // Reference to the trajectory visualizer
    public List<ScreenShake> screenShakes; // List of ScreenShake scripts (one for each gun image)

    private float lastShootTime;
    private bool isFiring; // Track if the fire button is held down

    private void Update()
    {
        Weapon currentWeapon = weaponManager.weapons[weaponManager.currentWeaponIndex];

        // Check for left mouse button press (shooting)
        if (Input.GetMouseButtonDown(0)) // 0 = left mouse button
        {
            StartFiring();
        }

        // Check for left mouse button release (stop shooting)
        if (Input.GetMouseButtonUp(0)) // 0 = left mouse button
        {
            StopFiring();
        }

        // Handle shooting for all weapons (respect fireRate)
        if (isFiring && weaponManager.CanShoot())
        {
            if (currentWeapon.rapidFire && Time.time >= lastShootTime + currentWeapon.fireRate)
            {
                Shoot();
                lastShootTime = Time.time; // Update the last shoot time
            }
        }

        // Visualize trajectory when holding right mouse button (only for the sniper)
        if (currentWeapon.weaponName == "Sniper") // Replace "Sniper" with the exact name of your sniper weapon
        {
            if (Input.GetMouseButton(1)) // 1 = right mouse button
            {
                trajectoryVisualizer.Initialize(bulletSpawnPoint, currentWeapon);
                trajectoryVisualizer.DrawRicochetTrajectory();
            }
            else
            {
                // Stop drawing the trajectory when right mouse button is released
                trajectoryVisualizer.StopDrawingTrajectory();
            }
        }
    }

    private void StartFiring()
    {
        isFiring = true;

        // Get the current weapon
        Weapon currentWeapon = weaponManager.weapons[weaponManager.currentWeaponIndex];

        // If the current weapon has rapid fire, shoot immediately (if fireRate allows)
        if (currentWeapon.rapidFire && weaponManager.CanShoot())
        {
            if (Time.time >= lastShootTime + currentWeapon.fireRate)
            {
                Shoot();
                lastShootTime = Time.time; // Update the last shoot time
            }
        }
        else if (!currentWeapon.rapidFire && weaponManager.CanShoot())
        {
            // If the weapon does NOT have rapid fire, shoot once
            Shoot();
        }
    }

    private void StopFiring()
    {
        isFiring = false;
    }

    public void Shoot()
    {
        Weapon currentWeapon = weaponManager.weapons[weaponManager.currentWeaponIndex];

        // Check if the player can shoot
        if (!weaponManager.CanShoot())
        {
            return;
        }

        // Deduct ammo
        currentWeapon.currentAmmo--;
        Debug.Log("Shoot called! Current Ammo: " + currentWeapon.currentAmmo); // Debug log
        weaponManager.UpdateAmmoUI(); // Call the public method to update the ammo UI

        // Play the muzzle flash particle system
        if (currentWeapon.muzzleFlash != null)
        {
            currentWeapon.muzzleFlash.Stop(); // Stop the particle system to reset it
            currentWeapon.muzzleFlash.Play(); // Play the particle system
        }

        // Trigger screen shake for the current weapon's UI image
        if (screenShakes.Count > weaponManager.currentWeaponIndex)
        {
            ScreenShake currentScreenShake = screenShakes[weaponManager.currentWeaponIndex];
            if (currentScreenShake != null)
            {
                currentScreenShake.TriggerShake(currentWeapon.screenShakeDuration, currentWeapon.screenShakeMagnitude);
            }
            else
            {
                Debug.LogError($"ScreenShake script for weapon {currentWeapon.weaponName} is not assigned!");
            }
        }
        else
        {
            Debug.LogError($"No ScreenShake script assigned for weapon {currentWeapon.weaponName}!");
        }

        Vector3 direction = transform.forward;
        TrailRenderer trail = Instantiate(currentWeapon.bulletTrail, bulletSpawnPoint.position, Quaternion.identity);

        RaycastHit hit; // Declare the hit variable here
        if (Physics.Raycast(bulletSpawnPoint.position, direction, out hit, float.MaxValue, mask))
        {
            // Apply force and damage to the object
            ApplyForceToObject(hit.collider, direction, currentWeapon.bulletForce, currentWeapon.damage);

            // Spawn impact particle system
            if (currentWeapon.impactParticleSystem != null)
            {
                Instantiate(currentWeapon.impactParticleSystem, hit.point, Quaternion.LookRotation(hit.normal));
            }

            StartCoroutine(SpawnTrail(trail, hit.point, hit.normal, currentWeapon.bounceDistance, true));
        }
        else
        {
            StartCoroutine(SpawnTrail(trail, bulletSpawnPoint.position + direction * 100, Vector3.zero, currentWeapon.bounceDistance, false));
        }
    }

    private void ApplyForceToObject(Collider collider, Vector3 direction, float bulletForce, float damage)
    {
        Rigidbody rb = collider.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(direction * bulletForce, ForceMode.Impulse);
        }

        // Apply damage if the object has a Health component
        Health health = collider.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }
    }

    private IEnumerator SpawnTrail(TrailRenderer trail, Vector3 hitPoint, Vector3 hitNormal, float bounceDistance, bool madeImpact)
    {
        Vector3 startPosition = trail.transform.position;
        Vector3 direction = (hitPoint - trail.transform.position).normalized;

        float distance = Vector3.Distance(trail.transform.position, hitPoint);
        float startingDistance = distance;

        while (distance > 0)
        {
            trail.transform.position = Vector3.Lerp(startPosition, hitPoint, 1 - (distance / startingDistance));
            distance -= Time.deltaTime * weaponManager.weapons[weaponManager.currentWeaponIndex].bulletSpeed;

            yield return null;
        }

        trail.transform.position = hitPoint;

        if (madeImpact)
        {
            if (weaponManager.weapons[weaponManager.currentWeaponIndex].bouncingBullets && bounceDistance > 0)
            {
                Vector3 bounceDirection = Vector3.Reflect(direction, hitNormal);

                // Declare the hit variable outside the if block
                RaycastHit hit;

                if (Physics.Raycast(hitPoint, bounceDirection, out hit, bounceDistance, mask))
                {
                    // Apply force to the bounced object if it has a Rigidbody
                    ApplyForceToObject(hit.collider, bounceDirection, weaponManager.weapons[weaponManager.currentWeaponIndex].bulletForce, weaponManager.weapons[weaponManager.currentWeaponIndex].damage);

                    yield return StartCoroutine(SpawnTrail(
                        trail,
                        hit.point,
                        hit.normal,
                        bounceDistance - Vector3.Distance(hit.point, hitPoint),
                        true
                    ));
                }
                else
                {
                    yield return StartCoroutine(SpawnTrail(
                        trail,
                        hitPoint + bounceDirection * bounceDistance,
                        Vector3.zero,
                        0,
                        false
                    ));
                }
            }
        }

        Destroy(trail.gameObject, trail.time);
    }
}
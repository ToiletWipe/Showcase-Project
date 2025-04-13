using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class HitscanShootingV2 : MonoBehaviour
{
    public WeaponManager weaponManager;
    public Transform bulletSpawnPoint;
    public LayerMask mask;
    public RicochetTrajectoryVisualizer trajectoryVisualizer;
    public List<ScreenShake> screenShakes;
    public GrenadeThrower grenadeThrower; // NEW: Grenade reference

    private float lastShootTime;
    private bool isFiring;

    private void Update()
    {
        Weapon currentWeapon = weaponManager.weapons[weaponManager.currentWeaponIndex];

        // Left-Click: Shooting
        if (Input.GetMouseButtonDown(0))
        {
            StartFiring();
        }
        if (Input.GetMouseButtonUp(0))
        {
            StopFiring();
        }

        // Rapid Fire Handling
        if (isFiring && weaponManager.CanShoot())
        {
            if (currentWeapon.rapidFire && Time.time >= lastShootTime + currentWeapon.fireRate)
            {
                Shoot();
                lastShootTime = Time.time;
            }
        }

        // Right-Click: Sniper Trajectory OR SMG Grenade
        if (Input.GetMouseButtonDown(1)) // Right-click pressed
        {
            if (currentWeapon.weaponName == "Sniper")
            {
                trajectoryVisualizer.Initialize(bulletSpawnPoint, currentWeapon);
                trajectoryVisualizer.DrawRicochetTrajectory();
            }
            else if (currentWeapon.weaponName == "SMG" && grenadeThrower != null)
            {
                grenadeThrower.TryThrowGrenade(); // Throw grenade
            }
        }

        // Stop trajectory when right-click is released (sniper only)
        if (Input.GetMouseButtonUp(1) && currentWeapon.weaponName == "Sniper")
        {
            trajectoryVisualizer.StopDrawingTrajectory();
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
        // If the collider is NOT tagged as "Enemy", add force.
        if (!collider.CompareTag("Enemy"))
        {
            Rigidbody rb = collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(direction * bulletForce, ForceMode.Impulse);
                Debug.Log("Applied force to: " + collider.name);
            }
        }

        // Apply damage if the object has a Health component
        Health health = collider.GetComponent<Health>();
        // Only apply damage if the collider is tagged as "Enemy" 
        // and the Health component is on the same GameObject (i.e. the main collider), not on a child detection collider.
        if (collider.CompareTag("Enemy") && health != null)
        {
            // This check assumes that the main enemy collider's GameObject has the Health component.
            if (collider.gameObject == health.gameObject)
            {
                health.TakeDamage(damage);
                Debug.Log("Hit " + collider.name + " for " + damage + " damage!");
            }
        }
        else
        {
            // For any other objects (or enemies whose colliders are on the main object), apply damage if Health is found.
            if (health != null)
            {
                health.TakeDamage(damage);
                Debug.Log("Hit " + collider.name + " for " + damage + " damage!");
            }
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
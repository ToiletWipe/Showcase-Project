using UnityEngine;

[System.Serializable]
public class Weapon
{
    public string weaponName; // Name of the weapon
    public Sprite weaponIcon; // Icon to display in the UI
    public float fireRate;    // Delay between shots
    public float bulletSpeed; // Speed of the bullet
    public float bulletForce; // Force applied to bullets
    public float damage;      // Damage dealt by the weapon
    public bool bouncingBullets; // Whether bullets can bounce
    public float bounceDistance; // Maximum distance for bouncing bullets
    public bool rapidFire; // Whether the weapon has rapid fire capability
    public ParticleSystem muzzleFlash; // Muzzle flash particle system
    public ParticleSystem impactParticleSystem; // Impact particle system
    public TrailRenderer bulletTrail;  // Bullet trail renderer

    // Ammo properties
    public int maxAmmo; // Maximum ammo capacity
    public int currentAmmo; // Current ammo count
    public float reloadTime; // Time it takes to reload

    // Reload UI
    public GameObject reloadUI; // Reference to the reload UI GameObject for this weapon
}
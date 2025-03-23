using UnityEngine;

public class GrenadeThrower : MonoBehaviour
{
    [Header("Grenade Settings")]
    public GameObject grenadePrefab; // Prefab for the grenade
    public float throwForce = 10f; // Force applied to throw the grenade
    public float explosionDelay = 3f; // Delay before the grenade explodes
    public float explosionRadius = 5f; // Radius of the explosion
    public float explosionForce = 1000f; // Force of the explosion
    public GameObject explosionParticles; // Particle effect for the explosion
    public float explosionDamage = 50f; // Damage dealt by the explosion

    [Header("Meter Settings")]
    public MeterSystem meterSystem; // Reference to the MeterSystem
    public float meterCostToThrow = 20f; // Meter cost to throw a grenade

    void Update()
    {
        // Check if the "G" key is pressed
        if (Input.GetKeyDown(KeyCode.G))
        {
            TryThrowGrenade();
        }
    }

    void TryThrowGrenade()
    {
        // Check if there's enough meter to throw a grenade
        if (meterSystem.HasEnoughMeter(meterCostToThrow))
        {
            ThrowGrenade();
            meterSystem.DeductFromMeter(meterCostToThrow); // Deduct the meter cost
        }
        else
        {
            Debug.Log("Not enough meter to throw a grenade!");
        }
    }

    void ThrowGrenade()
    {
        // Instantiate the grenade at the player's position
        GameObject grenade = Instantiate(grenadePrefab, transform.position + transform.forward, Quaternion.identity);

        // Apply force to the grenade
        Rigidbody grenadeRb = grenade.GetComponent<Rigidbody>();
        if (grenadeRb != null)
        {
            grenadeRb.AddForce(transform.forward * throwForce, ForceMode.Impulse);
        }

        // Initialize the grenade's explosion script
        BlackHoleExplosion explosionScript = grenade.GetComponent<BlackHoleExplosion>();
        if (explosionScript != null)
        {
            explosionScript.Initialize(explosionRadius, explosionForce, explosionDelay, explosionParticles, explosionDamage);
        }
        else
        {
            Debug.LogError("BlackHoleExplosion script not found on grenade prefab!");
        }
    }
}
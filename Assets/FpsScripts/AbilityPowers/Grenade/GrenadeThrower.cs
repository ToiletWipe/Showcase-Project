using UnityEngine;

public class GrenadeThrower : MonoBehaviour
{
    [Header("Grenade Settings")]
    public GameObject grenadePrefab;
    public float throwForce = 10f;
    public float explosionDelay = 3f;
    public float explosionRadius = 5f;
    public float explosionForce = 1000f;
    public GameObject explosionParticles;
    public float explosionDamage = 50f;

    [Header("Meter Settings")]
    public MeterSystem meterSystem;
    public float meterCostToThrow = 20f;

    // Now called by HitscanShootingV2 (no Update() needed)
    public void TryThrowGrenade()
    {
        if (meterSystem.HasEnoughMeter(meterCostToThrow))
        {
            ThrowGrenade();
            meterSystem.DeductFromMeter(meterCostToThrow);
        }
        else
        {
            Debug.Log("Not enough meter to throw grenade!");
        }
    }

    private void ThrowGrenade()
    {
        GameObject grenade = Instantiate(grenadePrefab, transform.position + transform.forward, Quaternion.identity);
        Rigidbody grenadeRb = grenade.GetComponent<Rigidbody>();
        if (grenadeRb != null)
        {
            grenadeRb.AddForce(transform.forward * throwForce, ForceMode.Impulse);
        }

        BlackHoleExplosion explosionScript = grenade.GetComponent<BlackHoleExplosion>();
        if (explosionScript != null)
        {
            explosionScript.Initialize(explosionRadius, explosionForce, explosionDelay, explosionParticles, explosionDamage);
        }
    }
}
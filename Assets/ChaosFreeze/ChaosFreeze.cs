using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaoticFreeze : MonoBehaviour
{
    [Header("Chaotic Settings")]
    public LayerMask bitsLayer; // Layer for "Bits" objects
    public float chaosRange = 5f; // Range to detect "Bits" objects
    public float chaosForce = 50f; // Force applied to make objects fly randomly
    public float chaosDuration = 2f; // Duration of the chaotic movement
    public float freezeDelay = 1f; // Delay before freezing the objects

    private List<Rigidbody> chaoticObjects = new List<Rigidbody>(); // List of objects to be affected
    private bool isChaotic = false; // Whether the chaotic effect is active

    void Update()
    {
        // Start chaotic movement when "X" is pressed
        if (Input.GetKeyDown(KeyCode.X))
        {
            StartChaos();
        }
    }

    // Start the chaotic movement
    void StartChaos()
    {
        if (isChaotic) return; // Prevent multiple triggers

        isChaotic = true;
        chaoticObjects.Clear(); // Clear previous objects

        // Find all "Bits" objects within range
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, chaosRange, bitsLayer);
        foreach (var hitCollider in hitColliders)
        {
            Rigidbody rb = hitCollider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                chaoticObjects.Add(rb);

                // Add a script to handle collisions
                ChaosObject chaosObject = hitCollider.GetComponent<ChaosObject>();
                if (chaosObject == null)
                {
                    chaosObject = hitCollider.gameObject.AddComponent<ChaosObject>();
                }
                chaosObject.chaoticFreezeScript = this;
            }
        }

        // Apply random forces to the objects
        StartCoroutine(ApplyChaos());
    }

    // Apply random forces to the objects
    IEnumerator ApplyChaos()
    {
        float startTime = Time.time;

        while (Time.time - startTime < chaosDuration)
        {
            foreach (var rb in chaoticObjects)
            {
                if (rb != null)
                {
                    // Apply a random force in a random direction
                    Vector3 randomDirection = Random.onUnitSphere;
                    rb.AddForce(randomDirection * chaosForce, ForceMode.Impulse);
                }
            }

            yield return null; // Wait for the next frame
        }

        // Freeze the objects after the chaos duration
        StartCoroutine(FreezeObjects());
    }

    // Freeze the objects in place
    IEnumerator FreezeObjects()
    {
        yield return new WaitForSeconds(freezeDelay);

        foreach (var rb in chaoticObjects)
        {
            if (rb != null)
            {
                //rb.linearVelocity = Vector3.zero; // Stop movement
                //rb.angularVelocity = Vector3.zero; // Stop rotation
                rb.isKinematic = true; // Freeze the object
            }
        }

        isChaotic = false; // Reset the chaotic state
    }

    // Unfreeze an object when it is hit
    public void UnfreezeObject(Rigidbody rb)
    {
        if (rb != null)
        {
            rb.isKinematic = false; // Unfreeze the object
            rb.AddForce(Vector3.down * 10f, ForceMode.Impulse); // Apply a downward force to make it fall
        }
    }
}

// Script to handle collisions on individual objects
public class ChaosObject : MonoBehaviour
{
    public ChaoticFreeze chaoticFreezeScript; // Reference to the main script
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if the object is frozen and is hit by a projectile or another object
        if (rb.isKinematic && chaoticFreezeScript != null)
        {
            chaoticFreezeScript.UnfreezeObject(rb);
        }
    }
}
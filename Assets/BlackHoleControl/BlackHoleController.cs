using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHoleController : MonoBehaviour
{
    [Header("Sucking Settings")]
    public float suckRadius = 5f; // Radius to detect objects
    public LayerMask bitsLayer; // Layer for "Bits" objects
    public float suckForce = 50f; // Force applied to suck objects
    public float minDistanceToDestroy = 0.5f; // Distance at which objects are destroyed

    [Header("Black Hole Settings")]
    public GameObject blackHolePrefab; // Prefab for the black hole
    public float growthRate = 0.1f; // How much the black hole grows per object
    public float maxBlackHoleSize = 5f; // Maximum size of the black hole
    public float blackHoleOffset = 2f; // Distance in front of the player to spawn the black hole

    [Header("Throw Settings")]
    public float throwForce = 500f; // Force applied to throw the black hole

    private GameObject currentBlackHole; // Reference to the current black hole
    private List<Rigidbody> suckedObjects = new List<Rigidbody>(); // List of sucked objects

    void Update()
    {
        // Create and grow the black hole when "G" is held down
        if (Input.GetKey(KeyCode.G))
        {
            if (currentBlackHole == null)
            {
                CreateBlackHole();
            }
            SuckObjects();
        }
        // Release and throw the black hole when "G" is released
        else if (Input.GetKeyUp(KeyCode.G))
        {
            ThrowBlackHole();
        }

        // Move the black hole with the player while it's active
        if (currentBlackHole != null && Input.GetKey(KeyCode.G))
        {
            currentBlackHole.transform.position = transform.position + transform.forward * blackHoleOffset;
        }
    }

    // Create the black hole
    void CreateBlackHole()
    {
        currentBlackHole = Instantiate(blackHolePrefab, transform.position + transform.forward * blackHoleOffset, Quaternion.identity);
        currentBlackHole.transform.localScale = Vector3.one * 0.1f; // Start small

        // Add a trigger collider to detect objects
        SphereCollider collider = currentBlackHole.AddComponent<SphereCollider>();
        collider.radius = 0.5f; // Adjust size as needed
        collider.isTrigger = true;

        // Add a script to handle collisions with "Bits" objects
        currentBlackHole.AddComponent<BlackHoleCollisionHandler>();
    }

    // Detect and suck objects
    void SuckObjects()
    {
        if (currentBlackHole == null) return;

        Collider[] hitColliders = Physics.OverlapSphere(currentBlackHole.transform.position, suckRadius, bitsLayer);
        foreach (var hitCollider in hitColliders)
        {
            Rigidbody rb = hitCollider.GetComponent<Rigidbody>();
            if (rb != null && !suckedObjects.Contains(rb))
            {
                suckedObjects.Add(rb);

                // Unfreeze position and rotation constraints
                rb.constraints = RigidbodyConstraints.None;
            }
        }

        // Apply suck force to all sucked objects
        foreach (var rb in suckedObjects)
        {
            if (rb != null)
            {
                Vector3 direction = (currentBlackHole.transform.position - rb.position).normalized;
                rb.AddForce(direction * suckForce, ForceMode.Acceleration);

                // Destroy the object if it's close enough to the black hole
                if (Vector3.Distance(rb.position, currentBlackHole.transform.position) < minDistanceToDestroy)
                {
                    Destroy(rb.gameObject);
                    GrowBlackHole();
                }
            }
        }
    }

    // Grow the black hole
    void GrowBlackHole()
    {
        if (currentBlackHole != null && currentBlackHole.transform.localScale.x < maxBlackHoleSize)
        {
            currentBlackHole.transform.localScale += new Vector3(growthRate, growthRate, growthRate);
        }
    }

    // Throw the black hole
    void ThrowBlackHole()
    {
        if (currentBlackHole != null)
        {
            // Disable the trigger collider to stop sucking objects
            Collider blackHoleCollider = currentBlackHole.GetComponent<Collider>();
            if (blackHoleCollider != null)
            {
                blackHoleCollider.enabled = false;
            }

            // Add a Rigidbody if it doesn't have one
            Rigidbody rb = currentBlackHole.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = currentBlackHole.AddComponent<Rigidbody>();
            }

            // Apply force to throw the black hole
            rb.AddForce(transform.forward * throwForce, ForceMode.Impulse);

            // Reset for the next black hole
            currentBlackHole = null;
            suckedObjects.Clear();
        }
    }
}

// Script to handle collisions with the black hole
public class BlackHoleCollisionHandler : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // Check if the object is in the "Bits" layer
        if (other.gameObject.layer == LayerMask.NameToLayer("Bits"))
        {
            // Destroy the object
            Destroy(other.gameObject);
        }
    }
}
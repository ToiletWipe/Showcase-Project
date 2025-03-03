using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHoleControllerV2 : MonoBehaviour
{
    [Header("Sucking Settings")]
    public float suckRadius = 5f; // Radius to detect objects
    public LayerMask bitsLayer; // Layer for "Bits" objects
    public float suckForce = 50f; // Force applied to suck objects
    public float minDistanceToDisable = 0.5f; // Distance at which objects are disabled

    [Header("Black Hole Settings")]
    public GameObject blackHolePrefab; // Prefab for the black hole
    public float growthRate = 0.1f; // How much the black hole grows per object
    public float maxBlackHoleSize = 5f; // Maximum size of the black hole
    public float blackHoleOffset = 2f; // Distance in front of the player to spawn the black hole

    [Header("Throw Settings")]
    public float throwForce = 500f; // Force applied to throw the black hole

    private GameObject currentBlackHole; // Reference to the current black hole
    private List<Rigidbody> suckedObjects = new List<Rigidbody>(); // List of sucked objects
    private int disabledObjectCount = 0; // Track the number of disabled objects

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
        currentBlackHole.AddComponent<BlackHoleDisableHandler>().controller = this;

        // Reset disabled object count
        disabledObjectCount = 0;
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

                // Manually set the center of mass for irregularly shaped objects
                SetCenterOfMass(rb, true);
            }
        }

        // Apply suck force to all sucked objects
        foreach (var rb in suckedObjects)
        {
            if (rb != null)
            {
                Vector3 direction = (currentBlackHole.transform.position - rb.position).normalized;
                rb.AddForce(direction * suckForce, ForceMode.Acceleration);

                // Disable the object if it's close enough to the black hole
                if (Vector3.Distance(rb.position, currentBlackHole.transform.position) < minDistanceToDisable)
                {
                    DisableObject(rb.gameObject);
                }
            }
        }
    }

    // Set or reset the center of mass
    void SetCenterOfMass(Rigidbody rb, bool manual)
    {
        if (manual)
        {
            // Calculate the center of mass based on the object's bounds
            Bounds bounds = rb.GetComponent<Collider>().bounds;
            rb.centerOfMass = bounds.center - rb.transform.position;
        }
        else
        {
            // Reset to auto center of mass
            rb.ResetCenterOfMass();
        }
    }

    // Disable an object
    public void DisableObject(GameObject obj)
    {
        if (obj.activeSelf) // Ensure the object is active before disabling it
        {
            obj.SetActive(false); // Disable the object
            disabledObjectCount++; // Increment the disabled object count
            GrowBlackHole(); // Grow the black hole
        }
    }

    // Grow the black hole
    void GrowBlackHole()
    {
        if (currentBlackHole != null && currentBlackHole.transform.localScale.x < maxBlackHoleSize)
        {
            // Increase the size of the black hole based on the number of disabled objects
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

            // Reset the center of mass for all sucked objects
            foreach (var suckedRb in suckedObjects)
            {
                if (suckedRb != null)
                {
                    SetCenterOfMass(suckedRb, false); // Reset to auto center of mass
                }
            }

            suckedObjects.Clear();
            disabledObjectCount = 0; // Reset disabled object count
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

public class ObjectSucker : MonoBehaviour
{
    [Header("Sucking Settings")]
    public float suckRadius = 5f; // Radius to detect objects
    public LayerMask suckableLayer; // Layer for objects that can be sucked
    public float suckForce = 50f; // Force applied to suck objects
    public float minDistanceToDisable = 0.5f; // Distance at which objects are disabled
    public float meterGainPerObject = 10f; // Amount of meter gained per sucked object

    [Header("Black Hole Settings")]
    public GameObject blackHolePrefab; // Black hole prefab to show when sucking
    public Vector3 blackHoleOffset = new Vector3(0, 0, 2); // Offset for the black hole position

    public MeterSystem meterSystem; // Reference to the MeterSystem

    private List<Rigidbody> suckedObjects = new List<Rigidbody>(); // List of sucked objects
    private GameObject currentBlackHole; // Reference to the spawned black hole

    void Update()
    {
        // Suck objects when a key is pressed (e.g., Q) and the meter is not full
        if (Input.GetKey(KeyCode.Q) && !meterSystem.HasEnoughMeter(meterSystem.maxMeter))
        {
            // Show the black hole if it's not already shown
            if (currentBlackHole == null)
            {
                currentBlackHole = Instantiate(blackHolePrefab, transform.position + transform.forward * blackHoleOffset.z, Quaternion.identity);
                currentBlackHole.transform.parent = transform; // Make the black hole a child of the player
            }

            // Update the black hole's position to follow the player
            currentBlackHole.transform.position = transform.position + transform.forward * blackHoleOffset.z;

            SuckObjects();
        }
        else
        {
            // Stop sucking and hide the black hole
            if (currentBlackHole != null)
            {
                Destroy(currentBlackHole);
                currentBlackHole = null;
            }
        }
    }

    // Detect and suck objects
    void SuckObjects()
    {
        // Find all colliders within the suck radius on the specified layer
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, suckRadius, suckableLayer);
        foreach (var hitCollider in hitColliders)
        {
            Rigidbody rb = hitCollider.GetComponent<Rigidbody>();
            if (rb != null && !suckedObjects.Contains(rb))
            {
                suckedObjects.Add(rb);
                rb.constraints = RigidbodyConstraints.None; // Unfreeze position and rotation constraints
            }
        }

        // Apply suck force to all sucked objects
        foreach (var rb in suckedObjects)
        {
            if (rb != null)
            {
                // Calculate direction from the object's center to the sucker's position
                Vector3 direction = (transform.position - rb.GetComponent<Collider>().bounds.center).normalized;
                rb.AddForce(direction * suckForce, ForceMode.Acceleration);

                // Disable the object if it's close enough to the sucker
                if (Vector3.Distance(rb.GetComponent<Collider>().bounds.center, transform.position) < minDistanceToDisable)
                {
                    DisableObject(rb.gameObject);
                }
            }
        }
    }

    // Disable an object and refill the meter
    void DisableObject(GameObject obj)
    {
        if (obj.activeSelf) // Ensure the object is active before disabling it
        {
            obj.SetActive(false); // Disable the object
            meterSystem.AddToMeter(meterGainPerObject); // Refill the meter
        }
    }

    // Visualize the suck radius in the editor (optional)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, suckRadius);
    }
}
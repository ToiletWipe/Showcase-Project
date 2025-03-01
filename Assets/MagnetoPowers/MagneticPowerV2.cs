using System.Collections; // Required for IEnumerator
using UnityEngine;

public class MagneticPowerV2 : MonoBehaviour
{
    public float suckRange = 5f; // Range within which objects are sucked
    public float suckForce = 10f; // Force applied to suck objects
    public float maxSphereSize = 5f; // Maximum size of the sphere
    public float growthRate = 0.1f; // How much the sphere grows per object
    public float throwForce = 20f; // Force applied to throw the sphere

    private bool isSucking = false; // Whether the player is currently sucking objects
    private GameObject sphere; // The sphere mesh
    private int bitsLayer; // Layer ID for the "Bits" layer
    private float currentSphereSize = 1f; // Current size of the sphere
    private int objectsSucked = 0; // Number of objects sucked up
    private int objectsNeededToGrow = 4; // Number of objects required to grow the sphere

    void Start()
    {
        // Get the layer ID for the "Bits" layer
        bitsLayer = LayerMask.NameToLayer("Bits");
        if (bitsLayer == -1)
        {
            Debug.LogError("Layer 'Bits' does not exist. Please create it in the Layer Manager.");
        }

        // Create the sphere mesh
        sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.localScale = Vector3.one * currentSphereSize;
        sphere.GetComponent<Renderer>().material.color = Color.blue; // Customize the sphere's appearance
        sphere.SetActive(false); // Hide the sphere initially

        // Disable collision between the sphere and the player
        Physics.IgnoreCollision(sphere.GetComponent<Collider>(), GetComponent<Collider>());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            StartSucking();
        }

        if (Input.GetKeyUp(KeyCode.G))
        {
            StopSucking();
        }

        if (isSucking)
        {
            SuckObjects();
            UpdateSphere();
        }
    }

    void StartSucking()
    {
        isSucking = true;
        sphere.SetActive(true); // Show the sphere
        sphere.transform.position = transform.position + Vector3.up * 2f; // Position the sphere above the player
    }

    void StopSucking()
    {
        isSucking = false;
        ThrowSphere();
    }

    void SuckObjects()
    {
        // Detect objects in the "Bits" layer within the suck range
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, suckRange, 1 << bitsLayer);
        foreach (var hitCollider in hitColliders)
        {
            Rigidbody rb = hitCollider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 direction = (sphere.transform.position - hitCollider.transform.position).normalized;
                rb.AddForce(direction * suckForce, ForceMode.Force);

                // If the object is close enough to the sphere, destroy it and count it
                if (Vector3.Distance(hitCollider.transform.position, sphere.transform.position) < 0.5f)
                {
                    Destroy(hitCollider.gameObject); // Destroy the object
                    objectsSucked++;

                    // Grow the sphere after sucking up the required number of objects
                    if (objectsSucked >= objectsNeededToGrow)
                    {
                        GrowSphere();
                        objectsSucked = 0; // Reset the counter
                    }
                }
            }
        }
    }

    void GrowSphere()
    {
        // Increase the sphere's size
        if (currentSphereSize < maxSphereSize)
        {
            currentSphereSize += growthRate;
            sphere.transform.localScale = Vector3.one * currentSphereSize;
        }
    }

    void UpdateSphere()
    {
        // Keep the sphere above the player
        sphere.transform.position = transform.position + Vector3.up * 2f;
    }

    void ThrowSphere()
    {
        // Add a Rigidbody to the sphere and throw it
        Rigidbody sphereRb = sphere.AddComponent<Rigidbody>();
        sphereRb.mass = currentSphereSize; // Adjust mass based on size
        sphereRb.AddForce(transform.forward * throwForce, ForceMode.Impulse);

        // Reset the sphere after throwing
        StartCoroutine(ResetSphere());
    }

    IEnumerator ResetSphere()
    {
        yield return new WaitForSeconds(5f); // Wait for 5 seconds before resetting
        Destroy(sphere); // Destroy the thrown sphere
        CreateNewSphere(); // Create a new sphere for the next use
    }

    void CreateNewSphere()
    {
        // Create a new sphere mesh
        sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.localScale = Vector3.one; // Reset size
        sphere.GetComponent<Renderer>().material.color = Color.blue; // Customize the sphere's appearance
        sphere.SetActive(false); // Hide the sphere initially
        currentSphereSize = 1f; // Reset size

        // Disable collision between the new sphere and the player
        Physics.IgnoreCollision(sphere.GetComponent<Collider>(), GetComponent<Collider>());
    }
}
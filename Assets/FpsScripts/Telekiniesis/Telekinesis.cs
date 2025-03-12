using UnityEngine;
using System.Collections.Generic;

public class Telekinesis : MonoBehaviour
{
    public Camera playerCamera;
    public float grabRange = 5f;
    public float throwForce = 15f;
    public Material highlightMaterial;
    public Transform[] snapPoints; // Assign empty GameObjects in Inspector
    public LayerMask grabbableLayer; // Set in Inspector
    public float pickupSmoothSpeed = 10f; // Speed of object moving to snap point

    private List<Rigidbody> heldObjects = new List<Rigidbody>();
    private Dictionary<Rigidbody, bool> objectReachedPoint = new Dictionary<Rigidbody, bool>();
    private List<Renderer> highlightedObjects = new List<Renderer>();
    private Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>(); // Track original materials
    private int maxObjects = 3;

    void Update()
    {
        // This will still handle grabbing objects and throwing them
        if (Input.GetKeyDown(KeyCode.E))
            TryGrabObject();

        if (Input.GetMouseButtonDown(1))
            ThrowAllObjects();

        HandleHighlighting(); // Keep handling highlighting for objects
    }

    void FixedUpdate()
    {
        UpdateHeldObjects();
    }

    void TryGrabObject()
    {
        if (heldObjects.Count >= maxObjects) return;

        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, grabRange, grabbableLayer))
        {
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            if (rb != null && !heldObjects.Contains(rb))
            {
                GrabObject(rb);
            }
        }
    }

    void GrabObject(Rigidbody rb)
    {
        rb.isKinematic = true; // Disable physics
        rb.interpolation = RigidbodyInterpolation.None; // Prevent jitter
        rb.detectCollisions = false; // Disable collision

        Transform snapPoint = snapPoints[heldObjects.Count];

        objectReachedPoint[rb] = false; // Object is moving towards snap point
        heldObjects.Add(rb);
    }

    void UpdateHeldObjects()
    {
        for (int i = 0; i < heldObjects.Count; i++)
        {
            Rigidbody rb = heldObjects[i];
            Transform snapPoint = snapPoints[i];

            if (!objectReachedPoint[rb])
            {
                // Move smoothly to the snap point
                rb.transform.position = Vector3.Lerp(rb.transform.position, snapPoint.position, Time.deltaTime * pickupSmoothSpeed);
                rb.transform.rotation = Quaternion.Lerp(rb.transform.rotation, snapPoint.rotation, Time.deltaTime * pickupSmoothSpeed);

                // Check if close enough to lock
                if (Vector3.Distance(rb.transform.position, snapPoint.position) < 0.01f)
                {
                    objectReachedPoint[rb] = true;
                    rb.transform.SetParent(snapPoint, true); // Parent to snap point
                    rb.transform.localPosition = Vector3.zero;
                    rb.transform.localRotation = Quaternion.identity;
                }
            }
        }
    }

    void ThrowAllObjects()
    {
        foreach (Rigidbody rb in heldObjects)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
            rb.transform.SetParent(null);

            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // Default throw direction (straight forward)
            Vector3 throwDirection = playerCamera.transform.forward;

            // Perform a raycast to determine the exact point the player is looking at
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, 100f))
            {
                throwDirection = (hit.point - rb.transform.position).normalized; // Aim directly at the hit point
            }

            // Apply force to throw the object
            rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);

            // Add damage script to thrown object to deal damage and destroy it
            rb.gameObject.AddComponent<ThrownObjectDamage>(); // Add the damage script to handle destruction
        }

        heldObjects.Clear();
        objectReachedPoint.Clear();
    }

    // Handling the highlighting logic for the grabbable objects
    void HandleHighlighting()
    {
        // Reset material for previously highlighted objects
        foreach (Renderer rend in highlightedObjects)
        {
            if (rend != null)
            {
                // Revert to the original material if it was highlighted previously
                if (originalMaterials.ContainsKey(rend))
                {
                    rend.material = originalMaterials[rend];
                }
            }
        }
        highlightedObjects.Clear();

        // Raycast to detect grabbable objects
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, grabRange, grabbableLayer))
        {
            Renderer rend = hit.collider.GetComponent<Renderer>();
            if (rend != null)
            {
                // Store the original material if it's the first time highlighting this object
                if (!originalMaterials.ContainsKey(rend))
                {
                    originalMaterials[rend] = rend.material;
                }

                rend.material = highlightMaterial; // Highlight the object
                highlightedObjects.Add(rend);
            }
        }
    }
}

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
    public float holdSmoothSpeed = 10f; // Speed at which objects follow the snap points

    private List<Rigidbody> heldObjects = new List<Rigidbody>();
    private List<Collider> heldColliders = new List<Collider>();
    private List<Material> originalMaterials = new List<Material>();
    private List<Renderer> highlightedObjects = new List<Renderer>();
    private int maxObjects = 3;
    private Material originalMaterial;

    void Update()
    {
        HandleHighlighting();
        UpdateHeldObjects(); // Ensures objects move with the camera

        if (Input.GetKeyDown(KeyCode.E))
            TryGrabObject();

        if (Input.GetMouseButtonDown(1))
            ThrowAllObjects();
    }

    void HandleHighlighting()
    {
        foreach (Renderer rend in highlightedObjects)
            rend.material = originalMaterial;
        highlightedObjects.Clear();

        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, grabRange, grabbableLayer))
        {
            Renderer rend = hit.collider.GetComponent<Renderer>();
            if (rend != null)
            {
                originalMaterial = rend.material;
                rend.material = highlightMaterial;
                highlightedObjects.Add(rend);
            }
        }
    }

    void TryGrabObject()
    {
        if (heldObjects.Count >= maxObjects) return;

        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, grabRange, grabbableLayer))
        {
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            Collider col = hit.collider.GetComponent<Collider>();
            if (rb != null && !heldObjects.Contains(rb))
            {
                GrabObject(rb, col);
            }
        }
    }

    void GrabObject(Rigidbody rb, Collider col)
    {
        Transform snapPoint = snapPoints[heldObjects.Count];

        // Store collider reference and disable it
        if (col != null)
        {
            col.enabled = false;
            heldColliders.Add(col);
        }

        // Freeze Rigidbody instead of making it kinematic (Keeps Interpolation)
        rb.constraints = RigidbodyConstraints.FreezeAll;

        rb.transform.SetParent(snapPoint); // Attach to snap point
        heldObjects.Add(rb);
    }

    void UpdateHeldObjects()
    {
        for (int i = 0; i < heldObjects.Count; i++)
        {
            Rigidbody rb = heldObjects[i];
            Transform snapPoint = snapPoints[i];

            if (rb != null && snapPoint != null)
            {
                // Smoothly move object to snap point (prevents jitter)
                rb.transform.position = Vector3.Lerp(rb.transform.position, snapPoint.position, Time.deltaTime * holdSmoothSpeed);
                rb.transform.rotation = Quaternion.Lerp(rb.transform.rotation, snapPoint.rotation, Time.deltaTime * holdSmoothSpeed);
            }
        }
    }

    void ThrowAllObjects()
    {
        for (int i = 0; i < heldObjects.Count; i++)
        {
            Rigidbody rb = heldObjects[i];

            if (rb != null)
            {
                rb.constraints = RigidbodyConstraints.None; // Unfreeze Rigidbody

                // Restore collider
                if (heldColliders[i] != null)
                    heldColliders[i].enabled = true;

                rb.transform.SetParent(null);
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                rb.AddForce(playerCamera.transform.forward * throwForce, ForceMode.Impulse);
            }
        }

        heldObjects.Clear();
        heldColliders.Clear();
    }
}

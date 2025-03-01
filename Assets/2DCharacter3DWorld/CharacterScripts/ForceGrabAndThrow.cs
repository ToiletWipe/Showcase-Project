using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceGrabAndThrow : MonoBehaviour
{
    [Header("Force Grab Settings")]
    public float grabRange = 5f; // Range within which objects can be grabbed
    public float grabForce = 10f; // Force applied to grab objects
    public float throwForce = 20f; // Force applied to throw objects
    public Transform holdPosition; // Position where the object will be held
    public LayerMask grabLayer; // Layer mask for objects that can be grabbed
    public Material highlightMaterial; // Material used to highlight objects

    private GameObject grabbedObject; // Reference to the currently grabbed object
    private Rigidbody grabbedObjectRb; // Rigidbody of the grabbed object
    private bool isHoldingObject = false; // Whether the player is currently holding an object

    private GameObject lastHighlightedObject; // Last object that was highlighted
    private Material[] originalMaterials; // Original materials of the highlighted object

    void Update()
    {
        // Check for grab input
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isHoldingObject)
            {
                TryGrabObject();
            }
            else
            {
                ThrowObject();
            }
        }

        // If holding an object, move it to the hold position
        if (isHoldingObject && grabbedObject != null)
        {
            grabbedObjectRb.linearVelocity = (holdPosition.position - grabbedObject.transform.position) * grabForce;
        }

        // Highlight objects within grab range
        HighlightObjectInRange();
    }

    void TryGrabObject()
    {
        // Raycast to detect objects within grab range
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, grabRange, grabLayer))
        {
            // Check if the object has a Rigidbody
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                grabbedObject = hit.collider.gameObject;
                grabbedObjectRb = rb;
                grabbedObjectRb.useGravity = false; // Disable gravity while holding
                grabbedObjectRb.linearDamping = 10; // Increase drag to make it easier to hold
                isHoldingObject = true;

                // Restore the original material of the previously highlighted object
                RestoreOriginalMaterial();
            }
        }
    }

    void ThrowObject()
    {
        if (grabbedObject != null)
        {
            // Re-enable gravity and reset drag
            grabbedObjectRb.useGravity = true;
            grabbedObjectRb.linearDamping = 1;

            // Apply throw force in the direction the player is facing
            grabbedObjectRb.AddForce(transform.forward * throwForce, ForceMode.Impulse);

            // Release the object
            grabbedObject = null;
            grabbedObjectRb = null;
            isHoldingObject = false;
        }
    }

    void HighlightObjectInRange()
    {
        // Raycast to detect objects within grab range
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, grabRange, grabLayer))
        {
            GameObject hitObject = hit.collider.gameObject;

            // If the object is not already highlighted, highlight it
            if (hitObject != lastHighlightedObject)
            {
                // Restore the original material of the previously highlighted object
                RestoreOriginalMaterial();

                // Store the original materials of the new object
                Renderer renderer = hitObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    originalMaterials = renderer.materials;
                    Material[] newMaterials = new Material[renderer.materials.Length];
                    for (int i = 0; i < newMaterials.Length; i++)
                    {
                        newMaterials[i] = highlightMaterial;
                    }
                    renderer.materials = newMaterials;
                }

                // Update the last highlighted object
                lastHighlightedObject = hitObject;
            }
        }
        else
        {
            // If no object is in range, restore the original material of the last highlighted object
            RestoreOriginalMaterial();
            lastHighlightedObject = null;
        }
    }

    void RestoreOriginalMaterial()
    {
        if (lastHighlightedObject != null)
        {
            Renderer renderer = lastHighlightedObject.GetComponent<Renderer>();
            if (renderer != null && originalMaterials != null)
            {
                renderer.materials = originalMaterials;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw a debug line to visualize the grab range
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * grabRange);
    }
}
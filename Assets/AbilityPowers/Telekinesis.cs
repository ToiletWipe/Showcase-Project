using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Telekinesis : MonoBehaviour
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
    private Collider grabbedObjectCollider; // Collider of the grabbed object
    private bool isHoldingObject = false; // Whether the player is currently holding an object

    private GameObject lastHighlightedObject; // Last object that was highlighted
    private Material[] originalMaterials; // Original materials of the highlighted object

    private Vector3 grabOffset; // Offset between the hold position and the mesh center

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

        // Highlight objects within grab range
        HighlightObjectInRange();
    }

    void FixedUpdate()
    {
        // If holding an object, move it to the hold position with interpolation
        if (isHoldingObject && grabbedObject != null)
        {
            // Calculate the target position for the object's mesh center
            Vector3 targetPosition = holdPosition.position - grabOffset;

            // Smoothly move the object to the target position
            Vector3 moveDirection = (targetPosition - GetMeshCenter(grabbedObject)) * grabForce;
            grabbedObjectRb.linearVelocity = moveDirection;
        }
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
                grabbedObjectCollider = hit.collider;

                // Calculate the offset between the hold position and the mesh center
                grabOffset = holdPosition.position - GetMeshCenter(grabbedObject);

                // Disable collider and gravity while holding
                grabbedObjectCollider.enabled = false;
                grabbedObjectRb.useGravity = false;

                // Freeze rotation and restrict movement
                grabbedObjectRb.constraints = RigidbodyConstraints.FreezeRotation;

                // Increase drag to make it easier to hold
                grabbedObjectRb.linearDamping = 5;
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
            // Re-enable collider, gravity, and remove constraints
            grabbedObjectCollider.enabled = true;
            grabbedObjectRb.useGravity = true;
            grabbedObjectRb.constraints = RigidbodyConstraints.None;

            // Enable Continuous Collision Detection (CCD)
            grabbedObjectRb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // Reset drag
            grabbedObjectRb.linearDamping = 1;

            // Apply throw force in the direction the camera is facing
            Vector3 throwDirection = transform.forward.normalized; // Normalize to ensure consistent force
            grabbedObjectRb.AddForce(throwDirection * throwForce, ForceMode.Impulse);

            // Release the object
            grabbedObject = null;
            grabbedObjectRb = null;
            grabbedObjectCollider = null;
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

    private Vector3 GetMeshCenter(GameObject obj)
    {
        // Get the Renderer component to calculate the mesh bounds
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Return the center of the mesh bounds in world space
            return renderer.bounds.center;
        }

        // Fallback to the object's position if no Renderer is found
        return obj.transform.position;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw a debug line to visualize the grab range
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * grabRange);

        // Draw the mesh center of the grabbed object
        if (isHoldingObject && grabbedObject != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(GetMeshCenter(grabbedObject), 0.1f);
        }
    }
}
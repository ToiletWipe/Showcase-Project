using UnityEngine;

public class Telekinesis : MonoBehaviour
{
    [Header("Telekinesis Settings")]
    public float grabRange = 10f; // Range within which objects can be grabbed
    public float liftHeight = 2f; // Height at which the object is lifted
    public float moveSpeed = 5f; // Speed at which the object moves
    public float throwForce = 20f; // Force applied to throw objects
    public LayerMask grabLayer; // Layer mask for objects that can be grabbed

    [Header("References")]
    public Camera playerCamera; // Reference to the player's camera
    public Transform holdPosition; // Position where the object will be held

    private GameObject grabbedObject; // Reference to the currently grabbed object
    private Rigidbody grabbedObjectRb; // Rigidbody of the grabbed object
    private bool isHoldingObject = false; // Whether the player is currently holding an object

    void Update()
    {
        // Handle grab input
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isHoldingObject)
            {
                TryGrabObject();
            }
            else
            {
                ReleaseObject();
            }
        }

        // If holding an object, move it based on mouse input
        if (isHoldingObject && grabbedObject != null)
        {
            MoveObject();
        }

        // Throw the object when left-clicking
        if (isHoldingObject && Input.GetMouseButtonDown(0)) // Left mouse button
        {
            ThrowObject();
        }
    }

    void TryGrabObject()
    {
        // Raycast to detect objects within grab range
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, grabRange, grabLayer))
        {
            // Check if the object has a Rigidbody
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                grabbedObject = hit.collider.gameObject;
                grabbedObjectRb = rb;
                grabbedObjectRb.useGravity = false; // Disable gravity while holding
                grabbedObjectRb.linearDamping = 10; // Increase damping to make it easier to control
                isHoldingObject = true;
            }
        }
    }

    void MoveObject()
    {
        // Get the mouse movement
        float mouseX = Input.GetAxis("Mouse X") * moveSpeed * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * moveSpeed * Time.deltaTime;

        // Calculate the new position based on mouse input
        Vector3 newPosition = holdPosition.position + playerCamera.transform.right * mouseX + playerCamera.transform.up * mouseY;

        // Move the object smoothly to the new position
        grabbedObjectRb.velocity = (newPosition - grabbedObject.transform.position) * moveSpeed;
    }

    void ThrowObject()
    {
        if (grabbedObject != null)
        {
            // Re-enable gravity and reset damping
            grabbedObjectRb.useGravity = true;
            grabbedObjectRb.linearDamping = 1;

            // Calculate throw direction based on camera forward
            Vector3 throwDirection = playerCamera.transform.forward;

            // Apply throw force in the calculated direction
            grabbedObjectRb.AddForce(throwDirection * throwForce, ForceMode.Impulse);

            // Release the object
            grabbedObject = null;
            grabbedObjectRb = null;
            isHoldingObject = false;
        }
    }

    void ReleaseObject()
    {
        if (grabbedObject != null)
        {
            // Re-enable gravity and reset damping
            grabbedObjectRb.useGravity = true;
            grabbedObjectRb.linearDamping = 1;

            // Release the object
            grabbedObject = null;
            grabbedObjectRb = null;
            isHoldingObject = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw a debug line to visualize the grab range
        Gizmos.color = Color.green;
        Gizmos.DrawLine(playerCamera.transform.position, playerCamera.transform.position + playerCamera.transform.forward * grabRange);
    }
}
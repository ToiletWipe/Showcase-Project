using UnityEngine;
using System.Collections;
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

    // UI References
    public GameObject leftHandImage; // Assign the left hand image in the Inspector
    public GameObject rightHandImage; // Assign the right hand image in the Inspector
    public GameObject[] weaponCanvases; // Assign all weapon canvases in the Inspector
    private int activeWeaponIndex = 0; // Tracks the currently active weapon

    private List<Rigidbody> heldObjects = new List<Rigidbody>();
    private Dictionary<Rigidbody, bool> objectReachedPoint = new Dictionary<Rigidbody, bool>();
    private List<Renderer> highlightedObjects = new List<Renderer>();
    private Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>(); // Track original materials
    private int maxObjects = 3;

    private bool useLeftHandNext = true; // Flag to alternate hands when picking up

    void Start()
    {
        // Initialize the active weapon canvas
        SetActiveWeaponCanvas(activeWeaponIndex);

        // Hide hands at the start
        leftHandImage.SetActive(false);
        rightHandImage.SetActive(false);
    }

    void Update()
    {
        // Handle weapon switching
        if (Input.GetKeyDown(KeyCode.Alpha1)) // Switch to weapon 1
        {
            SwitchWeapon(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) // Switch to weapon 2
        {
            SwitchWeapon(1);
        }
        // Add more keys for additional weapons if needed

        // Handle grabbing and throwing
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

        // Show one hand briefly (alternating) and hide the active weapon canvas
        if (useLeftHandNext)
        {
            StartCoroutine(ShowHandBriefly(leftHandImage, weaponCanvases[activeWeaponIndex]));
        }
        else
        {
            StartCoroutine(ShowHandBriefly(rightHandImage, weaponCanvases[activeWeaponIndex]));
        }

        // Toggle the hand flag for the next pickup
        useLeftHandNext = !useLeftHandNext;
    }

    void UpdateHeldObjects()
    {
        // Iterate through held objects in reverse to safely remove destroyed objects
        for (int i = heldObjects.Count - 1; i >= 0; i--)
        {
            Rigidbody rb = heldObjects[i];

            // Check if the object has been destroyed
            if (rb == null)
            {
                // Remove the destroyed object from the list and dictionary
                heldObjects.RemoveAt(i);
                objectReachedPoint.Remove(rb);
                continue;
            }

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
        // Check if there are any held objects
        if (heldObjects.Count == 0)
        {
            return; // Exit if there are no objects to throw
        }

        // Show both hands briefly and hide the active weapon canvas
        StartCoroutine(ShowBothHandsBriefly(leftHandImage, rightHandImage, weaponCanvases[activeWeaponIndex]));

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
            ThrownObjectDamage thrownObjectDamage = rb.gameObject.AddComponent<ThrownObjectDamage>();
            thrownObjectDamage.SetThrown(true); // Mark the object as thrown
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

    // Coroutine to show one hand briefly and hide the active weapon canvas
    private IEnumerator ShowHandBriefly(GameObject hand, GameObject weaponCanvas)
    {
        // Hide the active weapon canvas
        weaponCanvas.SetActive(false);

        // Show the hand
        hand.SetActive(true);

        // Wait for a brief period (e.g., 0.5 seconds)
        yield return new WaitForSeconds(0.4f);

        // Hide the hand
        hand.SetActive(false);

        // Show the active weapon canvas again
        weaponCanvas.SetActive(true);
    }

    // Coroutine to show both hands briefly and hide the active weapon canvas
    private IEnumerator ShowBothHandsBriefly(GameObject leftHand, GameObject rightHand, GameObject weaponCanvas)
    {
        // Hide the active weapon canvas
        weaponCanvas.SetActive(false);

        // Show both hands
        leftHand.SetActive(true);
        rightHand.SetActive(true);

        // Wait for a brief period (e.g., 0.5 seconds)
        yield return new WaitForSeconds(0.5f);

        // Hide both hands
        leftHand.SetActive(false);
        rightHand.SetActive(false);

        // Show the active weapon canvas again
        weaponCanvas.SetActive(true);
    }

    // Switch between weapons
    private void SwitchWeapon(int newWeaponIndex)
    {
        if (newWeaponIndex >= 0 && newWeaponIndex < weaponCanvases.Length)
        {
            // Hide the current weapon canvas
            weaponCanvases[activeWeaponIndex].SetActive(false);

            // Update the active weapon index
            activeWeaponIndex = newWeaponIndex;

            // Show the new weapon canvas
            weaponCanvases[activeWeaponIndex].SetActive(true);
        }
    }

    // Set the active weapon canvas
    private void SetActiveWeaponCanvas(int index)
    {
        for (int i = 0; i < weaponCanvases.Length; i++)
        {
            weaponCanvases[i].SetActive(i == index);
        }
    }
}
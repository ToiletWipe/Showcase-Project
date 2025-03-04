using UnityEngine;

public class BlackHoleDisableHandler : MonoBehaviour
{
    public BlackHoleControllerV2 controller; // Reference to the main controller

    void OnTriggerEnter(Collider other)
    {
        // Check if the object is in the "Bits" layer
        if (other.gameObject.layer == LayerMask.NameToLayer("Bits"))
        {
            // Disable the object and grow the black hole
            controller.DisableObject(other.gameObject);
        }
    }
}
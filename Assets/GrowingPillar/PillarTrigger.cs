using UnityEngine;

public class PillarTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    public GrowingPillar pillar; // Reference to the pillar script

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Ensure the player has the tag "Player"
        {
            // Start scaling the pillar
            pillar.StartScaling();
        }
    }
}
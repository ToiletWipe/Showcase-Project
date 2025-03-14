using UnityEngine;

public class PillarTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    public GrowingPillar[] pillars; // Array of pillar scripts

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Ensure the player has the tag "Player"
        {
            // Start scaling all pillars
            foreach (var pillar in pillars)
            {
                pillar.StartScaling();
            }
        }
    }
}
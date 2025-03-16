using UnityEngine;

public class RunParticleEffect : MonoBehaviour
{
    public ParticleSystem hyperDriveEffect; // Reference to your particle effect

    void Update()
    {
        // Check if Left Shift is pressed
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            hyperDriveEffect.Play(); // Play the particle effect
        }

        // Check if Left Shift is released
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            hyperDriveEffect.Stop(); // Stop the particle effect
        }
    }
}
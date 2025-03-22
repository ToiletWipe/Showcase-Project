using UnityEngine;

public class SlowMotion : MonoBehaviour
{
    public float slowMotionTimeScale = 0.5f; // The time scale for slow motion (e.g., 0.5 for half speed)
    private bool isSlowMotion = false; // Track whether slow motion is active

    void Update()
    {
        // Check if the "C" key is pressed
        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleSlowMotion();
        }
    }

    void ToggleSlowMotion()
    {
        // Toggle slow motion on/off
        isSlowMotion = !isSlowMotion;

        if (isSlowMotion)
        {
            // Enable slow motion
            Time.timeScale = slowMotionTimeScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale; // Adjust fixedDeltaTime for physics
        }
        else
        {
            // Disable slow motion and return to normal speed
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f; // Reset fixedDeltaTime to default
        }
    }
}
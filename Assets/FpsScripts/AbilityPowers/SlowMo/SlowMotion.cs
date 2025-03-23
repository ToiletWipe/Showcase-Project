using UnityEngine;

public class SlowMotion : MonoBehaviour
{
    [Header("Slow Motion Settings")]
    public float slowMotionTimeScale = 0.5f; // The time scale for slow motion (e.g., 0.5 for half speed)
    public float meterDepletionRate = 5f; // Amount of meter depleted per second

    [Header("Meter System")]
    public MeterSystem meterSystem; // Reference to the MeterSystem

    private bool isSlowMotion = false; // Track whether slow motion is active

    void Update()
    {
        // Check if the "C" key is pressed
        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleSlowMotion();
        }

        // Deplete meter while slow motion is active
        if (isSlowMotion)
        {
            DepleteMeter();
        }
    }

    void ToggleSlowMotion()
    {
        // Check if there's enough meter to activate slow motion
        if (!isSlowMotion && !meterSystem.HasEnoughMeter(meterDepletionRate * Time.deltaTime))
        {
            Debug.Log("Not enough meter to activate slow motion!");
            return;
        }

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

    void DepleteMeter()
    {
        // Deduct meter over time
        if (meterSystem.DeductFromMeter(meterDepletionRate * Time.deltaTime))
        {
            Debug.Log($"Meter depleted by {meterDepletionRate * Time.deltaTime}. Current Meter: {meterSystem.currentMeter}");
        }
        else
        {
            // If the meter runs out, disable slow motion
            Debug.Log("Meter depleted. Disabling slow motion.");
            ToggleSlowMotion();
        }
    }
}
using UnityEngine;
using UnityEngine.UI; // Required for UI elements

public class MeterSystem : MonoBehaviour
{
    [Header("Meter Settings")]
    public float maxMeter = 100f; // Maximum meter capacity
    public float currentMeter = 0f; // Current meter value

    [Header("UI Settings")]
    public Slider meterSlider; // Reference to the UI Slider

    private void Start()
    {
        // Initialize the UI Slider
        if (meterSlider != null)
        {
            meterSlider.maxValue = maxMeter;
            meterSlider.value = currentMeter;
        }
    }

    // Add to the meter
    public void AddToMeter(float amount)
    {
        currentMeter = Mathf.Clamp(currentMeter + amount, 0f, maxMeter);
        UpdateMeterUI();
        Debug.Log($"Meter increased by {amount}. Current Meter: {currentMeter}");
    }

    // Deduct from the meter
    public bool DeductFromMeter(float amount)
    {
        if (currentMeter >= amount)
        {
            currentMeter -= amount;
            UpdateMeterUI();
            Debug.Log($"Meter decreased by {amount}. Current Meter: {currentMeter}");
            return true; // Deduction successful
        }
        else
        {
            Debug.Log("Not enough meter to perform this action.");
            return false; // Deduction failed
        }
    }

    // Check if the meter has enough for a specific amount
    public bool HasEnoughMeter(float amount)
    {
        return currentMeter >= amount;
    }

    // Update the UI Slider
    private void UpdateMeterUI()
    {
        if (meterSlider != null)
        {
            meterSlider.value = currentMeter;
        }
    }
}
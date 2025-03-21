using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    private RectTransform rectTransform; // Reference to the RectTransform of the UI element
    private Vector2 originalPosition; // The UI element's original position
    private float currentShakeDuration = 0f; // Timer for the shake duration
    private float currentShakeMagnitude; // Intensity of the current shake

    private void Start()
    {
        // Get the RectTransform component
        rectTransform = GetComponent<RectTransform>();

        // Store the UI element's original position
        if (rectTransform != null)
        {
            originalPosition = rectTransform.anchoredPosition;
        }
        else
        {
            Debug.LogError("ScreenShake script requires a RectTransform component!");
            enabled = false; // Disable the script if no RectTransform is found
        }
    }

    private void Update()
    {
        if (currentShakeDuration > 0)
        {
            // Randomize the UI element's position within a small range
            Vector2 randomOffset = Random.insideUnitCircle * currentShakeMagnitude;
            rectTransform.anchoredPosition = originalPosition + randomOffset;

            // Decrease the shake duration over time
            currentShakeDuration -= Time.deltaTime;
        }
        else
        {
            // Reset the UI element's position when the shake is over
            currentShakeDuration = 0f;
            rectTransform.anchoredPosition = originalPosition;
        }
    }

    public void TriggerShake(float duration, float magnitude)
    {
        // Start the screen shake with the specified duration and magnitude
        currentShakeDuration = duration;
        currentShakeMagnitude = magnitude;
    }
}
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider; // Reference to the UI Slider
    public Health healthScript; // Reference to the Health script

    private void Start()
    {
        if (healthScript != null)
        {
            // Set the max value of the slider to the max health
            slider.maxValue = healthScript.maxHealth;
            slider.value = healthScript.maxHealth;
        }
    }

    private void Update()
    {
        if (healthScript != null)
        {
            // Update the slider value to match the current health using the public property
            slider.value = healthScript.CurrentHealth;
        }
    }
}
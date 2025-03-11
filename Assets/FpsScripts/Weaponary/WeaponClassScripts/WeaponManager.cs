using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponManager : MonoBehaviour
{
    public List<Weapon> weapons; // List of available weapons
    public List<Image> weaponImages; // List of UI Images for each weapon
    public int currentWeaponIndex = 0; // Index of the currently selected weapon

    private void Start()
    {
        // Initialize the UI with the first weapon
        SwitchWeapon(currentWeaponIndex);
    }

    private void Update()
    {
        // Switch weapons using number keys (1, 2, 3, etc.)
        if (Input.GetKeyDown(KeyCode.Alpha1)) // Switch to weapon 1
        {
            SwitchWeapon(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) // Switch to weapon 2
        {
            SwitchWeapon(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3)) // Switch to weapon 3
        {
            SwitchWeapon(2);
        }
    }

    public void SwitchWeapon(int newIndex)
    {
        // Ensure the index is within bounds
        if (newIndex >= 0 && newIndex < weapons.Count)
        {
            currentWeaponIndex = newIndex;

            // Update the UI to show the selected weapon's image
            UpdateWeaponUI();
        }
    }

    private void UpdateWeaponUI()
    {
        // Hide all weapon images
        for (int i = 0; i < weaponImages.Count; i++)
        {
            weaponImages[i].gameObject.SetActive(false);
        }

        // Show the selected weapon's image
        weaponImages[currentWeaponIndex].gameObject.SetActive(true);
    }
}
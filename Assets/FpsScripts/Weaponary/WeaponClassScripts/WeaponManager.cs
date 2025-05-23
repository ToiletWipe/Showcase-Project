using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponManager : MonoBehaviour
{
    public List<Weapon> weapons; // List of available weapons
    public List<Image> weaponImages; // List of UI Images for each weapon
    public int currentWeaponIndex = 0; // Index of the currently selected weapon
    public TextMeshProUGUI ammoText; // Reference to the ammo counter text
    public MeterSystem meterSystem; // Reference to the MeterSystem
    public float meterCostToReload = 20f; // Meter cost to reload

    // Private backing field for reloading state
    private bool _isReloading = false;

    // Public property to access reloading state
    public bool IsReloading => _isReloading;

    private Coroutine reloadCoroutine; // Track the reload coroutine

    private void Start()
    {
        // Check if lists are assigned and not empty
        if (weapons == null || weapons.Count == 0)
        {
            Debug.LogError("Weapons list is not assigned or empty!");
            return;
        }

        if (weaponImages == null || weaponImages.Count == 0)
        {
            Debug.LogError("WeaponImages list is not assigned or empty!");
            return;
        }

        if (ammoText == null)
        {
            Debug.LogError("AmmoText is not assigned!");
            return;
        }

        // Initialize the UI with the first weapon
        SwitchWeapon(currentWeaponIndex);

        // Ensure currentAmmo is set to maxAmmo for all weapons
        foreach (Weapon weapon in weapons)
        {
            weapon.currentAmmo = weapon.maxAmmo;
        }

        UpdateAmmoUI();

        // Hide all reload UIs at the start
        HideAllReloadUIs();
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

        // Switch weapons using scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) // Scroll up
        {
            SwitchToNextWeapon();
        }
        else if (scroll < 0f) // Scroll down
        {
            SwitchToPreviousWeapon();
        }

        // Reload the current weapon
        if (Input.GetKeyDown(KeyCode.R) && !_isReloading)
        {
            TryReload();
        }
    }

    public void SwitchWeapon(int newIndex)
    {
        // Ensure the index is within bounds
        if (newIndex >= 0 && newIndex < weapons.Count)
        {
            // Stop the current reload if switching weapons
            if (_isReloading)
            {
                StopReload();
            }

            // Hide the current weapon's reload UI
            if (weapons[currentWeaponIndex].reloadUI != null)
            {
                weapons[currentWeaponIndex].reloadUI.SetActive(false);
            }

            currentWeaponIndex = newIndex;

            // Update the UI to show the selected weapon's image
            UpdateWeaponUI();
            UpdateAmmoUI();

            // Hide all reload UIs except for the current weapon
            HideAllReloadUIs();
        }
        else
        {
            Debug.LogWarning("Invalid weapon index: " + newIndex);
        }
    }

    private void SwitchToNextWeapon()
    {
        int nextIndex = currentWeaponIndex + 1;
        if (nextIndex >= weapons.Count)
        {
            nextIndex = 0; // Wrap around to the first weapon
        }
        SwitchWeapon(nextIndex);
    }

    private void SwitchToPreviousWeapon()
    {
        int previousIndex = currentWeaponIndex - 1;
        if (previousIndex < 0)
        {
            previousIndex = weapons.Count - 1; // Wrap around to the last weapon
        }
        SwitchWeapon(previousIndex);
    }

    private void UpdateWeaponUI()
    {
        // Hide all weapon images
        for (int i = 0; i < weaponImages.Count; i++)
        {
            if (weaponImages[i] != null)
            {
                weaponImages[i].gameObject.SetActive(false);
            }
        }

        // Show the selected weapon's image
        if (weaponImages[currentWeaponIndex] != null)
        {
            weaponImages[currentWeaponIndex].gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Weapon image at index " + currentWeaponIndex + " is not assigned!");
        }
    }

    // Change this method to public
    public void UpdateAmmoUI()
    {
        // Update the ammo counter text
        Weapon currentWeapon = weapons[currentWeaponIndex];
        ammoText.text = $"{currentWeapon.currentAmmo} / {currentWeapon.maxAmmo}";
    }

    private void TryReload()
    {
        Weapon currentWeapon = weapons[currentWeaponIndex];

        // Check if the current ammo is already full
        if (currentWeapon.currentAmmo >= currentWeapon.maxAmmo)
        {
            Debug.Log("Ammo is already full!");
            return;
        }

        // Check if there's enough meter to reload
        if (meterSystem.HasEnoughMeter(meterCostToReload))
        {
            // Deduct the meter cost and start the reload process
            meterSystem.DeductFromMeter(meterCostToReload);
            StartReload();
        }
        else
        {
            Debug.Log("Not enough meter to reload!");
        }
    }

private void StartReload()
    {
        if (_isReloading || weapons[currentWeaponIndex] == null)
        {
            return; // Exit if already reloading or weapon is null
        }

        _isReloading = true;

        // Hide the current weapon's image
        if (weaponImages[currentWeaponIndex] != null)
        {
            weaponImages[currentWeaponIndex].gameObject.SetActive(false);
        }

        // Show reloading UI for the current weapon
        if (weapons[currentWeaponIndex].reloadUI != null)
        {
            weapons[currentWeaponIndex].reloadUI.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Reload UI for weapon " + weapons[currentWeaponIndex].weaponName + " is not assigned!");
        }

        // Start the reload coroutine
        reloadCoroutine = StartCoroutine(Reload());
    }

    private void StopReload()
    {
        if (_isReloading && reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine); // Stop the reload coroutine
            _isReloading = false;

            // Hide reloading UI for the current weapon
            if (weapons[currentWeaponIndex].reloadUI != null)
            {
                weapons[currentWeaponIndex].reloadUI.SetActive(false);
            }

            // Show the current weapon's image
            if (weaponImages[currentWeaponIndex] != null)
            {
                weaponImages[currentWeaponIndex].gameObject.SetActive(true);
            }
        }
    }

    private IEnumerator Reload()
    {
        Weapon currentWeapon = weapons[currentWeaponIndex];

        yield return new WaitForSeconds(currentWeapon.reloadTime);

        currentWeapon.currentAmmo = currentWeapon.maxAmmo;

        // Hide reloading UI for the current weapon
        if (currentWeapon.reloadUI != null)
        {
            currentWeapon.reloadUI.SetActive(false);
        }

        // Show the current weapon's image
        if (weaponImages[currentWeaponIndex] != null)
        {
            weaponImages[currentWeaponIndex].gameObject.SetActive(true);
        }

        _isReloading = false;
        UpdateAmmoUI();
    }

    public bool CanShoot()
    {
        // Check if the player can shoot (not reloading and has ammo)
        Weapon currentWeapon = weapons[currentWeaponIndex];
        return !_isReloading && currentWeapon.currentAmmo > 0;
    }

    private void HideAllReloadUIs()
    {
        // Hide all reload UIs except for the current weapon
        for (int i = 0; i < weapons.Count; i++)
        {
            if (weapons[i].reloadUI != null)
            {
                weapons[i].reloadUI.SetActive(i == currentWeaponIndex && _isReloading);
            }
        }
    }
}
using UnityEngine;
using UnityEngine.UI;

public class ReloadUI : MonoBehaviour
{
    public WeaponManager weaponManager; // Reference to the WeaponManager
    public Image reloadImage; // Reference to the reload UI image

    private void Update()
    {
        // Check if the weapon is reloading using the public property
        if (weaponManager.IsReloading)
        {
            reloadImage.gameObject.SetActive(true);
        }
        else
        {
            reloadImage.gameObject.SetActive(false);
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

public class MagneticPowerV1 : MonoBehaviour
{
    public float suckRange = 5f; // Range within which objects are sucked
    public float suckForce = 10f; // Force applied to suck objects
    public float sphereRadius = 1f; // Initial radius of the sphere
    public float growthRate = 0.1f; // How much the sphere grows per object
    public float throwForce = 20f; // Force applied to throw the sphere

    private List<GameObject> suckedObjects = new List<GameObject>(); // List of sucked objects
    private bool isSucking = false; // Whether the player is currently sucking objects
    private GameObject sphere; // The sphere formed by sucked objects

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            StartSucking();
        }

        if (Input.GetKeyUp(KeyCode.G))
        {
            StopSucking();
        }

        if (isSucking)
        {
            SuckObjects();
            UpdateSphere();
        }
    }

    void StartSucking()
    {
        isSucking = true;
        sphere = new GameObject("MagneticSphere");
        sphere.transform.position = transform.position + transform.forward * 2f; // Position the sphere in front of the player
    }

    void StopSucking()
    {
        isSucking = false;
        ThrowSphere();
    }

    void SuckObjects()
    {
        // Get the layer mask for the "Bits" layer
        int bitsLayer = LayerMask.NameToLayer("Bits");
        if (bitsLayer == -1)
        {
            Debug.LogError("Layer 'Bits' does not exist. Please create it in the Layer Manager.");
            return;
        }

        // Detect objects in the "Bits" layer within the suck range
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, suckRange, 1 << bitsLayer);
        foreach (var hitCollider in hitColliders)
        {
            if (!suckedObjects.Contains(hitCollider.gameObject))
            {
                Rigidbody rb = hitCollider.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 direction = (sphere.transform.position - hitCollider.transform.position).normalized;
                    rb.AddForce(direction * suckForce, ForceMode.Force);

                    if (Vector3.Distance(hitCollider.transform.position, sphere.transform.position) < 0.5f)
                    {
                        suckedObjects.Add(hitCollider.gameObject);
                        hitCollider.transform.SetParent(sphere.transform);
                        sphereRadius += growthRate;
                    }
                }
            }
        }
    }

    void UpdateSphere()
    {
        // Arrange objects in a spherical formation
        for (int i = 0; i < suckedObjects.Count; i++)
        {
            float angle = i * Mathf.PI * 2 / suckedObjects.Count;
            Vector3 pos = sphere.transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * sphereRadius;
            suckedObjects[i].transform.position = Vector3.Lerp(suckedObjects[i].transform.position, pos, Time.deltaTime * 10f);
        }
    }

    void ThrowSphere()
    {
        Rigidbody sphereRb = sphere.AddComponent<Rigidbody>();
        sphereRb.mass = suckedObjects.Count * 0.5f; // Adjust mass based on number of objects
        sphereRb.AddForce(transform.forward * throwForce, ForceMode.Impulse);

        // Detach objects after throwing
        foreach (var obj in suckedObjects)
        {
            obj.transform.SetParent(null);
        }

        suckedObjects.Clear();
        Destroy(sphere, 5f); // Destroy the sphere after 5 seconds
    }
}
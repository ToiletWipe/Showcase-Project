using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Project.Scripts.Weapon
{
    public class FireGunAlt : MonoBehaviour
    {
        [SerializeField] private Transform barrelEnd;
        [SerializeField] private float force = 1000f;
        [SerializeField] private float hitRadius = 0.05f;
        private GameObject laserLine;

        private void Awake()
        {
            if (barrelEnd == null)
            {
                Debug.LogError("barrelEnd is not assigned in FireGunAlt!");
                return;
            }

            laserLine = BuildLaserLine();
        }

        private GameObject BuildLaserLine()
        {
            if (barrelEnd == null) return null; // Prevent errors

            var lineGraphic = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            DestroyImmediate(lineGraphic.GetComponent<Collider>());
            lineGraphic.transform.localScale = new Vector3(0.01f, 1000, 0.01f);
            lineGraphic.transform.localRotation = Quaternion.Euler(90, 0, 0);
            lineGraphic.transform.SetParent(barrelEnd, false);
            var renderer = lineGraphic.GetComponent<Renderer>();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            var mat = renderer.material;
            mat.color = Color.red;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.white);
            return lineGraphic;
        }

        void Update()
        {
            if (laserLine == null || barrelEnd == null) return; // Prevent errors

            if (Input.GetMouseButton(1))
            {
                FireLaser();
                laserLine.SetActive(true);
            }
            else
            {
                laserLine.SetActive(false);
            }
        }

        private void FireLaser()
        {
            if (barrelEnd == null) return; // Prevent errors

            var allHits = Physics.RaycastAll(barrelEnd.transform.position, transform.forward)
                .SelectMany(hit => Physics.OverlapSphere(hit.point, hitRadius))
                .Distinct()
                .ToList();

            foreach (var hit in allHits)
            {
                if (hit.attachedRigidbody != null) // Prevent errors
                {
                    hit.attachedRigidbody.AddForce(force * transform.forward);
                }
            }
        }
    }
}

using UnityEngine;
using System.Collections;

public class RicochetTrajectoryVisualizer : MonoBehaviour
{
    public LineRenderer trajectoryLine; // Reference to the LineRenderer for trajectory visualization
    public int maxBounces = 3; // Maximum number of bounces to visualize
    public LayerMask mask; // Layer mask for raycasting

    private Transform bulletSpawnPoint; // Spawn point for bullets
    private Weapon currentWeapon; // Reference to the current weapon
    private bool isDrawingTrajectory; // Track if the trajectory is being drawn

    private void Start()
    {
        // Ensure the LineRenderer is assigned
        if (trajectoryLine == null)
        {
            Debug.LogError("Trajectory LineRenderer is not assigned!");
            enabled = false; // Disable the script if no LineRenderer is assigned
        }

        // Hide the trajectory line at the start
        trajectoryLine.enabled = false;
    }

    public void Initialize(Transform spawnPoint, Weapon weapon)
    {
        bulletSpawnPoint = spawnPoint;
        currentWeapon = weapon;
    }

    public void DrawRicochetTrajectory()
    {
        if (trajectoryLine == null || bulletSpawnPoint == null || currentWeapon == null)
        {
            Debug.LogWarning("Trajectory visualization is not properly initialized!");
            return;
        }

        // Only draw the trajectory if it's not already being drawn
        if (!isDrawingTrajectory)
        {
            StartCoroutine(UpdateTrajectory());
        }
    }

    private IEnumerator UpdateTrajectory()
    {
        isDrawingTrajectory = true;

        while (currentWeapon != null && currentWeapon.bouncingBullets)
        {
            // Clear the previous trajectory
            trajectoryLine.positionCount = 0;

            Vector3 direction = bulletSpawnPoint.forward;
            Vector3 startPosition = bulletSpawnPoint.position;
            int bounceCount = 0;

            // Initialize the LineRenderer
            trajectoryLine.positionCount = 1;
            trajectoryLine.SetPosition(0, startPosition);

            while (bounceCount < maxBounces)
            {
                if (Physics.Raycast(startPosition, direction, out RaycastHit hit, float.MaxValue, mask))
                {
                    // Add the hit point to the LineRenderer
                    trajectoryLine.positionCount++;
                    trajectoryLine.SetPosition(trajectoryLine.positionCount - 1, hit.point);

                    // Calculate the bounce direction
                    direction = Vector3.Reflect(direction, hit.normal);
                    startPosition = hit.point;

                    bounceCount++;
                }
                else
                {
                    // Add the final point (where the bullet would go off into infinity)
                    trajectoryLine.positionCount++;
                    trajectoryLine.SetPosition(trajectoryLine.positionCount - 1, startPosition + direction * 100);
                    break;
                }
            }

            // Make the trajectory visible
            trajectoryLine.enabled = true;

            // Wait for the next frame
            yield return null;
        }

        // Hide the trajectory line when done
        trajectoryLine.enabled = false;
        isDrawingTrajectory = false;
    }

    public void StopDrawingTrajectory()
    {
        // Stop the trajectory drawing and hide the line
        StopAllCoroutines();
        trajectoryLine.enabled = false;
        isDrawingTrajectory = false;
    }
}
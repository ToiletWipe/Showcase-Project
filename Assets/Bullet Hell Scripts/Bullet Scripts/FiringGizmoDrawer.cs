using UnityEngine;

public class FiringGizmoDrawer : MonoBehaviour
{
    // Assign one of your BulletPatternBase instances in the inspector.
    public BulletPatternBase bulletPattern;

    // This should be the transform from which bullets are fired.
    public Transform firePoint;

    void OnDrawGizmos()
    {
        if (bulletPattern != null && firePoint != null)
        {
            bulletPattern.DrawFiringGizmo(firePoint);
        }
    }
}

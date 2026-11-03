
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.3f;

    private Vector3 velocity = Vector3.zero;
    private Vector3 offset;

    private void Awake()
    {
        transform.parent = null;

        if (target != null)
        {
            offset = transform.position - target.position;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Keep the original camera offset relative to its rotation
        Vector3 rotatedOffset = Quaternion.Euler(transform.eulerAngles) * offset;
        Vector3 targetPosition = target.position + rotatedOffset;

        transform.position = Vector3.SmoothDamp(transform.position,targetPosition,ref velocity,smoothTime);
    }
}

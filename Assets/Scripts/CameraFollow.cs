using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.3f;
    private Vector3 velocity = Vector3.zero;

    private void Awake()
    {
        transform.parent = null;
    }

    private void LateUpdate()
    {
        if(target != null)
        {
            Vector3 targetPosition = target.position;

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime); 
        }
    }
}

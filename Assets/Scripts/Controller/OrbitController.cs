using UnityEngine;

/// <summary>
/// Controls an object's orbit around a target, and allows the user to zoom in and out.
/// </summary>
/// <author>Stuart Brown</author>
public class OrbitController : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float zoomSpeed = 100f;
    [SerializeField] private float minDistance = 1000f;
    [SerializeField] private float maxDistance = 6000f;

    private float currentDistance;

    private void Start()
    {
        // Set the current distance to the initial distance between the camera and the target object.
        currentDistance = Vector3.Distance(transform.position, target.transform.position);
    }

    private void Update()
    {
        // Rotate the camera around the target object based on mouse input.
        if (Input.GetMouseButton(1))
        {
            transform.RotateAround(target.transform.position, transform.up, Input.GetAxis("Mouse X") * -speed);
            transform.RotateAround(target.transform.position, transform.right, Input.GetAxis("Mouse Y") * speed);
        }

        // Zoom the camera in or out based on mouse scroll input.
        currentDistance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
        transform.position = target.transform.position - transform.forward * currentDistance;
    }
}
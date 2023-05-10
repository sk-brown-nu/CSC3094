using UnityEngine;

/// <summary>
/// Controls the free-roaming movement of a spaceship.
/// </summary>
/// <author>Stuart Brown</author>
/// <remarks>
/// This class is modified from tutorial by gamesplusjames
/// found at https://www.youtube.com/watch?v=J6QR4KzNeJU.
/// </remarks>
public class FreeRoamController : MonoBehaviour
{
    [SerializeField]
    private float forwardSpeed = 100f, stafeSpeed = 100f, hoverSpeed = 100f;
    private float activeForwardSpeed, activeStafeSpeed, activeHoverSpeed;
    private readonly float forwardAcceleration = 2.5f, strafeAcceleration = 2f, hoverAcceleration = 2f;

    [SerializeField]
    private float lookRateSpeed = 100f;
    private Vector2 lookInput, screenCentre, mouseDistance;

    private float rollInput;
    [SerializeField]
    private float rollSpeed = 100f, rollAcceleration = 5f;

    private void Start()
    {
        // Set the screen center to be the middle of the screen
        screenCentre.x = Screen.width * .5f;
        screenCentre.y = Screen.height * .5f;

        // Confine the cursor to the game window and hide it
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Calculate the mouse distance from the center of the screen
        lookInput.x = Input.mousePosition.x;
        lookInput.y = Input.mousePosition.y;

        mouseDistance.x = (lookInput.x - screenCentre.x) / screenCentre.y;
        mouseDistance.y = (lookInput.y - screenCentre.y) / screenCentre.y;

        // Clamp the mouse distance magnitude to 1
        mouseDistance = Vector2.ClampMagnitude(mouseDistance, 1f);

        // Get the roll input from the user and lerp it
        rollInput = Mathf.Lerp(rollInput, Input.GetAxisRaw("Roll"), rollAcceleration * Time.deltaTime);

        // Rotate the spaceship based on the mouse and roll input
        transform.Rotate(-mouseDistance.y * lookRateSpeed * Time.deltaTime, mouseDistance.x * lookRateSpeed * Time.deltaTime, rollInput * rollSpeed * Time.deltaTime, Space.Self);

        // Get the movement input from the user and lerp it
        activeForwardSpeed = Mathf.Lerp(activeForwardSpeed, Input.GetAxisRaw("Vertical") * forwardSpeed, forwardAcceleration * Time.deltaTime);
        activeStafeSpeed = Mathf.Lerp(activeStafeSpeed, Input.GetAxisRaw("Horizontal") * stafeSpeed, strafeAcceleration * Time.deltaTime);
        activeHoverSpeed = Mathf.Lerp(activeHoverSpeed, Input.GetAxisRaw("Hover") * hoverSpeed, hoverAcceleration * Time.deltaTime);

        // Move the spaceship forward, left/right, and up/down based on the movement input
        transform.position += transform.forward * activeForwardSpeed * Time.deltaTime;
        transform.position += (transform.right * activeStafeSpeed * Time.deltaTime) + (transform.up * activeHoverSpeed * Time.deltaTime);
    }
}

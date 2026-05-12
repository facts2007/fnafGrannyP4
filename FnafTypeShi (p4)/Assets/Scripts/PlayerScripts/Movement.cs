using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleFPSController : MonoBehaviour
{
    public Transform cam;
    public float speed = 5f;
    public float mouseSensitivity = 200f;
    public float gravity = -9.81f;
    public float groundCheckDistance = 1.1f;

    [Header("Crouch Settings")]
    public float crouchSpeedMultiplier = 0.5f;
    public float crouchHeight = 1f;
    public float standingHeight = 2f;
    public float crouchTransitionSpeed = 8f;

    CharacterController controller;
    float xRotation = 0f;
    float verticalVelocity = 0f;
    bool isCrouching = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // --- MOUSE LOOK ---
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        if (cam) cam.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // --- CROUCH TOGGLE ---
        if (Input.GetKeyDown(KeyCode.C))
        {
            isCrouching = !isCrouching;
        }

        // Smooth crouch height transition
        float targetHeight = isCrouching ? crouchHeight : standingHeight;
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);

        // Adjust camera
        if (cam)
        {
            float camTargetY = controller.height - 0.2f;
            Vector3 camPos = cam.localPosition;
            cam.localPosition = new Vector3(camPos.x, Mathf.Lerp(camPos.y, camTargetY, Time.deltaTime * crouchTransitionSpeed), camPos.z);
        }

        // --- INPUT ---
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 desiredMove = transform.right * x + transform.forward * z;
        desiredMove = desiredMove.normalized;

        // --- GROUND NORMAL ---
        RaycastHit hit;
        Vector3 groundNormal = Vector3.up;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, groundCheckDistance))
            groundNormal = hit.normal;

        Vector3 moveAlongSlope = Vector3.ProjectOnPlane(desiredMove, groundNormal).normalized;

        // --- GRAVITY ---
        if (controller.isGrounded)
            verticalVelocity = -0.5f;
        else
            verticalVelocity += gravity * Time.deltaTime;

        // --- MOVEMENT ---
        float currentSpeed = isCrouching ? speed * crouchSpeedMultiplier : speed;
        Vector3 finalMove = (moveAlongSlope * currentSpeed) + new Vector3(0f, verticalVelocity, 0f);
        controller.Move(finalMove * Time.deltaTime);
    }
}

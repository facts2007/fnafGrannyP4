using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public Transform cameraTransform;
    public float gravity = -9.81f;

    [Header("Crouch Settings")]
    public float crouchSpeed = 2.5f;
    public float crouchHeight = 1f;
    public float standHeight = 2f;
    public float crouchTransitionSpeed = 8f;
    public Vector3 crouchCameraOffset = new Vector3(0, -0.5f, 0);

    CharacterController controller;
    float verticalVelocity = 0f;
    bool isCrouching = false;
    Vector3 standingCameraLocalPos;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        standingCameraLocalPos = cameraTransform.localPosition;
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 move = forward * moveZ + right * moveX;
        move = move.normalized;

        if (controller.isGrounded)
            verticalVelocity = -0.5f;
        else
            verticalVelocity += gravity * Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.C))
            isCrouching = true;
        if (Input.GetKeyUp(KeyCode.C))
            isCrouching = false;

        float targetHeight = isCrouching ? crouchHeight : standHeight;
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);

        Vector3 targetCameraPos = isCrouching
            ? standingCameraLocalPos + crouchCameraOffset
            : standingCameraLocalPos;
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, targetCameraPos, Time.deltaTime * crouchTransitionSpeed);

        float currentSpeed = isCrouching ? crouchSpeed : speed;
        Vector3 finalMove = move * currentSpeed + Vector3.up * verticalVelocity;
        controller.Move(finalMove * Time.deltaTime);
    }
}
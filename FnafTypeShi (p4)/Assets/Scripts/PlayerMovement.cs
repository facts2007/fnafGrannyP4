using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public Transform cameraTransform;

    void Start()
{
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
}
    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        // Camera forward/right pakken maar Y eruit halen (belangrijk)
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 move = forward * moveZ + right * moveX;

        transform.Translate(move * speed * Time.deltaTime, Space.World);
    }
}
using UnityEngine;
using Photon.Pun; // Add this

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviourPun
{
    [Header("Movement Settings")]
    public float moveSpeed = 2.5f;
    public float gravity = -9.81f;

    [Header("References")]
    public Transform cameraTransform;

    [HideInInspector]
    public bool isMovementLocked = false;

    private CharacterController controller;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        if (!photonView.IsMine) return; // 🔥 Add this line!

        if (isMovementLocked) return;

        HandleMovement();
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 move = (forward * vertical + right * horizontal);
        controller.Move(move * moveSpeed * Time.deltaTime);

        if (!controller.isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
        else
        {
            velocity.y = -1f;
        }
    }
}

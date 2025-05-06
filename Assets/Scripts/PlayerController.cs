using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PhotonView))]
public class PlayerController : MonoBehaviourPun, IPunObservable
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;

    [Header("References")]
    public Transform cameraTransform;

    [HideInInspector]
    public bool isMovementLocked = false;

    private CharacterController controller;
    private Vector3 velocity;

    private Vector3 networkPosition;
    private Quaternion networkRotation;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        PlayerInput input = GetComponent<PlayerInput>();

        if (photonView.IsMine)
        {
            gameObject.tag = "Player";

            // Assign camera
            if (cameraTransform == null)
            {
                Camera cam = Camera.main;
                if (cam != null)
                    cameraTransform = cam.transform;
            }

            if (input != null)
            {
                input.enabled = true;

                // Lock InputUser to this player
                if (input.user != null)
                {
                    if (Keyboard.current != null)
                        InputUser.PerformPairingWithDevice(Keyboard.current, input.user);

                    if (Gamepad.current != null)
                        InputUser.PerformPairingWithDevice(Gamepad.current, input.user);
                }

            }
        }
        else
        {
            if (input != null)
                input.enabled = false;

            Transform rigRoot = transform.Find("XRCardboardRig");
            if (rigRoot != null)
                rigRoot.gameObject.SetActive(false);

            this.enabled = false;
        }
    }
    void Update()
    {
        if (!photonView.IsMine || isMovementLocked)
            return;

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

        Vector3 move = forward * vertical + right * horizontal;
        controller.Move(move * moveSpeed * Time.deltaTime);

        if (!controller.isGrounded)
            velocity.y += gravity * Time.deltaTime;
        else
            velocity.y = -1f;

        controller.Move(velocity * Time.deltaTime);
    }

    // Photon network sync
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}

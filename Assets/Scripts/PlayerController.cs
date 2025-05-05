using UnityEngine;
using Photon.Pun;

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

        if (photonView.IsMine)
        {
            gameObject.tag = "Player"; // Used for teleport logic, etc.

            // Auto-assign camera if not set
            if (cameraTransform == null)
            {
                Camera cam = Camera.main;
                if (cam != null)
                {
                    cameraTransform = cam.transform;
                }
            }
        }
        else
        {
            // Disable remote player's rig visuals
            Transform rigRoot = transform.Find("XRCardboardRig");
            if (rigRoot != null)
                rigRoot.gameObject.SetActive(false);

            // Optional: disable script completely for remote players
            this.enabled = false;
        }
    }

    void Update()
    {
        if (!photonView.IsMine || isMovementLocked) return;

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
        {
            velocity.y += gravity * Time.deltaTime;
        }
        else
        {
            velocity.y = -1f;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    // Sync position/rotation across network
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

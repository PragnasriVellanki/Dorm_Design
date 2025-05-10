using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Collider))]
public class TeleportDoor : MonoBehaviour
{
    [Header("Teleport Settings")]
    public Transform teleportTarget;
    public float verticalOffset = 0f;
    public bool isLocked = false;

    [Header("Highlighting")]
    public Renderer doorRenderer;
    public Color hoverColor = Color.green;
    private Color originalColor;

    [Header("Popup UI")]
    public GameObject popupCanvas;         // World-space UI canvas
    public TextMeshProUGUI popupText;
    public string lockedMessage = "🚫 This door is locked.";

    private bool isHovered = false;

    void Start()
    {
        if (doorRenderer != null)
        {
            originalColor = doorRenderer.material.color;
        }

        if (popupCanvas != null)
        {
            popupCanvas.SetActive(false);
        }
    }

    void Update()
    {
        // Only local player can trigger teleport
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom) return;

        if (isHovered && IsLocalPlayerLookingAtMe())
        {
            if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(KeyCode.JoystickButton5))
            {
                if (isLocked)
                {
                    ShowLockedPopup();
                }
                else
                {
                    TeleportLocalPlayer();
                }
            }
        }
    }

    public void SetHover(bool state)
    {
        isHovered = state;

        if (doorRenderer != null)
        {
            doorRenderer.material.color = state ? hoverColor : originalColor;
        }
    }

    void TeleportLocalPlayer()
    {
        // Find the local player only
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject localPlayer = null;

        foreach (GameObject player in players)
        {
            PhotonView pv = player.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                localPlayer = player;
                break;
            }
        }

        if (localPlayer == null)
        {
            Debug.LogWarning("Could not find local player to teleport.");
            return;
        }

        if (teleportTarget == null)
        {
            Debug.LogWarning("Teleport target not assigned!");
            return;
        }

        CharacterController cc = localPlayer.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        Vector3 targetPos = teleportTarget.position;
        targetPos.y += verticalOffset;
        localPlayer.transform.position = targetPos;

        if (cc != null) cc.enabled = true;

        Debug.Log("Teleported local player to: " + teleportTarget.name);
    }

    void ShowLockedPopup()
    {
        if (popupCanvas != null && popupText != null)
        {
            popupText.text = lockedMessage;
            popupCanvas.SetActive(true);
            CancelInvoke(nameof(HidePopup));
            Invoke(nameof(HidePopup), 2.5f);
        }
    }

    void HidePopup()
    {
        if (popupCanvas != null)
            popupCanvas.SetActive(false);
    }

    bool IsLocalPlayerLookingAtMe()
    {
        // Find the local player's camera
        Camera[] allCams = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera cam in allCams)
        {
            if (cam.CompareTag("PlayerCamera") && cam.gameObject.activeInHierarchy)
            {
                Ray ray = new Ray(cam.transform.position, cam.transform.forward);
                if (Physics.Raycast(ray, out RaycastHit hit, 10f))
                {
                    if (hit.collider != null && hit.collider.gameObject == gameObject)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
}

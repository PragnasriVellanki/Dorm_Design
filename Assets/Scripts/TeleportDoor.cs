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
        if (isHovered && (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(KeyCode.JoystickButton2)))
        {
            if (isLocked)
            {
                ShowLockedPopup();
            }
            else
            {
                TeleportPlayer();
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

    void TeleportPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj == null)
        {
            Debug.LogWarning("❌ Could not find player with tag 'Player'.");
            return;
        }

        if (teleportTarget == null)
        {
            Debug.LogWarning("⚠️ Teleport target not assigned!");
            return;
        }

        CharacterController cc = playerObj.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        Vector3 targetPos = teleportTarget.position;
        targetPos.y += verticalOffset;
        playerObj.transform.position = targetPos;

        if (cc != null) cc.enabled = true;

        Debug.Log("🟢 Teleported player to: " + teleportTarget.name);
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
}

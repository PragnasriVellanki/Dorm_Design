using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AdvancedInventoryUIController : MonoBehaviour
{
    [Header("Canvas & Category Buttons")]
    public GameObject inventoryCanvas;                 // ✅ Drag here
    public Button livingRoomButton;
    public Button bedRoomButton;
    public Button bathRoomButton;

    [Header("Category Manager")]
    public AdvancedInventoryManager categoryManager;

    private Camera mainCamera;
    private bool inventoryOpen = false;
    private bool inputReady = true;

    private PlayerController playerController;

    void Start()
    {
        Debug.Log("📦 Inventory UI Controller started.");

        // Find the InventoryCanvas if not assigned
        if (inventoryCanvas == null)
        {
            inventoryCanvas = GameObject.Find("InventoryCanvas");
            if (inventoryCanvas == null)
            {
                Debug.LogError("❌ InventoryCanvas not found!");
                return;
            }
        }

        // Find the Main Camera from the spawned player
        if (mainCamera == null)
        {
            GameObject camObj = GameObject.Find("Main Camera");
            if (camObj != null)
            {
                mainCamera = camObj.GetComponent<Camera>();
                Debug.Log("✅ Main Camera assigned dynamically.");
            }
            else
            {
                Debug.LogError("❌ Main Camera not found!");
            }
        }

        // Assign Event Camera to Canvas
        Canvas canvas = inventoryCanvas.GetComponent<Canvas>();
        if (canvas != null && mainCamera != null)
        {
            canvas.worldCamera = mainCamera;
            Debug.Log("🎥 Event camera set to Main Camera.");
        }

        inventoryCanvas.SetActive(false);
    }


    void Update()
    {
        if ((Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.JoystickButton7)) && inputReady)
        {
            Debug.Log("🎮 Toggle Inventory Triggered.");
            ToggleInventory();
            inputReady = false;
            Invoke(nameof(ResetInput), 0.25f);
        }
    }

    void ToggleInventory()
    {
        inventoryOpen = !inventoryOpen;
        Debug.Log("📂 Inventory Open: " + inventoryOpen);

        if (inventoryCanvas != null)
        {
            inventoryCanvas.SetActive(inventoryOpen);

            if (inventoryOpen && mainCamera != null)
            {
                // ✅ Position the canvas in front of the player
                Vector3 forward = mainCamera.transform.forward;
                inventoryCanvas.transform.position = mainCamera.transform.position + forward * 15f;
                inventoryCanvas.transform.rotation = Quaternion.LookRotation(forward);
            }

            if (inventoryOpen && livingRoomButton != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(livingRoomButton.gameObject);
                Debug.Log("✅ Living Room Button Selected.");
            }

            if (inventoryOpen)
                categoryManager?.LoadCategory("Living");

            if (playerController != null)
            {
                playerController.isMovementLocked = true;
                Debug.Log("🎮 Player movement locked: True");
            }
        }

        if (!inventoryOpen && playerController != null)
        {
            playerController.isMovementLocked = false;
            Debug.Log("🎮 Player movement locked: False");
        }

        if (!inventoryOpen)
        {
            EventSystem.current.SetSelectedGameObject(null);
            Debug.Log("🔄 Inventory closed and selection cleared.");
        }
    }

    void ResetInput()
    {
        inputReady = true;
    }
}

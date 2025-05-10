using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AdvancedInventoryUIController : MonoBehaviour
{
    [Header("Canvas & Category Buttons")]
    public GameObject inventoryCanvas;                 
    public Button livingRoomButton;
    public Button bedRoomButton;
    public Button bathRoomButton;

    [Header("Category Manager")]
    public AdvancedInventoryManager categoryManager;

    private Camera mainCamera;
    private bool inventoryOpen = false;
    private bool inputReady = true;

    private PlayerController playerController;
    public ObjectMenuSpawner objectMenuSpawner;
    void Start()
    {
        //Debug.Log("Inventory UI Controller started.");

        // Find the InventoryCanvas if not assigned
        if (inventoryCanvas == null)
        {
            inventoryCanvas = GameObject.Find("InventoryCanvas");
            if (inventoryCanvas == null)
            {
                //Debug.LogError("InventoryCanvas not found!");
                return;
            }
        }
        // Find PlayerController on the Player
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerController = playerObj.GetComponent<PlayerController>();
            
        }

        // Find the Main Camera from the spawned player
        
        if (mainCamera == null)
        {
            Camera[] allCameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach (Camera cam in allCameras)
            {
                if (cam.CompareTag("PlayerCamera") && cam.gameObject.activeInHierarchy)
                {
                    mainCamera = cam;
                    //Debug.Log("Main Camera assigned from PlayerCamera tag.");
                    break;
                }
            }

            if (mainCamera == null)
            {
                Debug.LogError("No active PlayerCamera found!");
            }
        }


        // Assign Event Camera to Canvas
        Canvas canvas = inventoryCanvas.GetComponent<Canvas>();
        if (canvas != null && mainCamera != null)
        {
            canvas.worldCamera = mainCamera;
            Debug.Log("Event camera set to Main Camera.");
        }
        livingRoomButton.onClick.AddListener(() => categoryManager.LoadLivingRoom());
        bedRoomButton.onClick.AddListener(() => categoryManager.LoadBedRoom());
        bathRoomButton.onClick.AddListener(() => categoryManager.LoadBathRoom());

        inventoryCanvas.SetActive(false);


    }


    void Update()
    {
        if ((Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.JoystickButton7)) && inputReady)
        {
            Debug.Log("Toggle Inventory Triggered.");
            ToggleInventory();
            inputReady = false;
            Invoke(nameof(ResetInput), 0.25f);
        }
    }

    void ToggleInventory()
    {
        inventoryOpen = !inventoryOpen;
        Debug.Log("Inventory Open: " + inventoryOpen);
        if (inventoryOpen) CloseObjectMenuIfOpen();
        if (inventoryCanvas != null)
        {
            inventoryCanvas.SetActive(inventoryOpen);

            if (inventoryOpen && mainCamera != null)
            {
                // Position the canvas in front of the player
                Vector3 forward = mainCamera.transform.forward;
                inventoryCanvas.transform.position = mainCamera.transform.position + forward * 4f;
                inventoryCanvas.transform.rotation = Quaternion.LookRotation(forward);
            }

            if (inventoryOpen && livingRoomButton != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(livingRoomButton.gameObject);
                Debug.Log("Living Room Button Selected.");
            }

            if (playerController != null)
            {
                playerController.isMovementLocked = true;
                Debug.Log("Player movement locked: True");
            }
        }

        if (!inventoryOpen && playerController != null)
        {
            playerController.isMovementLocked = false;
            Debug.Log("Player movement locked: False");
        }

        if (!inventoryOpen)
        {
            EventSystem.current.SetSelectedGameObject(null);
            Debug.Log("Inventory closed and selection cleared.");
        }
    }
    public void CloseInventoryExternally()
    {
        inventoryOpen = false;

        if (inventoryCanvas != null)
            inventoryCanvas.SetActive(false);

        if (playerController != null)
            playerController.isMovementLocked = false;

        EventSystem.current.SetSelectedGameObject(null);
        Debug.Log("Inventory closed externally.");
    }
    public void CloseObjectMenuIfOpen()
    {
        Debug.Log("Checking if Object Menu is open to close...");
        if (objectMenuSpawner != null && objectMenuSpawner.IsMenuOpen())
        {
            Debug.Log("Object Menu was open. Closing it.");
            objectMenuSpawner.CloseMenu();
        }
        else
        {
            Debug.Log("Object Menu is already closed.");
        }
    }
    public void SetInventoryButtons(List<Button> buttons)
    {
        if (buttons == null || buttons.Count == 0) return;

        // Setup vertical navigation between buttons
        for (int i = 0; i < buttons.Count; i++)
        {
            Navigation nav = buttons[i].navigation;
            nav.mode = Navigation.Mode.Explicit;

            if (i > 0)
                nav.selectOnUp = buttons[i - 1];
            if (i < buttons.Count - 1)
                nav.selectOnDown = buttons[i + 1];

            buttons[i].navigation = nav;
        }

        // Select the first button
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
    }

    void ResetInput()
    {
        inputReady = true;
    }
    public void LoadLivingRoom() => categoryManager?.LoadLivingRoom();
    public void LoadBedRoom() => categoryManager?.LoadBedRoom();
    public void LoadBathRoom() => categoryManager?.LoadBathRoom();


}

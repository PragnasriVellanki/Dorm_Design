using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance;

    [Header("Inventory Settings")]
    public GameObject inventoryPanel;
    public List<Button> slotButtons;
    public int inventorySize = 20;

    [Header("Cameras")]
    public Camera mainCamera;
    public Camera inventoryCamera;

    private List<GameObject> items = new List<GameObject>();
    private int inventoryIndex = 0;
    private bool inventoryOpen = false;
    private bool joystickInputReady = true;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        // ✅ Fully hide inventory panel and disable inventory camera
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
            inventoryPanel.layer = LayerMask.NameToLayer("InventoryOnly");
        }

        if (inventoryCamera != null)
        {
            inventoryCamera.enabled = false;
            inventoryCamera.cullingMask = LayerMask.GetMask("InventoryOnly");
        }

        RefreshSlots();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.JoystickButton7) || Input.GetKeyDown(KeyCode.JoystickButton4))
        {
            ToggleInventory();
        }

        if (!inventoryOpen) return;

        HandleJoystickNavigation();

        if (Input.GetKeyDown(KeyCode.JoystickButton10))
        {
            SelectCurrentItem();
        }
    }

    public void ToggleInventory()
    {
        inventoryOpen = !inventoryOpen;

        if (inventoryOpen)
        {
            inventoryPanel?.SetActive(true);
            inventoryCamera.enabled = true;
            if (mainCamera != null) mainCamera.enabled = false;

            PositionInventoryPanel();
            HighlightInventorySlot(inventoryIndex);
        }
        else
        {
            inventoryPanel?.SetActive(false);
            if (mainCamera != null) mainCamera.enabled = true;
            inventoryCamera.enabled = false;
        }

        RefreshSlots();
    }

    private void PositionInventoryPanel()
    {
        if (inventoryPanel == null || inventoryCamera == null) return;

        Transform cam = inventoryCamera.transform;

        Vector3 forwardOffset = cam.forward * 4f;
        Vector3 newPos = cam.position + forwardOffset;
        inventoryPanel.transform.position = newPos;

        Vector3 lookDir = newPos - cam.position;
        lookDir.y = 0;
        inventoryPanel.transform.rotation = Quaternion.LookRotation(lookDir);

        // ✅ Ensure inventoryPanel and children are on InventoryOnly layer
        SetLayerRecursive(inventoryPanel, LayerMask.NameToLayer("InventoryOnly"));
    }

    private void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }

    private void HandleJoystickNavigation()
    {
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        if (!joystickInputReady || (Mathf.Abs(vertical) < 0.5f && Mathf.Abs(horizontal) < 0.5f)) return;

        int cols = 5;
        int x = inventoryIndex % cols;
        int y = inventoryIndex / cols;

        if (horizontal > 0) x++;
        if (horizontal < 0) x--;
        if (vertical > 0) y--;
        if (vertical < 0) y++;

        x = Mathf.Clamp(x, 0, cols - 1);
        y = Mathf.Clamp(y, 0, (inventorySize / cols) - 1);
        inventoryIndex = Mathf.Clamp(y * cols + x, 0, inventorySize - 1);

        HighlightInventorySlot(inventoryIndex);
        joystickInputReady = false;
        Invoke(nameof(ResetJoystickInput), 0.2f);
    }

    private void ResetJoystickInput()
    {
        joystickInputReady = true;
    }

    void HighlightInventorySlot(int index)
    {
        for (int i = 0; i < slotButtons.Count; i++)
        {
            Image img = slotButtons[i].GetComponent<Image>();
            if (img != null)
                img.color = (i == index) ? Color.yellow : Color.white;
        }
    }
    public void HideInventoryPanel()
    {
        inventoryPanel.SetActive(false);

        // Reset all highlights
        for (int i = 0; i < slotButtons.Count; i++)
        {
            Image img = slotButtons[i].GetComponent<Image>();
            if (img != null)
                img.color = Color.white;
        }
    }


    public void HighlightSlotByMouse(int index)
    {
        inventoryIndex = index;
        HighlightInventorySlot(inventoryIndex);
    }

    public void RefreshSlots()
    {
        items = InventoryManager.Instance.storedObjects;

        for (int i = 0; i < slotButtons.Count; i++)
        {
            if (slotButtons[i] == null) continue; // ✅ Null safety

            ObjectIcon icon = slotButtons[i].GetComponent<ObjectIcon>();

            if (i < items.Count)
            {
                icon?.SetObject(items[i]);
                icon?.SetIndex(i);
                slotButtons[i].gameObject.SetActive(true);
            }
            else
            {
                icon?.Clear();
                slotButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void SelectCurrentItem()
    {
        if (inventoryIndex < 0 || inventoryIndex >= items.Count) return;

        GameObject selectedItem = items[inventoryIndex];

        InventoryManager.Instance.RemoveObject(inventoryIndex); // 🔁 Remove BEFORE activating

        selectedItem.SetActive(true);
        selectedItem.tag = "Selectable";
        selectedItem.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2f;

        RefreshSlots();
    }

}

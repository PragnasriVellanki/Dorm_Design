using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ObjectMenuSpawner : MonoBehaviour
{
    public GameObject objectMenu;
    public Behaviour movementComponent;
    public ReticlePointer reticlePointer;
    public float selectableDistance = 10f;
    public float verticalOffset = 0.7f; 

    public float rotationSpeed = 90f;
    public float repositionSpeed = 2f;

    public Transform cameraTransform;
    private Camera cam;
    private Transform currentTarget;
    public Transform CurrentTarget => currentTarget;

    public AdvancedInventoryManager inventoryManager;

    public enum ActionMode { None, Rotate, Reposition }
    private ActionMode currentActionMode = ActionMode.None;
    private PlayerController playerController;
    private bool menuJustOpened = false;

    void Start()
    {
        Camera[] allCameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera c in allCameras)
        {
            if (c.CompareTag("PlayerCamera") && c.gameObject.activeInHierarchy)
            {
                cam = c;
                cameraTransform = c.transform;
                break;
            }
        }

        Canvas objectMenuCanvas = objectMenu.GetComponent<Canvas>();
        if (objectMenuCanvas != null && cam != null)
            objectMenuCanvas.worldCamera = cam;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            playerController = playerObj.GetComponent<PlayerController>();

        objectMenu.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O) || Input.GetKeyDown(KeyCode.JoystickButton3))
        {
            if (objectMenu.activeSelf)
                CloseMenu();
            else
                TryOpenMenu();
        }

        if (menuJustOpened)
        {
            menuJustOpened = false;
            return;
        }
        if (objectMenu.activeSelf && Input.GetKeyDown(KeyCode.JoystickButton2))
        {
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            if (selected != null)
            {
                Button btn = selected.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.Invoke(); // Manually trigger click
                    Debug.Log($"JS2 triggered: {btn.gameObject.name}");
                }
            }
        }

        if (currentActionMode == ActionMode.Rotate && currentTarget != null)
        {
            if (Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.JoystickButton10))
            {
                float delta = rotationSpeed * Time.deltaTime;
                currentTarget.Rotate(Vector3.up * delta);
            }
        }

        if (currentActionMode == ActionMode.Reposition && currentTarget != null)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 direction = new Vector3(horizontal, 0, vertical);
            if (Input.GetKey(KeyCode.LeftArrow)) direction.x = -1;
            if (Input.GetKey(KeyCode.RightArrow)) direction.x = 1;
            if (Input.GetKey(KeyCode.UpArrow)) direction.z = 1;
            if (Input.GetKey(KeyCode.DownArrow)) direction.z = -1;

            if (direction.sqrMagnitude > 0.01f)
            {
                Vector3 move = cameraTransform.right * direction.x + cameraTransform.forward * direction.z;
                move.y = 0;
                currentTarget.position += move.normalized * repositionSpeed * Time.deltaTime;
            }
        }

        if (Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.JoystickButton5))
        {
            ExitActionMode();
        }
    }

    void TryOpenMenu()
    {
        if (cam == null) return;
        AdvancedInventoryUIController inv = FindAnyObjectByType<AdvancedInventoryUIController>();
        if (inv != null) inv.CloseInventoryExternally();

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, selectableDistance))
        {
            string tag = hit.collider.tag;
            if (tag == "LivingRoom" || tag == "Bedroom" || tag == "Bathroom")
            {
                ExitActionMode();
                currentTarget = hit.transform;
                ShowMenuAbove(currentTarget);
                menuJustOpened = true;

                // Lock movement
                if (playerController != null)
                    playerController.isMovementLocked = true;

                // Focus first button
                Button firstBtn = objectMenu.GetComponentInChildren<Button>();
                if (firstBtn != null)
                    EventSystem.current.SetSelectedGameObject(firstBtn.gameObject);
            }
        }
    }

    void ShowMenuAbove(Transform target)
    {
        objectMenu.SetActive(false);

        Vector3 objectPosition = target.position;

        // Use fixed vertical offset instead of relying on bounds
        Vector3 menuPos = objectPosition + Vector3.up * verticalOffset;

        objectMenu.transform.position = menuPos;
        objectMenu.transform.rotation = Quaternion.LookRotation(menuPos - cameraTransform.position);

        objectMenu.SetActive(true);
        Debug.Log("Object Menu spawned at fixed offset above target.");
    }

    public bool IsMenuOpen()
    {
        return objectMenu.activeSelf;
    }

    public void CloseMenu()
    {
        Debug.Log("Closing Object Menu...");
        objectMenu.SetActive(false);
        currentActionMode = ActionMode.None;
        ExitActionMode();
        currentTarget = null;

        if (playerController != null)
            playerController.isMovementLocked = false;
    }


    public void StartRotateMode()
    {
        if (!currentTarget) return;
        objectMenu.SetActive(false);
        currentActionMode = ActionMode.Rotate;
        if (movementComponent != null) movementComponent.enabled = false;
    }

    public void StartRepositionMode()
    {
        if (!currentTarget) return;
        objectMenu.SetActive(false);
        currentActionMode = ActionMode.Reposition;
        if (playerController != null) playerController.isMovementLocked = true;
    }

    public void StoreObjectToInventory()
    {
        if (!currentTarget || !inventoryManager) return;

        string category = currentTarget.CompareTag("LivingRoom") ? "Living"
                        : currentTarget.CompareTag("Bedroom") ? "Bedroom"
                        : currentTarget.CompareTag("Bathroom") ? "Bathroom"
                        : "Living";

        if (movementComponent != null) movementComponent.enabled = true;

        inventoryManager.StoreObject(currentTarget.gameObject, category);

        currentTarget = null;
        objectMenu.SetActive(false);

        if (playerController != null)
            playerController.isMovementLocked = false;
    }

    public void ExitActionMode()
    {
        currentActionMode = ActionMode.None;
        if (movementComponent != null)
            movementComponent.enabled = true;
    }
}

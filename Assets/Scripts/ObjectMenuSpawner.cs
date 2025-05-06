using UnityEngine;

public class ObjectMenuSpawner : MonoBehaviour
{
    public GameObject objectMenu;
    public Behaviour movementComponent;
    public ReticlePointer reticlePointer;
    public float selectableDistance = 10f;
    public float verticalOffset = 1.2f; // Menu closer to object

    public float rotationSpeed = 90f;
    public float repositionSpeed = 2f;

    public Transform cameraTransform;
    private Camera cam;
    private Transform currentTarget;
    public Transform CurrentTarget => currentTarget;

    public AdvancedInventoryManager inventoryManager;

    public enum ActionMode { None, Rotate, Reposition }
    private ActionMode currentActionMode = ActionMode.None;

    private bool menuJustOpened = false;

    void Start()
    {
        cam = Camera.main;
        if (cam != null)
        {
            cameraTransform = cam.transform;
            Debug.Log("✅ Camera assigned.");
        }
        else
        {
            Debug.LogError("❌ Main Camera not found!");
        }

        if (movementComponent == null)
        {
            GameObject character = GameObject.Find("Character");
            if (character != null)
            {
                movementComponent = character.GetComponent<PlayerController>();
                Debug.Log("✅ PlayerController assigned.");
            }
        }

        objectMenu.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O) || Input.GetKeyDown(KeyCode.JoystickButton3))
        {
            Debug.Log("🟣 [Input] Toggling object menu...");
            if (objectMenu.activeSelf) CloseMenu();
            else TryOpenMenu();
        }

        if (menuJustOpened)
        {
            menuJustOpened = false;
            return; // Prevent input on the same frame as menu opening
        }

        if (currentActionMode == ActionMode.Rotate && currentTarget != null)
        {
            if (Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.JoystickButton10))
            {
                float delta = rotationSpeed * Time.deltaTime;
                currentTarget.Rotate(Vector3.up * delta);
                Debug.Log("🔁 Rotating object...");
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
                Debug.Log("↔️ Moving object...");
            }
        }

        if (Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.JoystickButton5))
        {
            ExitActionMode();
        }
    }

    void TryOpenMenu()
    {
        if (cam == null)
        {
            Debug.LogError("❌ [TryOpenMenu] Main Camera is not assigned!");
            return;
        }

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, selectableDistance))
        {
            string tag = hit.collider.tag;

            if (tag == "LivingRoom" || tag == "Bedroom" || tag == "Bathroom")
            {
                ExitActionMode(); // Clear mode before opening menu
                currentTarget = hit.transform;
                ShowMenuAbove(currentTarget);
                menuJustOpened = true;
            }
            else
            {
                Debug.Log("⛔ Invalid tag hit: " + tag);
            }
        }
        else
        {
            Debug.Log("📭 No object hit.");
        }
    }

    void ShowMenuAbove(Transform target)
    {
        objectMenu.SetActive(false);

        Renderer rend = target.GetComponentInChildren<Renderer>();
        Vector3 top = rend ? new Vector3(rend.bounds.center.x, rend.bounds.max.y, rend.bounds.center.z) : target.position;
        Vector3 menuPos = top + Vector3.up * verticalOffset;

        objectMenu.transform.position = menuPos;
        objectMenu.transform.rotation = Quaternion.LookRotation(menuPos - cameraTransform.position);

        objectMenu.SetActive(true);
        Debug.Log("📌 Menu opened above: " + target.name);
    }

    public void CloseMenu()
    {
        objectMenu.SetActive(false);
        ExitActionMode();
        currentTarget = null;
        Debug.Log("🛑 Menu closed and action reset.");
    }

    public void StartRotateMode()
    {
        if (!currentTarget) return;
        currentActionMode = ActionMode.None;
        objectMenu.SetActive(false);
        currentActionMode = ActionMode.Rotate;
        if (movementComponent != null) movementComponent.enabled = false;

        Debug.Log("🔁 Entered Rotate Mode.");
    }

    public void StartRepositionMode()
    {
        if (!currentTarget) return;
        currentActionMode = ActionMode.None;
        objectMenu.SetActive(false);
        currentActionMode = ActionMode.Reposition;
        if (movementComponent != null) movementComponent.enabled = false;

        Debug.Log("↔️ Entered Reposition Mode.");
    }

    public void StoreObjectToInventory()
    {
        if (!currentTarget || !inventoryManager) return;

        string category = currentTarget.CompareTag("LivingRoom") ? "Living"
                        : currentTarget.CompareTag("Bedroom") ? "Bedroom"
                        : currentTarget.CompareTag("Bathroom") ? "Bathroom"
                        : "Living";

        currentActionMode = ActionMode.None;
        if (movementComponent != null) movementComponent.enabled = true;

        inventoryManager.StoreObject(currentTarget.gameObject, category);
        Debug.Log($"📦 Stored: {currentTarget.name} to {category}");

        currentTarget = null;
        objectMenu.SetActive(false);
    }

    public void ExitActionMode()
    {
        currentActionMode = ActionMode.None;
        if (movementComponent != null)
            movementComponent.enabled = true;
        Debug.Log("❌ Cleared action mode.");
    }
}

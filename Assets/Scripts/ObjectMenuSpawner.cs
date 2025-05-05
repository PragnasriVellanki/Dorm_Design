using UnityEngine;
using System.Collections;

public class ObjectMenuSpawner : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public GameObject objectMenu;
    public Behaviour movementComponent;
    public ReticlePointer reticlePointer;
    [Tooltip("Maximum distance for the object to be selectable (should match OutlineSelection)")]
    public float selectableDistance = 10f;
    [Tooltip("Height above the object's top where the menu appears")]
    public float verticalOffset = 2f; // Changed from 0.1f to 2f for proper placement

    [Header("Continuous Action Settings")]
    [SerializeField] float rotationSpeed = 90f;
    [SerializeField] float repositionSpeed = 2f;
    [SerializeField, Range(0.05f, 2f)] float dragRate = 0.2f;

    private Vector2 dragDegrees = Vector2.zero;
    private Quaternion initialRotation;
    [SerializeField] Transform cameraTransform = default;
    private Camera cam;
    private Transform currentTarget;
    public Transform CurrentTarget => currentTarget;

    public enum ActionMode { None, Rotate, Reposition }
    private ActionMode currentActionMode = ActionMode.None;

#if UNITY_EDITOR
    private Vector3 lastMousePos;
    private bool vrActive;
#endif

    void Awake()
    {
        cam = cameraTransform != null ? cameraTransform.GetComponent<Camera>() : Camera.main;
        initialRotation = cameraTransform.rotation;
    }

    void Start()
    {
        objectMenu.SetActive(false);
    }

    void Update()
    {
        // Open/close menu using Y (JoystickButton3) or L key
        if (Input.GetKeyDown(KeyCode.L) || Input.GetKeyDown(KeyCode.JoystickButton3))
        {
            if (objectMenu.activeSelf)
            {
                CloseMenu();
                TryOpenMenu();
            }
            else
            {
                TryOpenMenu();
            }
        }

        if (currentActionMode != ActionMode.None && currentTarget != null)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            if (currentActionMode == ActionMode.Rotate)
            {
                // 🔁 Rotate with joystick or A button
                if (Mathf.Abs(horizontal) > 0.01f)
                {
                    float deltaAngle = horizontal * rotationSpeed * Time.deltaTime;
                    currentTarget.Rotate(0f, deltaAngle, 0f);
                }
                else if (Input.GetKey(KeyCode.JoystickButton10)) // A button (hold to rotate)
                {
                    float deltaAngle = rotationSpeed * Time.deltaTime;
                    currentTarget.Rotate(0f, deltaAngle, 0f);
                }
            }
            else if (currentActionMode == ActionMode.Reposition && cam != null)
            {
                Vector3 moveInput = new Vector3(horizontal, 0f, vertical);
                if (moveInput.sqrMagnitude > 0.001f)
                {
                    Vector3 moveDirection = cam.transform.right * moveInput.x + cam.transform.forward * moveInput.z;
                    moveDirection.y = 0f;
                    Vector3 deltaMove = moveDirection * repositionSpeed * Time.deltaTime;
                    currentTarget.position += deltaMove;
                }
            }

            // 🚪 Exit any action mode by pressing B (JoystickButton5)
            if (Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.JoystickButton5))
            {
                currentActionMode = ActionMode.None;
                movementComponent.enabled = true;
                Debug.Log("❌ Exited action mode.");
            }
        }
        else
        {
#if UNITY_EDITOR
            if (vrActive)
                SimulateVR();
            else
                SimulateDrag();
#else
            if (!UnityEngine.XR.XRSettings.enabled)
                CheckDrag();
#endif
            UpdateCameraRotation();
        }
    }

    private void UpdateCameraRotation()
    {
        Quaternion attitude = initialRotation * Quaternion.Euler(dragDegrees.x, 0f, 0f);
        cameraTransform.rotation = Quaternion.Euler(0f, -dragDegrees.y, 0f) * attitude;
    }

    public void ResetCamera()
    {
        cameraTransform.rotation = initialRotation;
        dragDegrees = Vector2.zero;
    }

    public void DisableVR()
    {
#if UNITY_EDITOR
        vrActive = false;
#else
        var xrManager = UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager;
        if (xrManager.isInitializationComplete)
        {
            xrManager.StopSubsystems();
            xrManager.DeinitializeLoader();
        }
#endif
        SetObjects(false);
        ResetCamera();
        cam.ResetAspect();
        cam.fieldOfView = 60f;
        cam.ResetProjectionMatrix();
        cam.ResetWorldToCameraMatrix();
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
    }

    public void EnableVR() => EnableVRCoroutine();

    public Coroutine EnableVRCoroutine()
    {
        return StartCoroutine(enableVRRoutine());
        IEnumerator enableVRRoutine()
        {
            SetObjects(true);
#if UNITY_EDITOR
            yield return null;
            vrActive = true;
#else
            var xrManager = UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager;
            if (!xrManager.isInitializationComplete)
                yield return xrManager.InitializeLoader();
            xrManager.StartSubsystems();
#endif
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            ResetCamera();
        }
    }

    void SetObjects(bool vrActive) { }

    void CheckDrag()
    {
        if (Input.touchCount <= 0) return;
        Touch touch = Input.GetTouch(0);
        dragDegrees.x += touch.deltaPosition.y * dragRate;
        dragDegrees.y += touch.deltaPosition.x * dragRate;
    }

#if UNITY_EDITOR
    void SimulateVR()
    {
        Vector3 mousePos = Input.mousePosition;
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            Vector3 delta = mousePos - lastMousePos;
            dragDegrees.x -= delta.y * dragRate;
            dragDegrees.y -= delta.x * dragRate;
        }
        lastMousePos = mousePos;
    }

    void SimulateDrag()
    {
        Vector3 mousePos = Input.mousePosition;
        if (Input.GetMouseButton(0))
        {
            Vector3 delta = mousePos - lastMousePos;
            dragDegrees.x += delta.y * dragRate;
            dragDegrees.y += delta.x * dragRate;
        }
        lastMousePos = mousePos;
    }
#endif

    private void TryOpenMenu()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("No Main Camera found!");
            return;
        }

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, selectableDistance))
        {
            if (hit.collider.CompareTag("Selectable"))
            {
                currentTarget = hit.transform;
                ShowMenuAt(currentTarget);
            }
            else
            {
                Debug.Log("Hit object is not selectable: " + hit.collider.name);
            }
        }
        else
        {
            Debug.Log("No object hit by the raycast from the center of the screen.");
        }
    }

    private void ShowMenuAt(Transform target)
    {
        movementComponent.enabled = false;
        objectMenu.transform.SetParent(null);

        // Get bounds of the full object including children
        Renderer rend = target.GetComponentInChildren<Renderer>();
        Vector3 top = target.position;

        if (rend != null)
        {
            top = new Vector3(
                rend.bounds.center.x,
                rend.bounds.max.y,
                rend.bounds.center.z
            );
        }

        // Menu appears 2 units above top
        Vector3 menuPosition = top + Vector3.up * verticalOffset;
        objectMenu.transform.position = menuPosition;

        // Menu faces camera
        objectMenu.transform.rotation = Quaternion.LookRotation(menuPosition - cam.transform.position);

        objectMenu.SetActive(true);
    }



    public void CloseMenu()
    {
        CloseMenu(true);
    }

    public void CloseMenu(bool reenableMovement, bool keepTarget = false)
    {
        objectMenu.SetActive(false);
        if (!keepTarget)
            currentTarget = null;
        if (reenableMovement)
            movementComponent.enabled = true;
    }

    public void DeleteItem()
    {
        if (currentTarget != null)
        {
            Destroy(currentTarget.gameObject);
            CloseMenu();
        }
    }

    public void StartRotateMode()
    {
        if (currentTarget != null)
        {
            currentActionMode = ActionMode.None;
            CloseMenu(false, true);
            movementComponent.enabled = false;
            currentActionMode = ActionMode.Rotate;
            Debug.Log("✅ Entered Rotate Mode.");
        }
        else
        {
            Debug.LogWarning("No target selected to rotate.");
        }
    }

    public void StartRepositionMode()
    {
        if (currentTarget != null)
        {
            currentActionMode = ActionMode.None;
            CloseMenu(false, true);
            movementComponent.enabled = false;
            currentActionMode = ActionMode.Reposition;
            Debug.Log("Entered Reposition Mode.");
        }
        else
        {
            Debug.LogWarning("No target selected to reposition.");
        }
    }

    

}

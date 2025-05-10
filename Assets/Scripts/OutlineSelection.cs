using UnityEngine;

[RequireComponent(typeof(Collider))]
public class OutlineSelection : MonoBehaviour
{
    private Outline outline;
    private Camera raycastCamera;

    [Header("Selectable Settings")]
    [Tooltip("Maximum distance for the object to be selectable via raycast")]
    public float selectableDistance = 10f;

    void Start()
    {
        // Add or fetch the Outline component
        outline = GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
            outline.OutlineColor = Color.magenta;
            outline.OutlineWidth = 7.0f;
            outline.OutlineMode = Outline.Mode.OutlineVisible;
        }

        outline.enabled = false;

        // Assign the correct local PlayerCamera
        Camera[] allCams = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera cam in allCams)
        {
            if (cam.CompareTag("PlayerCamera") && cam.gameObject.activeInHierarchy)
            {
                raycastCamera = cam;
                Debug.Log("✅ OutlineSelection assigned PlayerCamera.");
                break;
            }
        }

        if (raycastCamera == null)
        {
            Debug.LogWarning("⚠️ No active PlayerCamera found for OutlineSelection!");
        }
    }

    void Update()
    {
        if (raycastCamera == null || outline == null) return;

        Ray ray = raycastCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, selectableDistance))
        {
            bool isThisHit = hit.collider != null && hit.collider.gameObject == gameObject;
            outline.enabled = isThisHit;
        }
        else
        {
            outline.enabled = false;
        }
    }
}

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

        // Automatically find and assign the Main Camera
        GameObject camObj = GameObject.Find("Main Camera");
        if (camObj != null)
        {
            raycastCamera = camObj.GetComponent<Camera>();
            Debug.Log("✅ OutlineSelection assigned Main Camera.");
        }
        else
        {
            Debug.LogWarning("⚠️ Main Camera not found by name!");
        }
    }

    void Update()
    {
        if (raycastCamera == null || outline == null) return;

        // Raycast from center of the camera view
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

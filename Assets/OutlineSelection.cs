using UnityEngine;

public class OutlineSelection : MonoBehaviour
{
    private Outline outline;

    [Header("Selectable Settings")]
    [Tooltip("Maximum distance for the object to be selectable via raycast")]
    public float selectableDistance = 10f;

    void Start()
    {
        outline = GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
            outline.OutlineColor = Color.magenta;
            outline.OutlineWidth = 7.0f;
            outline.OutlineMode = Outline.Mode.OutlineVisible;
        }

        outline.enabled = false;
    }

    public Camera raycastCamera;

    void Update()
    {
        Camera cam = raycastCamera != null ? raycastCamera : Camera.main;
        if (cam == null || outline == null) return;

        // Cast from center of screen or reticle
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

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

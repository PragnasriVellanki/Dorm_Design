using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class OutlineAllChildren : MonoBehaviour
{
    public float selectableDistance = 10f;
    private Camera raycastCamera;
    private List<Outline> outlines = new List<Outline>();

    void Start()
    {
        // Find and cache all outline targets in children
        foreach (Renderer rend in GetComponentsInChildren<Renderer>())
        {
            Outline o = rend.gameObject.GetComponent<Outline>();
            if (o == null)
            {
                o = rend.gameObject.AddComponent<Outline>();
                o.OutlineColor = Color.cyan;
                o.OutlineWidth = 6f;
                o.OutlineMode = Outline.Mode.OutlineVisible;
            }

            o.enabled = false;
            outlines.Add(o);
        }

        // Get the raycast camera
        Camera[] allCams = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera cam in allCams)
        {
            if (cam.CompareTag("PlayerCamera") && cam.gameObject.activeInHierarchy)
            {
                raycastCamera = cam;
                break;
            }
        }

        if (raycastCamera == null)
            Debug.LogWarning("⚠️ No camera with 'PlayerCamera' tag found.");
    }

    void Update()
    {
        if (raycastCamera == null) return;

        Ray ray = raycastCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, selectableDistance))
        {
            if (hit.collider != null && IsPartOfRobot(hit.collider.transform))
            {
                SetOutlines(true);
                return;
            }
        }

        SetOutlines(false);
    }

    bool IsPartOfRobot(Transform t)
    {
        return t.IsChildOf(this.transform);
    }

    void SetOutlines(bool state)
    {
        foreach (var o in outlines)
            o.enabled = state;
    }
}

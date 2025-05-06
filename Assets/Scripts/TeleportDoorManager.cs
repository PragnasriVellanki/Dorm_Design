using UnityEngine;

public class TeleportDoorManager : MonoBehaviour
{
    public float maxRaycastDistance = 10f;
    private TeleportDoor currentHoveredDoor = null;
    private Camera cam;

    void Start()
    {
        Camera[] allCams = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera c in allCams)
        {
            if (c.CompareTag("PlayerCamera") && c.gameObject.activeInHierarchy)
            {
                cam = c;
                break;
            }
        }

        if (cam == null)
            Debug.LogError("❌ No local PlayerCamera found in TeleportDoorManager.");
    }


    void Update()
    {
        if (cam == null) return; // Prevent crash

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance))
        {
            TeleportDoor door = hit.collider.GetComponent<TeleportDoor>();
            if (door != null)
            {
                if (currentHoveredDoor != door)
                {
                    ClearPreviousHighlight();
                    currentHoveredDoor = door;
                    currentHoveredDoor.SetHover(true);
                }
            }
            else
            {
                ClearPreviousHighlight();
            }
        }
        else
        {
            ClearPreviousHighlight();
        }
    }



    void ClearPreviousHighlight()
    {
        if (currentHoveredDoor != null)
        {
            currentHoveredDoor.SetHover(false);
            currentHoveredDoor = null;
        }
    }
}

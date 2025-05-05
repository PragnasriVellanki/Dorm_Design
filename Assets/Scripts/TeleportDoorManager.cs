using UnityEngine;

public class TeleportDoorManager : MonoBehaviour
{
    public float maxRaycastDistance = 10f;
    private TeleportDoor currentHoveredDoor = null;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxRaycastDistance))
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

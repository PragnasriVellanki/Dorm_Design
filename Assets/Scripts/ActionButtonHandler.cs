using UnityEngine;
using UnityEngine.EventSystems;

public class ActionButtonHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public enum ActionType
    {
        Delete,            // 👈 Now behaves like StoreToInventory
        Rotate,
        Reposition,
        StoreToInventory
    }

    public ActionType actionType;
    public ObjectMenuSpawner objectMenuSpawner;

    private bool isHovered = false;

    void Update()
    {
        if (isHovered && (Input.GetKeyDown(KeyCode.JoystickButton2) || Input.GetKeyDown(KeyCode.JoystickButton10))) // X or A
        {
            if (objectMenuSpawner == null)
            {
                Debug.LogWarning("⚠️ ObjectMenuSpawner not assigned!");
                return;
            }

            switch (actionType)
            {
                case ActionType.Delete:
                

                case ActionType.Rotate:
                    objectMenuSpawner.StartRotateMode();
                    break;

                case ActionType.Reposition:
                    objectMenuSpawner.StartRepositionMode();
                    break;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }
}

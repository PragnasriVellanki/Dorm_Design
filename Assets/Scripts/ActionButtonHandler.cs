using UnityEngine;
using UnityEngine.EventSystems;

public class ActionButtonHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public enum ActionType { StoreToInventory, Rotate, Reposition, Exit }
    public ActionType actionType;
    public ObjectMenuSpawner objectMenuSpawner;

    private bool isHovered;

    void Update()
    {
        if (isHovered && (Input.GetKeyDown(KeyCode.JoystickButton2) || Input.GetKeyDown(KeyCode.L)))
        {
            switch (actionType)
            {
                case ActionType.StoreToInventory:
                    objectMenuSpawner.StoreObjectToInventory();
                    break;
                case ActionType.Rotate:
                    objectMenuSpawner.StartRotateMode();
                    break;
                case ActionType.Reposition:
                    objectMenuSpawner.StartRepositionMode();
                    break;
                case ActionType.Exit:
                    objectMenuSpawner.CloseMenu();
                    break;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"🟣 Hovered: {gameObject.name}");
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData) => isHovered = false;
}

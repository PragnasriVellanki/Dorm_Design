using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryCategoryButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public enum CategoryType
    {
        LivingRoom,
        BedRoom,
        BathRoom
    }

    [Header("Setup")]
    public CategoryType categoryType;
    public AdvancedInventoryManager inventoryManager;

    [Header("Highlight")]
    public Image buttonImage;                    
    public Color highlightColor = Color.yellow;  
    private Color originalColor;

    private bool isHovered = false;

    void Start()
    {
        if (buttonImage != null)
            originalColor = buttonImage.color;
        else
            Debug.LogWarning("No Button Image assigned on: " + gameObject.name);
    }

    void Update()
    {
        if (isHovered && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton2)))
        {
            if (inventoryManager == null)
            {
                Debug.LogWarning("InventoryManager not assigned on: " + gameObject.name);
                return;
            }

            switch (categoryType)
            {
                case CategoryType.LivingRoom:
                    inventoryManager.LoadLivingRoom();
                    break;
                case CategoryType.BedRoom:
                    inventoryManager.LoadBedRoom();
                    break;
                case CategoryType.BathRoom:
                    inventoryManager.LoadBathRoom();
                    break;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;

        if (buttonImage != null)
            buttonImage.color = highlightColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;

        if (buttonImage != null)
            buttonImage.color = originalColor;
    }
}

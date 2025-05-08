using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUIManager : MonoBehaviour
{
    [Header("Category Buttons")]
    public List<Button> categoryButtons;

    [Header("Inventory Slots")]
    public Transform inventoryContentParent;
    public ScrollRect inventoryScrollRect;

    [Header("Navigation Settings")]
    public float inputDelay = 0.3f;

    private List<Button> inventorySlotButtons = new List<Button>();
    private int currentCategoryIndex = 0;
    private int currentSlotIndex = 0;

    private float inputCooldown = 0f;
    private bool inCategoryPanel = true;

    [Header("Grid Settings")]
    public int slotsPerRow = 4;

    void Start()
    {
        EventSystem.current.SetSelectedGameObject(categoryButtons[0].gameObject);
    }

    void Update()
    {
        if (inputCooldown > 0f)
        {
            inputCooldown -= Time.unscaledDeltaTime;
            return;
        }

        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        // 🟢 Select button using JoystickButton2 (X)
        if (Input.GetKeyDown(KeyCode.JoystickButton2))
        {
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            if (selected != null)
            {
                Button btn = selected.GetComponent<Button>();
                btn?.onClick.Invoke();
            }

            inputCooldown = inputDelay;
            return;
        }

        if (inCategoryPanel)
        {
            if (horizontal > 0.5f)
            {
                inCategoryPanel = false;
                if (inventorySlotButtons.Count > 0)
                {
                    EventSystem.current.SetSelectedGameObject(inventorySlotButtons[currentSlotIndex].gameObject);
                    ScrollToButton(inventorySlotButtons[currentSlotIndex]);
                }

                inputCooldown = inputDelay;
            }
            else if (Mathf.Abs(vertical) > 0.5f)
            {
                NavigateCategory(vertical < 0 ? 1 : -1);
            }
        }
        else
        {
            if (horizontal > 0.5f)
            {
                NavigateInventory(1);
            }
            else if (horizontal < -0.5f)
            {
                // Go back to category panel if far left
                if (currentSlotIndex % slotsPerRow == 0)
                {
                    inCategoryPanel = true;
                    EventSystem.current.SetSelectedGameObject(categoryButtons[currentCategoryIndex].gameObject);
                }
                else
                {
                    NavigateInventory(-1);
                }

                inputCooldown = inputDelay;
            }
            else if (vertical > 0.5f)
            {
                NavigateInventory(-slotsPerRow);
            }
            else if (vertical < -0.5f)
            {
                NavigateInventory(slotsPerRow);
            }
        }
    }

    public void NavigateCategory(int direction)
    {
        currentCategoryIndex += direction;
        currentCategoryIndex = Mathf.Clamp(currentCategoryIndex, 0, categoryButtons.Count - 1);
        EventSystem.current.SetSelectedGameObject(categoryButtons[currentCategoryIndex].gameObject);
        inputCooldown = inputDelay;
    }

    public void NavigateInventory(int direction)
    {
        if (inventorySlotButtons.Count == 0) return;

        int nextIndex = currentSlotIndex + direction;
        if (nextIndex >= 0 && nextIndex < inventorySlotButtons.Count)
        {
            currentSlotIndex = nextIndex;
            EventSystem.current.SetSelectedGameObject(inventorySlotButtons[currentSlotIndex].gameObject);
            ScrollToButton(inventorySlotButtons[currentSlotIndex]);
            inputCooldown = inputDelay;
        }
    }

    public void ScrollToButton(Button button)
    {
        if (inventoryScrollRect == null) return;

        RectTransform content = inventoryContentParent.GetComponent<RectTransform>();
        RectTransform buttonRect = button.GetComponent<RectTransform>();

        Vector2 pos = (Vector2)inventoryScrollRect.transform.InverseTransformPoint(content.position)
                    - (Vector2)inventoryScrollRect.transform.InverseTransformPoint(buttonRect.position);

        content.anchoredPosition += new Vector2(0, pos.y);
    }

    public void SetInventoryButtons(List<Button> buttons)
    {
        inventorySlotButtons = buttons;
        currentSlotIndex = 0;
    }
}

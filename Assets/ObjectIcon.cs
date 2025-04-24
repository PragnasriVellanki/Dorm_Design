using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ObjectIcon : MonoBehaviour, IPointerEnterHandler
{
    public Image iconImage;
    private int myIndex;

    private void Awake()
    {
        if (iconImage == null)
            iconImage = GetComponentInChildren<Image>();
    }

    public void SetObject(GameObject obj)
    {
        ObjectThumbnail thumb = obj.GetComponent<ObjectThumbnail>();
        iconImage.sprite = (thumb != null && thumb.thumbnailSprite != null) ? thumb.thumbnailSprite : null;
        iconImage.enabled = true;
    }

    public void Clear()
    {
        iconImage.sprite = null;
        iconImage.enabled = false;
    }

    public void SetIndex(int index)
    {
        myIndex = index;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        InventoryUIManager.Instance.HighlightSlotByMouse(myIndex);
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdvancedInventoryManager : MonoBehaviour
{
    [Header("Prefabs by Category")]
    public List<GameObject> livingRoomPrefabs;
    public List<GameObject> bedRoomPrefabs;
    public List<GameObject> bathRoomPrefabs;

    [Header("Inventory Grid Setup")]
    public Transform inventoryContentParent;       // ScrollView/Viewport/Content
    public GameObject inventorySlotButtonPrefab;   // Your slot button prefab (150x150)

    [Header("Object Spawn")]
    public Transform grabPoint;                    // Where to spawn the selected prefab

    private void Start()
    {
        LoadCategory("Living");
    }

    public void LoadCategory(string category)
    {
        // 1. Clear existing buttons
        foreach (Transform child in inventoryContentParent)
        {
            Destroy(child.gameObject);
        }

        // 2. Pick the correct list
        List<GameObject> prefabs = category switch
        {
            "Living" => livingRoomPrefabs,
            "Bedroom" => bedRoomPrefabs,
            "Bathroom" => bathRoomPrefabs,
            _ => new List<GameObject>()
        };

        // 3. Instantiate buttons
        foreach (GameObject prefab in prefabs)
        {
            GameObject buttonObj = Instantiate(inventorySlotButtonPrefab, inventoryContentParent);
            Button btn = buttonObj.GetComponent<Button>();

            // Optional: set image icon
            Image img = buttonObj.GetComponentInChildren<Image>();
            ObjectThumbnail thumb = prefab.GetComponent<ObjectThumbnail>();
            if (img != null && thumb != null)
                img.sprite = thumb.thumbnail;

            btn.onClick.AddListener(() =>
            {
                GameObject spawned = Instantiate(prefab, grabPoint.position, Quaternion.identity);
                spawned.tag = "Selectable";
                spawned.SetActive(true);
            });
        }
    }

    // These can be assigned to button OnClick events directly
    public void LoadLivingRoom() => LoadCategory("Living");
    public void LoadBedRoom() => LoadCategory("Bedroom");
    public void LoadBathRoom() => LoadCategory("Bathroom");
}

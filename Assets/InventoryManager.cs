using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public List<GameObject> storedObjects = new List<GameObject>();
    public int maxSlots = 20;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public bool StoreObject(GameObject obj)
    {
        if (storedObjects.Count >= maxSlots)
            return false;

        storedObjects.Add(obj);
        obj.SetActive(false); // Hide in scene

        if (obj.GetComponent<ObjectThumbnail>() == null)
            Debug.LogWarning($"[Inventory] {obj.name} is missing ObjectThumbnail.");

        InventoryUIManager.Instance.RefreshSlots(); // ✅ FIXED
        return true;
    }

    public void RemoveObject(int index)
    {
        if (index >= 0 && index < storedObjects.Count)
        {
            storedObjects.RemoveAt(index);
            InventoryUIManager.Instance.RefreshSlots(); // ✅ FIXED
        }
    }

    public GameObject GetObject(int index)
    {
        return (index >= 0 && index < storedObjects.Count) ? storedObjects[index] : null;
    }
}

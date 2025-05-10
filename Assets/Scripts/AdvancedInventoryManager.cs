using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class AdvancedInventoryManager : MonoBehaviour
{
    [Header("Prefabs by Category (Auto-loaded at runtime)")]
    public List<string> livingRoomPrefabNames;
    public List<string> bedRoomPrefabNames;
    public List<string> bathRoomPrefabNames;

    [Header("Inventory Grid Setup")]
    public Transform inventoryContentParent;
    public GameObject inventorySlotButtonPrefab;

    private Transform grabPoint;
    private GameObject currentHeldObject;
    private AdvancedInventoryUIController uiController;


    private List<GameObject> placedObjects = new List<GameObject>();
    private Dictionary<string, string> prefabPathMap = new Dictionary<string, string>();

    void Start()
    {
        GameObject gp = GameObject.Find("GrabPoint");
        if (gp != null) grabPoint = gp.transform;

        uiController = UnityEngine.Object.FindFirstObjectByType<AdvancedInventoryUIController>();


        // ✅ Auto-load prefab names from flat Prefabs folder and categorize by name
        CategorizePrefabs("Prefabs");

        LoadCategory("Living");
    }

    private void CategorizePrefabs(string path)
    {
        GameObject[] prefabs = Resources.LoadAll<GameObject>(path);
        foreach (GameObject prefab in prefabs)
        {
            if (prefab == null) continue;

            string name = prefab.name;
            prefabPathMap[name] = path + "/" + name;

            if (name.StartsWith("Bed_"))
                bedRoomPrefabNames.Add(name);
            else if (name.StartsWith("Bath_"))
                bathRoomPrefabNames.Add(name);
            else
                livingRoomPrefabNames.Add(name);
        }

        Debug.Log($"✅ Loaded {livingRoomPrefabNames.Count} Living, {bedRoomPrefabNames.Count} Bedroom, {bathRoomPrefabNames.Count} Bathroom prefabs.");
    }

    void Update()
    {
        if (currentHeldObject != null && (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.JoystickButton5)))
        {
            DropHeldObject();
        }

        if (currentHeldObject != null && grabPoint != null)
        {
            currentHeldObject.transform.position = grabPoint.position;
        }
    }

    public void StoreObject(GameObject obj, string category)
    {
        if (obj == null) return;
        if (placedObjects.Contains(obj)) placedObjects.Remove(obj);

        obj.SetActive(false);
        string prefabName = obj.name.Replace("(Clone)", "").Trim();

        switch (category)
        {
            case "Living": if (!livingRoomPrefabNames.Contains(prefabName)) livingRoomPrefabNames.Add(prefabName); break;
            case "Bedroom": if (!bedRoomPrefabNames.Contains(prefabName)) bedRoomPrefabNames.Add(prefabName); break;
            case "Bathroom": if (!bathRoomPrefabNames.Contains(prefabName)) bathRoomPrefabNames.Add(prefabName); break;
            default: if (!livingRoomPrefabNames.Contains(prefabName)) livingRoomPrefabNames.Add(prefabName); break;
        }
    }

    public List<string> GetAllPlacedObjectNames()
    {
        placedObjects.RemoveAll(obj => obj == null);
        List<string> names = new List<string>();
        foreach (var obj in placedObjects)
            names.Add(obj.name.Replace("(Clone)", "").Trim());
        return names;
    }

    public List<string> GetAllPlacedObjectRawNames()
    {
        placedObjects.RemoveAll(obj => obj == null);
        return placedObjects.ConvertAll(obj => obj.name);
    }

    public void LoadCategory(string category)
    {
        foreach (Transform child in inventoryContentParent)
            Destroy(child.gameObject);

        List<string> prefabNames = category switch
        {
            "Living" => livingRoomPrefabNames,
            "Bedroom" => bedRoomPrefabNames,
            "Bathroom" => bathRoomPrefabNames,
            _ => new List<string>()
        };

        foreach (string prefabName in prefabNames)
        {
            GameObject prefab = null;
            if (prefabPathMap.TryGetValue(prefabName, out string fullPath))
                prefab = Resources.Load<GameObject>(fullPath);

            if (prefab == null)
            {
                Debug.LogWarning($"❌ Could not load prefab: {prefabName}");
                continue;
            }
            else
            {
                Debug.Log($"✅ Loaded prefab: {prefabName}");
            }

            GameObject buttonObj = Instantiate(inventorySlotButtonPrefab, inventoryContentParent);
            Button btn = buttonObj.GetComponent<Button>();

            Image img = buttonObj.GetComponentInChildren<Image>();
            ObjectThumbnail thumb = prefab.GetComponent<ObjectThumbnail>();
            if (img != null && thumb != null && thumb.thumbnail != null)
                img.sprite = thumb.thumbnail;

            btn.onClick.AddListener(() => { GrabObject(prefabName); });
        }

        if (uiController != null)
        {
            List<Button> allButtons = new List<Button>();
            foreach (Transform child in inventoryContentParent)
            {
                Button b = child.GetComponent<Button>();
                if (b != null) allButtons.Add(b);
            }
            uiController.SetInventoryButtons(allButtons);
        }

    }

    private void GrabObject(string prefabName)
    {
        if (grabPoint == null) return;

        if (currentHeldObject != null)
            Destroy(currentHeldObject);

        GameObject prefab = Resources.Load<GameObject>("Prefabs/" + prefabName);
        if (prefab == null)
        {
            Debug.LogError($"❌ Failed to load prefab: {prefabName}");
            return;
        }

        currentHeldObject = Instantiate(prefab, grabPoint.position, Quaternion.identity);
        currentHeldObject.transform.SetParent(grabPoint, false);
        currentHeldObject.transform.localPosition = Vector3.zero;
        currentHeldObject.transform.localRotation = Quaternion.identity;
        currentHeldObject.transform.localScale = Vector3.one * 2f; 


        Rigidbody rb = currentHeldObject.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        // ✅ Close inventory after grabbing
        if (uiController != null)
            uiController.CloseInventoryExternally();

    }


    void DropHeldObject()
    {
        if (currentHeldObject != null)
        {
            currentHeldObject.transform.SetParent(null);
            currentHeldObject.transform.position += Vector3.up * 0.5f;

            Rigidbody rb = currentHeldObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            if (!placedObjects.Contains(currentHeldObject))
                placedObjects.Add(currentHeldObject);

            currentHeldObject = null;
        }
    }

    public void LoadLivingRoom() => LoadCategory("Living");
    public void LoadBedRoom() => LoadCategory("Bedroom");
    public void LoadBathRoom() => LoadCategory("Bathroom");
}

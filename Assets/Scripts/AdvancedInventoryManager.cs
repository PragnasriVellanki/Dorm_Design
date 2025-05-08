using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AdvancedInventoryManager : MonoBehaviour
{
    [Header("Prefabs by Category")]
    public List<GameObject> livingRoomPrefabs;
    public List<GameObject> bedRoomPrefabs;
    public List<GameObject> bathRoomPrefabs;

    [Header("Inventory Grid Setup")]
    public Transform inventoryContentParent;
    public GameObject inventorySlotButtonPrefab;

    private Transform grabPoint;
    private GameObject currentHeldObject;
    private InventoryUIManager uiManager;

    void Start()
    {
        GameObject gp = GameObject.Find("GrabPoint");
        if (gp != null)
            grabPoint = gp.transform;
        else
            Debug.LogWarning("❌ GrabPoint not found.");

        LoadCategory("Living");
        uiManager = UnityEngine.Object.FindFirstObjectByType<InventoryUIManager>();
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

        obj.SetActive(false);

        switch (category)
        {
            case "Living": livingRoomPrefabs.Add(obj); break;
            case "Bedroom": bedRoomPrefabs.Add(obj); break;
            case "Bathroom": bathRoomPrefabs.Add(obj); break;
            default: livingRoomPrefabs.Add(obj); break;
        }

        Debug.Log($"📥 Stored {obj.name} to {category} category.");
    }

    public void LoadCategory(string category)
    {
        foreach (Transform child in inventoryContentParent)
        {
            Destroy(child.gameObject);
        }

        List<GameObject> prefabs = category switch
        {
            "Living" => livingRoomPrefabs,
            "Bedroom" => bedRoomPrefabs,
            "Bathroom" => bathRoomPrefabs,
            _ => new List<GameObject>()
        };

        foreach (GameObject prefab in prefabs)
        {
            GameObject buttonObj = Instantiate(inventorySlotButtonPrefab, inventoryContentParent);
            Button btn = buttonObj.GetComponent<Button>();

            Image img = buttonObj.GetComponentInChildren<Image>();
            ObjectThumbnail thumb = prefab.GetComponent<ObjectThumbnail>();
            if (img != null && thumb != null && thumb.thumbnail != null)
                img.sprite = thumb.thumbnail;

            GameObject capturedPrefab = prefab;
            btn.onClick.AddListener(() =>
            {
                livingRoomPrefabs.Remove(capturedPrefab);
                bedRoomPrefabs.Remove(capturedPrefab);
                bathRoomPrefabs.Remove(capturedPrefab);
                GrabObject(capturedPrefab, null);
            });
        }

        if (uiManager != null)
        {
            List<Button> allButtons = new List<Button>();
            foreach (Transform child in inventoryContentParent)
            {
                Button b = child.GetComponent<Button>();
                if (b != null) allButtons.Add(b);
            }
            uiManager.SetInventoryButtons(allButtons);
        }
    }

    private void GrabObject(GameObject prefab, List<GameObject> sourceList)
    {
        if (grabPoint != null)
        {
            if (currentHeldObject != null)
                Destroy(currentHeldObject);

            currentHeldObject = Instantiate(prefab);
            currentHeldObject.transform.SetParent(null);
            currentHeldObject.transform.SetParent(grabPoint, false);
            currentHeldObject.transform.localScale = Vector3.one;
            currentHeldObject.transform.localPosition = Vector3.zero;
            currentHeldObject.transform.localRotation = Quaternion.identity;

            Vector3 globalScale = currentHeldObject.transform.lossyScale;
            float correctionFactor = 1f / globalScale.x;
            currentHeldObject.transform.localScale *= correctionFactor;

            Rigidbody rb = currentHeldObject.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            Debug.Log($"🛠️ Grabbed object: {currentHeldObject.name}");

            GameObject canvas = GameObject.Find("InventoryCanvas");
            if (canvas != null) canvas.SetActive(false);

            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                PlayerController pc = player.GetComponent<PlayerController>();
                if (pc != null)
                {
                    pc.isMovementLocked = false;
                    Debug.Log("🎮 Player movement unlocked after grabbing.");
                }
            }

            AdvancedInventoryUIController uiController = UnityEngine.Object.FindFirstObjectByType<AdvancedInventoryUIController>();
            if (uiController != null)
            {
                uiController.CloseInventoryExternally();
                Debug.Log("📂 InventoryOpen flag reset via public method.");
            }
        }
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

            Debug.Log("📦 Dropped object: " + currentHeldObject.name);
            currentHeldObject = null;
        }
    }

    public void LoadLivingRoom() => LoadCategory("Living");
    public void LoadBedRoom() => LoadCategory("Bedroom");
    public void LoadBathRoom() => LoadCategory("Bathroom");
}

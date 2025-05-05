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
    public Transform inventoryContentParent;
    public GameObject inventorySlotButtonPrefab;

    private Transform grabPoint;
    private GameObject currentHeldObject;

    void Start()
    {
        GameObject gp = GameObject.Find("GrabPoint");
        if (gp != null)
            grabPoint = gp.transform;
        else
            Debug.LogWarning("❌ GrabPoint not found.");

        LoadCategory("Living");
    }

    void Update()
    {
        // Drop object on Q (keyboard) or A (JoystickButton0)
        if (currentHeldObject != null && (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.JoystickButton0)))
        {
            DropHeldObject();
        }

        // Keep object following grab point (optional for smoothing)
        if (currentHeldObject != null && grabPoint != null)
        {
            currentHeldObject.transform.position = grabPoint.position;
        }
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

            btn.onClick.AddListener(() =>
            {
                if (grabPoint != null)
                {
                    // Destroy existing held object
                    if (currentHeldObject != null)
                        Destroy(currentHeldObject);

                    // Instantiate and attach to grab point
                    currentHeldObject = Instantiate(prefab, grabPoint.position, Quaternion.identity);
                    currentHeldObject.tag = "Selectable";
                    currentHeldObject.SetActive(true);
                    currentHeldObject.transform.SetParent(grabPoint);

                    currentHeldObject = Instantiate(prefab, grabPoint.position, Quaternion.identity);
                    currentHeldObject.transform.localScale = prefab.transform.localScale; // ✅ Original scale


                    // Make it float in hand
                    Rigidbody rb = currentHeldObject.GetComponent<Rigidbody>();
                    if (rb != null)
                        rb.isKinematic = true;

                    Debug.Log($"🛠️ Grabbed and scaled object: {currentHeldObject.name}");

                    // Close inventory
                    GameObject canvas = GameObject.Find("InventoryCanvas");
                    if (canvas != null) canvas.SetActive(false);

                    // Unlock player movement
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
                }
            });



        }
    }

    void DropHeldObject()
    {
        if (currentHeldObject != null)
        {
            // Detach from hand
            currentHeldObject.transform.SetParent(null);

            // Raise it slightly to avoid overlapping the floor
            currentHeldObject.transform.position += Vector3.up * 0.5f;

            // Re-enable physics
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


    // Optional: for external buttons
    public void LoadLivingRoom() => LoadCategory("Living");
    public void LoadBedRoom() => LoadCategory("Bedroom");
    public void LoadBathRoom() => LoadCategory("Bathroom");
}

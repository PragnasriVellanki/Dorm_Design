using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class CustomPrefabPool : MonoBehaviour, IPunPrefabPool
{
    // Key: prefab name (no extension), Value: Resources path
    private Dictionary<string, string> prefabPathMap = new Dictionary<string, string>();

    void Awake()
    {
        // Register all your categories and paths
        RegisterFolder("Prefabs/LivingRoom");
        RegisterFolder("Prefabs/Bedroom");
        RegisterFolder("Prefabs/Bathroom");

        // Set this pool as the active pool
        PhotonNetwork.PrefabPool = this;
    }

    private void RegisterFolder(string folderPath)
    {
        GameObject[] prefabs = Resources.LoadAll<GameObject>(folderPath);
        foreach (GameObject prefab in prefabs)
        {
            if (prefab != null && !prefabPathMap.ContainsKey(prefab.name))
            {
                prefabPathMap[prefab.name] = $"{folderPath}/{prefab.name}";
            }
        }
    }

    public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
    {
        if (prefabPathMap.TryGetValue(prefabId, out string path))
        {
            GameObject prefab = Resources.Load<GameObject>(path);
            if (prefab != null)
            {
                return GameObject.Instantiate(prefab, position, rotation);
            }
        }

        Debug.LogError($"❌ Could not instantiate {prefabId}. Ensure it's registered in CustomPrefabPool.");
        return null;
    }

    public void Destroy(GameObject gameObject)
    {
        GameObject.Destroy(gameObject);
    }
}

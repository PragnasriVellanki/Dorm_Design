using UnityEngine;
using Photon.Pun;

public class DormDesignSpawner : MonoBehaviourPunCallbacks
{
    [Header("Player Prefabs (must be in Resources folder)")]
    public string player1PrefabName = "Character";       // Player 1
    public string player2PrefabName = "Character 1";     // Player 2

    [Header("Spawn Points")]
    public Transform player1SpawnPoint;  // Assign this to LivingRoom spawn
    public Transform player2SpawnPoint;  // Assign this to Bedroom spawn

    void Start()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("⚠️ Photon is not connected yet.");
            return;
        }

        if (player1SpawnPoint == null || player2SpawnPoint == null)
        {
            Debug.LogError("❌ One or both spawn points are not assigned in the Inspector.");
            return;
        }

        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        string prefabToSpawn = actorNumber == 1 ? player1PrefabName : player2PrefabName;
        Transform spawnPoint = actorNumber == 1 ? player1SpawnPoint : player2SpawnPoint;

        GameObject playerObj = PhotonNetwork.Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);

        Debug.Log($"✅ Spawned '{prefabToSpawn}' for Player {actorNumber} at {spawnPoint.position}");
    }
}

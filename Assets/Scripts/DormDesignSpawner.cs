using UnityEngine;
using Photon.Pun;
using System;

public class DormDesignSpawner : MonoBehaviourPunCallbacks
{
    [Header("Player Prefabs (must be in Resources folder)")]
    public string player1PrefabName = "Character";       // For Player 1
    public string player2PrefabName = "Character 1";     // For Player 2

    [Header("Spawn Points")]
    public Transform player1SpawnPoint;  // Living Room
    public Transform player2SpawnPoint;  // Bedroom

    void Start()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("Photon is not connected yet.");
            return;
        }

        if (player1SpawnPoint == null || player2SpawnPoint == null)
        {
            Debug.LogError("Spawn points not assigned.");
            return;
        }

        // Get this player's index in the join order
        int joinIndex = Array.IndexOf(PhotonNetwork.PlayerList, PhotonNetwork.LocalPlayer);

        string prefabToSpawn;
        Transform spawnPoint;

        if (joinIndex == 0)
        {
            prefabToSpawn = player1PrefabName;
            spawnPoint = player1SpawnPoint;
        }
        else
        {
            prefabToSpawn = player2PrefabName;
            spawnPoint = player2SpawnPoint;
        }

        GameObject playerObj = PhotonNetwork.Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
        Debug.Log($"✅ Spawned '{prefabToSpawn}' for Player {joinIndex + 1} at {spawnPoint.position}");
    }
}

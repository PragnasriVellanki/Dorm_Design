using UnityEngine;
using Photon.Pun;

public class DormDesignSpawner : MonoBehaviourPunCallbacks
{
    public Transform spawnPoint;

    void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log("Spawning Player Avatar...");
            PhotonNetwork.Instantiate("Character", spawnPoint.position, Quaternion.identity);

        }
    }
}

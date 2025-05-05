using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PhotonConnectionManager : MonoBehaviourPunCallbacks
{
    public static PhotonConnectionManager Instance;
    private bool isConnectedAndReady = false;

    [Header("Room Settings")]
    public string roomName = "DormRoom";
    public byte maxPlayers = 4;
    public string sceneToLoad = "DormDesign";

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    // ⛔ Removed Start() method to prevent auto-connect

    public void ConnectToPhotonAndStart()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("🔌 Connecting to Photon...");
            PhotonNetwork.ConnectUsingSettings(); // Only connect when user triggers
        }
        else if (isConnectedAndReady)
        {
            Debug.Log("➡️ Already connected, joining room...");
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            Debug.Log("⏳ Photon is not ready yet.");
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("✅ Connected to Photon Master Server!");
        isConnectedAndReady = true;
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("✅ Joined Lobby. Attempting to join room...");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning("⚠️ No random room found. Creating a new one...");
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = maxPlayers };
        PhotonNetwork.CreateRoom(roomName + Random.Range(1000, 9999), roomOptions);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("✅ Joined Room! Loading DormDesign scene...");
        PhotonNetwork.LoadLevel(sceneToLoad);
    }
}

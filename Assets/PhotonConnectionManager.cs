using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PhotonConnectionManager : MonoBehaviourPunCallbacks
{
    public static PhotonConnectionManager Instance;
    private bool isConnectedAndReady = false;

    void Start()
    {
        Instance = this;

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void ConnectToPhotonAndStart()
    {
        if (isConnectedAndReady)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            Debug.Log("Photon not ready yet. Please wait...");
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server!");
        isConnectedAndReady = true;
        PhotonNetwork.JoinLobby(); // Good practice
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby. Ready for matchmaking.");
        isConnectedAndReady = true;
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning("No room found, creating a new one...");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 4 }); // Allow up to 4 players if needed
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room! Loading DormDesign scene...");
        PhotonNetwork.LoadLevel("DormDesign"); // Synchronized load
    }
}

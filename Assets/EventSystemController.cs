using Photon.Pun;
using UnityEngine;

public class EventSystemController : MonoBehaviourPun
{
    void Start()
    {
        if (!photonView.IsMine)
        {
            gameObject.SetActive(false); // Disable remote player's event system
        }
    }
}

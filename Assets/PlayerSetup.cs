using Photon.Pun;
using UnityEngine;

public class PlayerSetup : MonoBehaviourPun
{
    public GameObject reticlePointer;
    public GameObject eventSystem;

    void Start()
    {
        if (!photonView.IsMine)
        {
            if (reticlePointer) reticlePointer.SetActive(false);
            if (eventSystem) eventSystem.SetActive(false);
        }
    }
}

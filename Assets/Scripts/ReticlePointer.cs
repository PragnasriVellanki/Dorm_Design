using UnityEngine;
using Photon.Pun;

public class ReticlePointer : MonoBehaviourPun
{
    [Header("Settings")]
    public float sphereDistance = 10f;

    private Camera localCamera;

    void Start()
    {
        if (!photonView.IsMine)
        {
            // Disable ReticlePointer for remote players
            gameObject.SetActive(false);
            return;
        }

        // Get the camera only from this player prefab
        localCamera = GetComponentInParent<Camera>();
    }

    void Update()
    {
        if (localCamera == null) return;

        Ray ray = localCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 spherePosition = ray.GetPoint(sphereDistance);
        transform.position = spherePosition;
    }
}

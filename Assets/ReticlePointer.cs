using UnityEngine;

public class ReticlePointer : MonoBehaviour
{
    [Header("References")]
    public Transform character;  

    [Header("Settings")]
    [Tooltip("Distance from the camera where the sphere reticle should be placed.")]
    public float sphereDistance = 10f;

    private Camera cam;

    void Awake()
    {
        cam = Camera.main;
    }

    void Update()
    {
        // Create a ray from the center of the screen.
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        // Calculate the position along the ray at sphereDistance.
        Vector3 spherePosition = ray.GetPoint(sphereDistance);
        // Set the sphere's position.
        transform.position = spherePosition;
    }
}

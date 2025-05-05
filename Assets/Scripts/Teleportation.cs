using UnityEngine;
using System.Collections;

public class TeleportationManager : MonoBehaviour
{
    [Header("XR Rig Reference")]
    [Tooltip("The XR Rig transform that should be teleported.")]
    public Transform xrRig;

    [Header("Teleport Destinations")]
    [Tooltip("Assign destination objects from each room here.")]
    public Transform[] teleportDestinations;

    [Tooltip("Optional vertical offset applied to the teleport destination.")]
    public float verticalOffset = 0f;

    // Index for cycling through destinations.
    private int currentIndex = 0;

    void Update()
    {
        // When Y is pressed, teleport to the next destination.
        if (Input.GetKeyDown(KeyCode.Y) || Input.GetKeyDown(KeyCode.JoystickButton5))
        {
            TeleportToNextDestination();
        }
    }

    void TeleportToNextDestination()
    {
        if (teleportDestinations == null || teleportDestinations.Length == 0)
        {
            Debug.LogWarning("No teleport destinations assigned.");
            return;
        }

        // Get the current destination.
        Vector3 destination = teleportDestinations[currentIndex].position;
        destination.y += verticalOffset;  // Apply vertical offset if needed.

        // Teleport the XR Rig.
        if (xrRig != null)
        {
            xrRig.position = destination;
            Debug.Log("Teleported XR Rig to: " + destination);
        }
        else
        {
            Debug.LogWarning("XR Rig transform is not assigned!");
        }

        // Cycle to the next destination.
        currentIndex = (currentIndex + 1) % teleportDestinations.Length;
    }
}

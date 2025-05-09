using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class RobotStartCanvasController : MonoBehaviour
{
    [Header("Target Robots")]
    public Transform[] robotTransforms; // Drag RobotLivingRoom, RobotBedRoom, RobotBathRoom

    [Header("Canvas Setup")]
    public GameObject canvasPrefab; // Drag RobotStartCanvas prefab here
    public Vector3 offset = new Vector3(0f, 1.8f, 0f);
    public Vector3 canvasRotationEuler = new Vector3(0f, 90f, 0f); // Rotate canvas 90 degrees on Y
    public string defaultMessage = "Hey there! I’m DormE 😊 Come closer and press Y to chat with me!";

    private Dictionary<Transform, GameObject> robotCanvasMap = new Dictionary<Transform, GameObject>();
    private Dictionary<Transform, TextMeshProUGUI> robotTextMap = new Dictionary<Transform, TextMeshProUGUI>();

    void Start()
    {
        
        foreach (Transform robot in robotTransforms)
        {
            if (robot == null) continue;

            // Custom rotation: 90° for bathroom robot
            Vector3 rotationEuler = canvasRotationEuler;
            if (robot.name == "RobotBathRoom")
                rotationEuler = new Vector3(0f, 180f, 0f); // Rotate bathroom canvas

            GameObject canvasInstance = Instantiate(canvasPrefab, robot.position + offset, Quaternion.Euler(rotationEuler));
            canvasInstance.transform.SetParent(robot, true); // Optional: follow robot
            canvasInstance.SetActive(true);

            TextMeshProUGUI greetingText = canvasInstance.GetComponentInChildren<TextMeshProUGUI>();
            if (greetingText != null)
                greetingText.text = defaultMessage;

            robotCanvasMap[robot] = canvasInstance;
            robotTextMap[robot] = greetingText;
        }

    }

    // Called when chat opens
    public void HideGreetingCanvas(Transform robot)
    {
        if (robotCanvasMap.ContainsKey(robot))
            robotCanvasMap[robot].SetActive(false);
    }

    // Called when chat closes
    public void ShowGreetingCanvas(Transform robot)
    {
        if (robotCanvasMap.ContainsKey(robot))
        {
            robotCanvasMap[robot].SetActive(true);
            DisplayTemporaryMessage(robot, "Thanks for spending time with me!", 3f);
        }
    }

    public void DisplayTemporaryMessage(Transform robot, string message, float duration)
    {
        if (robotTextMap.ContainsKey(robot))
        {
            StopAllCoroutines();
            robotTextMap[robot].text = message;
            StartCoroutine(RestoreMessageAfterDelay(robot, duration));
        }
    }

    private IEnumerator RestoreMessageAfterDelay(Transform robot, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (robotTextMap.ContainsKey(robot))
            robotTextMap[robot].text = defaultMessage;
    }
}

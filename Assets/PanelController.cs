using UnityEngine;
using TMPro;
using System.Collections;

public class PanelController : MonoBehaviour
{
    [Header("UI Panel to control")]
    public GameObject panel;

    [Header("Camera Reference")]
    public Camera userCamera;

    [Header("Distance from Camera")]
    public float distanceInFront = 1f;

    [Header("Gemini Handler (for screenshot/feedback)")]
    public UnityAndGeminiV3 geminiHandler;

    [Header("Hint Text")]
    public TMP_Text hintText;
    public string defaultMessage = "Press N for design feedback or M for room analysis.";

    private bool panelIsActive = false;

    void Start()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }

        if (userCamera == null)
        {
            userCamera = Camera.main;
        }

        if (hintText != null)
        {
            hintText.text = defaultMessage;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (panel.activeSelf)
            {
                panel.SetActive(false);
                panelIsActive = false;
            }
            else
            {
                PositionPanelInFrontOfCamera();
                panel.SetActive(true);
                panelIsActive = true;

                if (hintText != null)
                {
                    hintText.text = defaultMessage;
                }
            }
        }

        if ((Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.N)) && panelIsActive)
        {
            string action = Input.GetKeyDown(KeyCode.M) ? "room" : "feedback";
            StartCoroutine(TemporarilyHidePanelAndExecute(action));

            if (hintText != null)
            {
                hintText.text = "";
            }
        }
    }

    private void PositionPanelInFrontOfCamera()
    {
        if (panel != null && userCamera != null)
        {
            panel.transform.position = userCamera.transform.position + userCamera.transform.forward * distanceInFront;
            panel.transform.rotation = userCamera.transform.rotation;
        }
    }

    private IEnumerator TemporarilyHidePanelAndExecute(string actionType)
    {
        panel.SetActive(false);
        yield return new WaitForEndOfFrame();

        if (geminiHandler != null)
        {
            if (actionType == "room")
                geminiHandler.SendScreenshotToGeminiForRoomCommentary();
            else if (actionType == "feedback")
                geminiHandler.TriggerDesignFeedbackPopup();
        }

        yield return new WaitForSeconds(1f); // Wait for popup to appear before showing panel again
        PositionPanelInFrontOfCamera();
        panel.SetActive(true);
    }
}
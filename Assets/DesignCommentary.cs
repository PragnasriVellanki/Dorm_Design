using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

[System.Serializable]
public class Delta
{
    public string content;
}

[System.Serializable]
public class Choice
{
    public Delta delta;
    public string finish_reason;
    public int index;
}

[System.Serializable]
public class LlamaResponse
{
    public Choice[] choices;
    public int created;
    public string id;
    public string model;
}

public class DesignCommentary : MonoBehaviour
{
    [Header("API Settings")]
    [Tooltip("API endpoint for the commentary service.")]
    public string apiUrl = "https://api.sambanova.ai/v1/chat/completions";
    [Tooltip("API key.")]
    public string apiKey = "99da0e1d-db0e-4a43-85d6-59a5aa2322fd";

    [Header("Commentary Prompt Settings")]
    [Tooltip("The prompt to send to the API asking for a one-sentence commentary on the design.")]
    public string commentaryPrompt = "What do you see in this image?";

    [Header("UI Settings")]
    [Tooltip("TextMeshProUGUI element to display the commentary.")]
    public TextMeshProUGUI commentaryTMP;

    [Tooltip("Optional delay in seconds after the frame ends before capturing the screenshot.")]
    public float captureDelay = 0.1f;

    void Update()
    {
        // Press N to capture a screenshot and request design commentary.
        if (Input.GetKeyDown(KeyCode.N))
        {
            StartCoroutine(CaptureAndGetCommentary());
        }
    }

    IEnumerator CaptureAndGetCommentary()
    {
        // Wait for the end of the frame and an additional delay.
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(captureDelay);

        // Capture the screenshot as a Texture2D.
        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
        if (screenshot == null)
        {
            Debug.LogError("Screenshot capture failed!");
            yield break;
        }
        Debug.Log("Screenshot captured.");

        // Send the screenshot to the API.
        yield return StartCoroutine(GetCommentary(commentaryPrompt, screenshot, OnCommentaryReceived));

        // Clean up the screenshot texture.
        Destroy(screenshot);
    }

    IEnumerator GetCommentary(string prompt, Texture2D image, System.Action<string> callback)
    {
        if (image == null)
        {
            Debug.LogError("No image provided for commentary!");
            yield break;
        }

        // Convert the image to JPEG data and encode it to Base64.
        byte[] imageData = image.EncodeToJPG(80);
        string base64Image = System.Convert.ToBase64String(imageData);
        Debug.Log("Base 64 Image: " + base64Image);

        // Build the JSON payload using the structure from your curl code.
        string jsonPayload =
            "{" +
                "\"stream\": true, " +
                "\"model\": \"Llama-3.2-11B-Vision-Instruct\", " +
                "\"messages\": [" +
                    "{" +
                        "\"role\": \"user\", " +
                        "\"content\": [" +
                            "{" +
                                "\"type\": \"text\", " +
                                "\"text\": \"" + prompt + "\"" +
                            "}," +
                            "{" +
                                "\"type\": \"image_url\", " +
                                "\"image_url\": {" +
                                    "\"url\": \"data:image/jpeg;base64," + base64Image + "\"" +
                                "}" +
                            "}" +
                        "]" +
                    "}" +
                "]" +
            "}";

        Debug.Log("Payload: " + jsonPayload);

        // Create the UnityWebRequest.
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Request failed: " + request.error);
            callback("Error: " + request.error);
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            Debug.Log("Received response: " + jsonResponse);

            LlamaResponse response = JsonUtility.FromJson<LlamaResponse>(jsonResponse);
            if (response != null && response.choices != null && response.choices.Length > 0)
            {
                string content = response.choices[0].delta.content;
                callback(content);
            }
            else
            {
                callback("Error: Failed to parse response.");
            }
        }
    }

    void OnCommentaryReceived(string commentary)
    {
        Debug.Log("Design Commentary: " + commentary);
        if (commentaryTMP != null)
        {
            commentaryTMP.text = "Design Commentary:\n" + commentary;
        }
        else
        {
            Debug.LogWarning("Commentary TextMeshProUGUI element is not assigned.");
        }
    }
}

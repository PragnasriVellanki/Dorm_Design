using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using TMPro;
using System.IO;
using System;
using System.Linq;


[System.Serializable]
public class UnityAndGeminiKey { public string key; }

[System.Serializable]
public class InlineData { public string mimeType; public string data; }

[System.Serializable]
public class TextPart { public string text; }

[System.Serializable]
public class ImagePart { public string text; public InlineData inlineData; }

[System.Serializable]
public class TextContent { public string role; public TextPart[] parts; }

[System.Serializable]
public class TextCandidate { public TextContent content; }

[System.Serializable]
public class TextResponse { public TextCandidate[] candidates; }

[System.Serializable]
public class ImageContent { public string role; public ImagePart[] parts; }

[System.Serializable]
public class ImageCandidate { public ImageContent content; }

[System.Serializable]
public class ImageResponse { public ImageCandidate[] candidates; }

[System.Serializable]
public class ChatRequest { public TextContent[] contents; }

public class UnityAndGeminiV3 : MonoBehaviour
{
    [Header("JSON API Configuration")]
    public TextAsset jsonApi;

    private string apiKey = "";
    private string apiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
    private string imageEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-exp-image-generation:generateContent";

    [Header("ChatBot Function")]
    public TMP_InputField inputField;
    public TMP_Text uiText;
    private TextContent[] chatHistory;

    [Header("Prompt Function")]
    public string prompt = "";

    [Header("Image Prompt Function")]
    public string imagePrompt = "";
    public Material skyboxMaterial;

    [Header("Popup UI")]
    public GameObject responsePopup;
    public TMP_Text responsePopupText;
    private Camera mainCamera;

    [Header("Inventory Reference")]
    public AdvancedInventoryManager inventoryManager;
    private List<string> allInventoryObjectNames = new List<string>();
    private string currentDetectedRoom = "";

    [Header("Robot Placement for Popup")]
    public Transform robotTransform;
    public Vector3 openOffset = new Vector3(-1.4f, -0.9f, -0.4f);
    public Vector3 scaledDownScale = new Vector3(1f, 1f, 1f);
    public Vector3 openRotationEuler = new Vector3(0f, 20f, 0f);

    private Vector3 originalRobotPosition;
    private Quaternion originalRobotRotation;
    private Vector3 originalRobotScale;
    private PlayerController playerController;
    [Header("Greeting Canvas Reference")]
    public RobotStartCanvasController startCanvasController;
    private bool isFeedbackPopupOpen = false;



    void Start()
    {
        UnityAndGeminiKey jsonApiKey = JsonUtility.FromJson<UnityAndGeminiKey>(jsonApi.text);
        apiKey = jsonApiKey.key;
        chatHistory = new TextContent[] { };

        if (!string.IsNullOrEmpty(prompt))
            StartCoroutine(SendPromptRequestToGemini(prompt));

        if (!string.IsNullOrEmpty(imagePrompt))
            StartCoroutine(SendPromptRequestToGeminiImageGenerator(imagePrompt));

        Camera[] allCameras = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera cam in allCameras)
        {
            if (cam.CompareTag("PlayerCamera") && cam.gameObject.activeInHierarchy)
            {
                mainCamera = cam;
                Debug.Log("✅ Main Camera assigned from PlayerCamera tag.");
                break;
            }
        }
        if (robotTransform != null)
        {
            originalRobotPosition = robotTransform.position;
            originalRobotRotation = robotTransform.rotation;
            originalRobotScale = robotTransform.localScale;
        }

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            playerController = playerObj.GetComponent<PlayerController>();



        CacheAllInventoryItems();

    }

    private void CacheAllInventoryItems()
    {
        if (inventoryManager == null) return;

        void AddNames(List<GameObject> list)
        {
            foreach (var obj in list)
                if (obj != null && !allInventoryObjectNames.Contains(obj.name))
                    allInventoryObjectNames.Add(obj.name);
        }

        allInventoryObjectNames.AddRange(inventoryManager.bathRoomPrefabNames);
        allInventoryObjectNames.AddRange(inventoryManager.bedRoomPrefabNames);
        allInventoryObjectNames.AddRange(inventoryManager.livingRoomPrefabNames);

    }

    private string BuildRoomCommentaryPrompt(string room, List<string> visibleObjects)
    {
        List<string> expected = new List<string>();

        if (room.Contains("bathroom")) expected = inventoryManager.bathRoomPrefabNames;
        else if (room.Contains("bedroom")) expected = inventoryManager.bedRoomPrefabNames;
        else if (room.Contains("living room") || room.Contains("livingroom")) expected = inventoryManager.livingRoomPrefabNames;


        var matched = visibleObjects.FindAll(o => expected.Contains(o));
        var missing = expected.FindAll(o => !visibleObjects.Contains(o));

        return $"The user is in the {room}. You can interact with: {string.Join(", ", matched)}. " +
               $"You don't yet see: {string.Join(", ", missing)}. " +
               $"Make a short design commentary — limit your response to 4 sentences. " +
               $"Mention at least one good thing and one improvement suggestion.";
    }


    public void TriggerDesignFeedbackPopup()
    {

        string designPrompt = "Provide a one-sentence commentary on this design. Mention one thing that's really good and one thing that's really bad.";
        StartCoroutine(SendPromptRequestToGemini(designPrompt, true));
    }

    public IEnumerator SendPromptRequestToGemini(string promptText, bool showPopup = false, System.Action<string> onResponse = null)
    {
        string url = $"{apiEndpoint}?key={apiKey}";
        string jsonData = "{\"contents\": [{\"parts\": [{\"text\": \"" + promptText + "\"}]}]}";
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);
        if (startCanvasController != null && robotTransform != null)
            startCanvasController.HideGreetingCanvas(robotTransform);

        // STEP 1: Show the popup immediately with a loading message (optional)
        if (showPopup && responsePopup != null && responsePopupText != null)
        {
            responsePopup.SetActive(false);

            Vector3 forward = mainCamera.transform.forward;
            responsePopup.transform.position = mainCamera.transform.position + forward * 2.5f;
            responsePopup.transform.rotation = Quaternion.LookRotation(forward);

            if (robotTransform != null)
            {
                Vector3 offset = mainCamera.transform.right * openOffset.x +
                                 mainCamera.transform.up * openOffset.y +
                                 mainCamera.transform.forward * openOffset.z;

                robotTransform.position = responsePopup.transform.position + offset;
                robotTransform.rotation = Quaternion.Euler(openRotationEuler);
                robotTransform.localScale = scaledDownScale;
            }

            if (playerController != null)
                playerController.isMovementLocked = true;

            responsePopupText.text = "Hold on! I'm thinking...";
            responsePopup.SetActive(true);
        }

        // STEP 2: Send Gemini request
        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            // STEP 3: Handle response
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
                string fallback = "⚠️ Oops! Something went wrong.";

                if (showPopup && responsePopupText != null)
                    responsePopupText.text = fallback;

                onResponse?.Invoke(fallback);
            }
            else
            {
                TextResponse response = JsonUtility.FromJson<TextResponse>(www.downloadHandler.text);

                if (response.candidates.Length > 0 && response.candidates[0].content.parts.Length > 0)
                {
                    string text = response.candidates[0].content.parts[0].text;
                    Debug.Log("[Gemini ✅ Response] " + text);

                    if (showPopup && responsePopupText != null)
                        responsePopupText.text = text;

                    onResponse?.Invoke(text);
                }
                else
                {
                    string fallback = "🤖 Hmm... I couldn’t come up with a response.";

                    if (showPopup && responsePopupText != null)
                        responsePopupText.text = fallback;

                    onResponse?.Invoke(fallback);
                }
            }
        }
    }


    public List<string> GetExpectedRoomPrefabNames(string room)
{
    if (room == "bedroom")
        return new List<string>(inventoryManager.bedRoomPrefabNames);
    else if (room == "bathroom")
        return new List<string>(inventoryManager.bathRoomPrefabNames);
    else if (room == "living room")
        return new List<string>(inventoryManager.livingRoomPrefabNames);
    else
        return new List<string>();
}



    public string BuildRoomInventoryPrompt(string room, List<string> currentItems, bool isAddPrompt)
    {
        // 1. Get expected prefab names for the room
        List<string> expectedRaw = new List<string>();

        if (room == "bedroom") expectedRaw = inventoryManager.bedRoomPrefabNames;
        else if (room == "bathroom") expectedRaw = inventoryManager.bathRoomPrefabNames;
        else if (room == "living room") expectedRaw = inventoryManager.livingRoomPrefabNames;


        Debug.Log($"✅ Prefabs for room [{room}]: {string.Join(", ", expectedRaw)}");
        Debug.Log($"📦 Current Placed Items: {string.Join(", ", currentItems)}");

     

        // 3. Build prompt string
        if (isAddPrompt)
        {
            return $"The user is designing a {room}. They've already added: {string.Join(", ", currentItems)}. " +
                   $"They haven't yet added: {string.Join(", ", expectedRaw)}. " +
                   $"Please give 2-3 helpful suggestions on what to add to complete the room. Use general names like 'bed' or 'lamp' instead of object file names." +
                   $"Keep it really short like 1-4 sentences. Avoid bullet points and the use of **.";
        }
        else
        {
            if (currentItems.Count == 0)
            {
                return $"Since there are no items in the room yet, there's nothing to remove or replace. The player should start placing furniture first.";
            }

            return $"The user is designing a {room}. They've added: {string.Join(", ", currentItems)}. " +
                   $"Please give 2-3 suggestions for what could be removed, replaced, or reorganized. Use general terms, not file names." +
                   $"Keep it really short like 1-4 sentences. Avoid bullet points and the use of **.";
        }
    }



    public void SendChat()
    {
        string userMessage = inputField.text;
        StartCoroutine(SendChatRequestToGemini(userMessage));
    }

    private IEnumerator SendChatRequestToGemini(string newMessage)
    {
        string url = $"{apiEndpoint}?key={apiKey}";

        TextContent userContent = new TextContent
        {
            role = "user",
            parts = new TextPart[] { new TextPart { text = newMessage } }
        };

        List<TextContent> contentsList = new List<TextContent>(chatHistory);
        contentsList.Add(userContent);
        chatHistory = contentsList.ToArray();

        ChatRequest chatRequest = new ChatRequest { contents = chatHistory };
        string jsonData = JsonUtility.ToJson(chatRequest);
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                TextResponse response = JsonUtility.FromJson<TextResponse>(www.downloadHandler.text);
                if (response.candidates.Length > 0 && response.candidates[0].content.parts.Length > 0)
                {
                    string reply = response.candidates[0].content.parts[0].text;
                    uiText.text = reply;

                    TextContent botContent = new TextContent
                    {
                        role = "model",
                        parts = new TextPart[] { new TextPart { text = reply } }
                    };
                    contentsList.Add(botContent);
                    chatHistory = contentsList.ToArray();
                }
            }
        }
    }

    public void ClosePopup()
    {
        if (responsePopup != null)
            responsePopup.SetActive(false);

        if (robotTransform != null)
        {
            robotTransform.position = originalRobotPosition;
            robotTransform.rotation = originalRobotRotation;
            robotTransform.localScale = originalRobotScale;
        }
        if (startCanvasController != null && robotTransform != null)
            startCanvasController.ShowGreetingCanvas(robotTransform);

        if (playerController != null)
            playerController.isMovementLocked = false;
    }




    private IEnumerator SendPromptRequestToGeminiImageGenerator(string promptText)
    {
        string url = $"{imageEndpoint}?key={apiKey}";

        string jsonData = $@"{{
            ""contents"": [{{
                ""parts"": [{{
                    ""text"": ""{promptText}""
                }}]
            }}],
            ""generationConfig"": {{
                ""responseModalities"": [""Text"", ""Image""]
            }}
        }}";

        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log("Request complete!");
                Debug.Log("Full response: " + www.downloadHandler.text);

                try
                {
                    ImageResponse response = JsonUtility.FromJson<ImageResponse>(www.downloadHandler.text);

                    if (response.candidates != null && response.candidates.Length > 0 &&
                        response.candidates[0].content != null &&
                        response.candidates[0].content.parts != null)
                    {
                        foreach (var part in response.candidates[0].content.parts)
                        {
                            if (!string.IsNullOrEmpty(part.text))
                            {
                                Debug.Log("Text response: " + part.text);
                            }
                            else if (part.inlineData != null && !string.IsNullOrEmpty(part.inlineData.data))
                            {
                                byte[] imageBytes = System.Convert.FromBase64String(part.inlineData.data);

                                Texture2D tex = new Texture2D(2, 2);
                                tex.LoadImage(imageBytes);
                                byte[] pngBytes = tex.EncodeToPNG();
                                string path = Application.persistentDataPath + "/gemini-image.png";
                                File.WriteAllBytes(path, pngBytes);
                                Debug.Log("Saved to: " + path);

                                string imagePath = Path.Combine(Application.persistentDataPath, "gemini-image.png");
                                Texture2D panoramaTex = new Texture2D(2, 2);
                                panoramaTex.LoadImage(File.ReadAllBytes(imagePath));
                                Texture2D properlySizedTex = ResizeTexture(panoramaTex, 1024, 512);

                                if (skyboxMaterial != null)
                                {
                                    skyboxMaterial.shader = Shader.Find("Skybox/Panoramic");
                                    skyboxMaterial.SetTexture("_MainTex", properlySizedTex);
                                    DynamicGI.UpdateEnvironment();
                                    Debug.Log("Skybox updated with panoramic image!");
                                }
                                else
                                {
                                    Debug.LogError("Skybox material not assigned!");
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("No valid response parts found.");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("JSON Parse Error: " + e.Message);
                }
            }
        }
    }

    private Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        Graphics.Blit(source, rt);
        Texture2D result = new Texture2D(newWidth, newHeight);
        RenderTexture.active = rt;
        result.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        result.Apply();
        RenderTexture.ReleaseTemporary(rt);
        return result;
    }

    public void SendScreenshotToGeminiForRoomCommentary()
    {
        // Immediately open popup with "Capturing..." before yield starts
        if (responsePopup != null && responsePopupText != null)
        {
            responsePopup.SetActive(false); // Hide before positioning

            Vector3 forward = mainCamera.transform.forward;
            responsePopup.transform.position = mainCamera.transform.position + forward * 2.5f;
            responsePopup.transform.rotation = Quaternion.LookRotation(forward);

            responsePopupText.text = "Capturing your setup... Please wait!";
            responsePopup.SetActive(true);  // Show after transform update

        }

        StartCoroutine(SendScreenshotToDetectRoom());
    }

    private IEnumerator SendScreenshotToDetectRoom()
    {
        yield return new WaitForEndOfFrame();

        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
        string base64Image = Convert.ToBase64String(screenshot.EncodeToPNG());
        UnityEngine.Object.Destroy(screenshot);

        string url = $"{apiEndpoint}?key={apiKey}";
        string prompt = "Which room is in this image? Choose only from: bedroom, bathroom, living room.";

        string jsonData = $@"
    {{
        ""contents"": [{{
            ""parts"": [
                {{
                    ""inlineData"": {{
                        ""mimeType"": ""image/png"",
                        ""data"": ""{base64Image}""
                    }}
                }},
                {{
                    ""text"": ""{prompt}""
                }}
            ]
        }}]
    }}";

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(new System.Text.UTF8Encoding().GetBytes(jsonData));
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
                yield break;
            }

            TextResponse response = JsonUtility.FromJson<TextResponse>(www.downloadHandler.text);
            if (response.candidates.Length == 0) yield break;

            currentDetectedRoom = response.candidates[0].content.parts[0].text.ToLower().Trim();
            Debug.Log("🛏️ Detected Room: " + currentDetectedRoom);

            StartCoroutine(SendScreenshotToDetectObjects());
        }
    }

    private IEnumerator SendScreenshotToDetectObjects()
    {
        yield return new WaitForEndOfFrame();

        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
        string base64Image = Convert.ToBase64String(screenshot.EncodeToPNG());
        UnityEngine.Object.Destroy(screenshot);

        string url = $"{apiEndpoint}?key={apiKey}";
        string prompt = "List the names of any recognizable furniture or decor objects in the uploaded room image.";

        string jsonData = $@"
    {{
        ""contents"": [{{
            ""parts"": [
                {{
                    ""inlineData"": {{
                        ""mimeType"": ""image/png"",
                        ""data"": ""{base64Image}""
                    }}
                }},
                {{
                    ""text"": ""{prompt}""
                }}
            ]
        }}]
    }}";

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(new System.Text.UTF8Encoding().GetBytes(jsonData));
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
                yield break;
            }

            TextResponse response = JsonUtility.FromJson<TextResponse>(www.downloadHandler.text);
            if (response.candidates.Length == 0) yield break;

            string objectsText = response.candidates[0].content.parts[0].text;
            Debug.Log("🪑 Detected Objects: " + objectsText);

            List<string> objects = new List<string>(objectsText.Split(','));
            for (int i = 0; i < objects.Count; i++) objects[i] = objects[i].Trim();

            string summaryPrompt = BuildRoomCommentaryPrompt(currentDetectedRoom, objects);
            StartCoroutine(SendPromptRequestToGemini(summaryPrompt, true));
        }
    }


    private IEnumerator SendScreenshotPrompt(string promptText)
    {
        yield return new WaitForEndOfFrame();

        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
        byte[] imageBytes = screenshot.EncodeToPNG();
        string base64Image = Convert.ToBase64String(imageBytes);
        string mimeType = "image/png";
        UnityEngine.Object.Destroy(screenshot);

        string url = $"{apiEndpoint}?key={apiKey}";

        string jsonData = $@"
        {{
            ""contents"": [{{
                ""parts"": [
                    {{
                        ""inlineData"": {{
                            ""mimeType"": ""{mimeType}"",
                            ""data"": ""{base64Image}""
                        }}
                    }},
                    {{
                        ""text"": ""{promptText}""
                    }}
                ]
            }}]
        }}";

        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                TextResponse response = JsonUtility.FromJson<TextResponse>(www.downloadHandler.text);
                if (response.candidates.Length > 0 && response.candidates[0].content.parts.Length > 0)
                {
                    string text = response.candidates[0].content.parts[0].text;
                    Debug.Log("Screenshot-to-text result: " + text);

                    if (responsePopup != null && responsePopupText != null)
                    {
                        responsePopupText.text = text;
                        responsePopup.SetActive(true);
                    }
                }
                else
                {
                    Debug.Log("No text found in screenshot response.");
                }
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N) || Input.GetKeyDown(KeyCode.JoystickButton11))
        {
            if (isFeedbackPopupOpen)
            {
                ClosePopup();
            }
            else
            {
                SendScreenshotToGeminiForRoomCommentary();
            }

            isFeedbackPopupOpen = !isFeedbackPopupOpen;
        }
    }


}
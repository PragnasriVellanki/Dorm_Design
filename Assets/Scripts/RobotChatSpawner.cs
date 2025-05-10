using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;


public class RobotChatSpawner : MonoBehaviour
{
    [Header("Chat Setup")]
    public GameObject robotChatCanvas;
    public GameObject questionPanel;
    public GameObject answerPanel;
    public TextMeshProUGUI answerText;

    [Header("Buttons")]
    public Button questionButton1;
    public Button questionButton2;
    public Button questionButton3;
    public Button questionButton4;
    public Button backButton;

    [Header("Settings")]
    public float verticalOffset = 1.5f;
    public float selectableDistance = 10f;

    

    public UnityAndGeminiV3 geminiAgent;
    public AdvancedInventoryManager inventoryManager;

    [Header("Room Robot References")]
    public Transform bedRobot;
    public Transform bathRobot;
    public Transform livingRobot;

    private Camera cam;
    private Transform cameraTransform;
    private PlayerController playerController;
    private GameObject currentRobot = null;
    private Transform currentRobotTransform = null;
    private bool chatJustOpened = false;
    private bool isMenuOpen = false;
    private int currentIndex = 0;
    private Button[] questionButtons;

    [Header("Robot Movement")]
    public Vector3 openOffset = new Vector3(-1.5f, 0, 0);
    public Vector3 scaledDownScale = new Vector3(0.5f, 0.5f, 0.5f);
    private Vector3 originalRobotPosition;
    private Vector3 originalRobotScale;
    private Quaternion originalRobotRotation;
    public Vector3 openRotationEuler = new Vector3(0f, -30f, 0f);
    private float navCooldown = 0.25f;
    private float lastNavTime = 0f;

    void Start()
    {
        Camera[] allCameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera c in allCameras)
        {
            if (c.CompareTag("PlayerCamera") && c.gameObject.activeInHierarchy)
            {
                cam = c;
                cameraTransform = c.transform;
                break;
            }
        }

        

        if (robotChatCanvas != null)
        {
            Canvas canvas = robotChatCanvas.GetComponent<Canvas>();
            if (canvas != null && cam != null)
                canvas.worldCamera = cam;

            robotChatCanvas.SetActive(false);
        }

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            playerController = playerObj.GetComponent<PlayerController>();

        questionButtons = new Button[] { questionButton1, questionButton2, questionButton3, questionButton4 };

        questionButton1.onClick.AddListener(() =>
        {
            ShowAnswer("Hi! I’m DormE, your intelligent assistant here in the Dorm Design experience. If you'd like feedback or suggestions based on the room you've set up, just face the furniture you've placed so the room is clearly visible, then press the Options button to ask me! When you're done, you can press B to close the chat. Thank you and happy designing!");
        });

        questionButton2.onClick.RemoveAllListeners();
        questionButton2.onClick.AddListener(() =>
        {
            string room = GetRoomFromRobotTag(currentRobot);
            List<string> currentItems = inventoryManager.GetAllPlacedObjectNames();
            Debug.Log("[CurrentItems Raw] " + string.Join(", ", currentItems));

            string prompt = geminiAgent.BuildRoomInventoryPrompt(room, currentItems, true);
            geminiAgent.StartCoroutine(geminiAgent.SendPromptRequestToGemini(prompt, false, (response) =>
            {
                ShowAnswer(response);
            }));
        });

        questionButton3.onClick.RemoveAllListeners();
        questionButton3.onClick.AddListener(() =>
        {
            string room = GetRoomFromRobotTag(currentRobot);
            List<string> currentItems = inventoryManager.GetAllPlacedObjectNames();

            if (currentItems.Count == 0)
            {
                ShowAnswer("You haven’t added anything yet, so there's nothing to remove!");
                return;
            }

            Debug.Log("[CurrentItems Raw] " + string.Join(", ", currentItems));
            string prompt = geminiAgent.BuildRoomInventoryPrompt(room, currentItems, false);
            geminiAgent.StartCoroutine(geminiAgent.SendPromptRequestToGemini(prompt, false, (response) =>
            {
                ShowAnswer(response);
            }));
        });

        questionButton4.onClick.AddListener(() => ShowAnswer("Your dorm layout includes three cozy rooms—a Living Room, Bedroom, and Bathroom—and you can personalize each space using two main tools: the Inventory Menu to choose items, and the Object Menu to interact with them directly!"));

        backButton.onClick.AddListener(ShowQuestions);

        questionPanel.SetActive(false);
        answerPanel.SetActive(false);
    }

    void Update()
    {
        if (!isMenuOpen)
        {
            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, selectableDistance))
            {
                GameObject hitObj = hit.collider.gameObject;
                if (IsRecognizedRobot(hitObj))
                {
                    currentRobot = hitObj;
                    currentRobotTransform = hitObj.transform;
                }
                else
                {
                    currentRobot = null;
                    currentRobotTransform = null;
                }
            }
            else
            {
                currentRobot = null;
                currentRobotTransform = null;
            }
        }

        if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.JoystickButton3))
        {
            if (isMenuOpen)
                CloseChat();
            else if (currentRobot != null)
                OpenChat();
        }

        if (!isMenuOpen) return;
        


        float vertical = Input.GetAxis("Vertical");

        if (questionPanel.activeSelf && Mathf.Abs(vertical) > 0.5f && Time.time - lastNavTime > navCooldown)
        {
            currentIndex += vertical < 0 ? 1 : -1;
            currentIndex = Mathf.Clamp(currentIndex, 0, questionButtons.Length - 1);
            EventSystem.current.SetSelectedGameObject(questionButtons[currentIndex].gameObject);
            lastNavTime = Time.time;
        }


        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton2))
        {
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            selected?.GetComponent<Button>()?.onClick.Invoke();
        }
    }

    void OpenChat()
    {
        isMenuOpen = true;
        chatJustOpened = true;

        robotChatCanvas.SetActive(true);
        questionPanel.SetActive(true);
        answerPanel.SetActive(false);
        currentIndex = 0;

        Vector3 menuPos = cameraTransform.position + cameraTransform.forward * 2f;
        robotChatCanvas.transform.position = menuPos;
        robotChatCanvas.transform.rotation = Quaternion.LookRotation(menuPos - cameraTransform.position);
        RobotStartCanvasController startCanvas = Object.FindFirstObjectByType<RobotStartCanvasController>();
        if (startCanvas != null && currentRobotTransform != null)
            startCanvas.HideGreetingCanvas(currentRobotTransform);

        if (currentRobotTransform != null)
        {
            originalRobotPosition = currentRobotTransform.position;
            originalRobotRotation = currentRobotTransform.rotation;
            originalRobotScale = currentRobotTransform.localScale;

            Vector3 rightOffset = cameraTransform.right * openOffset.x +
                                  cameraTransform.up * openOffset.y +
                                  cameraTransform.forward * openOffset.z;
            currentRobotTransform.position = menuPos + rightOffset;
            currentRobotTransform.localScale = scaledDownScale;
            currentRobotTransform.rotation = Quaternion.Euler(openRotationEuler);
        }

        EventSystem.current.SetSelectedGameObject(questionButtons[0].gameObject);

        if (playerController != null)
            playerController.isMovementLocked = true;

        

        Debug.Log("Robot chat opened.");
    }

    void CloseChat()
    {
        isMenuOpen = false;
        robotChatCanvas.SetActive(false);
        questionPanel.SetActive(false);
        answerPanel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);

        if (currentRobotTransform != null)
        {
            currentRobotTransform.position = originalRobotPosition;
            currentRobotTransform.rotation = originalRobotRotation;
            currentRobotTransform.localScale = originalRobotScale;
        }

        if (playerController != null)
            playerController.isMovementLocked = false;

        if (playerController != null)
            playerController.isMovementLocked = false;

        RobotStartCanvasController startCanvas = Object.FindFirstObjectByType<RobotStartCanvasController>();
        if (startCanvas != null && currentRobotTransform != null)
            startCanvas.ShowGreetingCanvas(currentRobotTransform);


        Debug.Log("Robot chat closed.");
    }





    void ShowAnswer(string answer)
    {
        questionPanel.SetActive(false);
        answerPanel.SetActive(true);
        answerText.text = answer;
        EventSystem.current.SetSelectedGameObject(backButton.gameObject);
    }

    void ShowQuestions()
    {
        answerPanel.SetActive(false);
        questionPanel.SetActive(true);
        currentIndex = 0;
        EventSystem.current.SetSelectedGameObject(questionButtons[0].gameObject);
    }

    private string GetRoomFromRobotTag(GameObject robot)
    {
        if (robot == null)
        {
            Debug.LogWarning("Robot is null");
            return "unknown";
        }

        string tag = robot.tag.ToLower();
        Debug.Log("GetRoomFromRobotTag: " + tag);

        if (tag.Contains("living")) return "living room";
        if (tag.Contains("bath")) return "bathroom";
        if (tag.Contains("bed")) return "bedroom";

        return "unknown";
    }

    private bool IsRecognizedRobot(GameObject obj)
    {
        string tag = obj.tag.ToLower();
        return tag.Contains("robotliving") || tag.Contains("robotbath") || tag.Contains("robotbed");
    }

}

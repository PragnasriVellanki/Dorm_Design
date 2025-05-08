using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

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
    public string robotTag = "Robot";

    [Header("Greeting Canvas")]
    public GameObject robotGreetingCanvas;


    private Camera cam;
    private Transform cameraTransform;
    private PlayerController playerController;
    private GameObject currentRobot = null;

    private bool chatJustOpened = false;
    private bool isMenuOpen = false;
    private float inputCooldown = 0f;
    private int currentIndex = 0;
    private Button[] questionButtons;
    private TextMeshProUGUI greetingText;
    private string originalGreeting = "Hey there! I’m DormE 😊Come closer and press Y to chat with me!";
    private Coroutine restoreGreetingCoroutine;
    [Header("Robot Movement")]
    public Transform robotTransform;           // Drag your robot model here in Inspector
    public Vector3 openOffset = new Vector3(-1.5f, 0, 0); // Adjust as needed
    private Vector3 originalRobotPosition;
    private Vector3 originalRobotScale;
    public Vector3 scaledDownScale = new Vector3(0.5f, 0.5f, 0.5f); // Or tweak as needed
    private Quaternion originalRobotRotation;
    public Vector3 openRotationEuler = new Vector3(0f, -30f, 0f); // Adjust as needed
    private GameObject lastInteractedRobot;

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
        if (robotGreetingCanvas != null)
        {
            greetingText = robotGreetingCanvas.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (robotChatCanvas != null)
        {
            Canvas canvas = robotChatCanvas.GetComponent<Canvas>();
            if (canvas != null && cam != null)
                canvas.worldCamera = cam;

            robotChatCanvas.SetActive(false);
        }
        if (robotTransform != null)
            originalRobotPosition = robotTransform.position;
        if (robotTransform != null)
            originalRobotScale = robotTransform.localScale;
        if (robotTransform != null)
            originalRobotRotation = robotTransform.rotation;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            playerController = playerObj.GetComponent<PlayerController>();

        questionButtons = new Button[] { questionButton1, questionButton2, questionButton3, questionButton4 };

        questionButton1.onClick.AddListener(() => ShowAnswer("Hi! I’m DormE, your intelligent assistant in the Dorm Design application. I can explain how everything works, help you navigate the interface, and show you what each component does. Once you've finished arranging your furniture, I can capture your final room setup and provide helpful feedback or suggestions to improve your design."));
        questionButton2.onClick.AddListener(() => ShowAnswer("💡 I can answer your questions and assist you."));
        questionButton3.onClick.AddListener(() => ShowAnswer("🎮 Point at me and press Y or R to interact."));
        questionButton4.onClick.AddListener(() => ShowAnswer("👋 Goodbye!"));
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
                if (hit.collider.CompareTag(robotTag))
                {
                    currentRobot = hit.collider.gameObject;
                }
                else
                {
                    currentRobot = null;
                }
            }
            else
            {
                currentRobot = null;
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

        if (inputCooldown > 0f)
        {
            inputCooldown -= Time.unscaledDeltaTime;
            return;
        }

        float vertical = Input.GetAxis("Vertical");

        if (questionPanel.activeSelf && Mathf.Abs(vertical) > 0.5f)
        {
            currentIndex += vertical < 0 ? 1 : -1;
            currentIndex = Mathf.Clamp(currentIndex, 0, questionButtons.Length - 1);
            EventSystem.current.SetSelectedGameObject(questionButtons[currentIndex].gameObject);
            inputCooldown = 0.3f;
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

        // 👉 Move robot to the side of the menu based on camera orientation
        if (robotTransform != null)
        {
            Vector3 rightOffset = cameraTransform.right * openOffset.x +
                                  cameraTransform.up * openOffset.y +
                                  cameraTransform.forward * openOffset.z;
            robotTransform.position = menuPos + rightOffset;
        }
        if (robotTransform != null)
            robotTransform.localScale = scaledDownScale;
        if (robotTransform != null)
            robotTransform.rotation = Quaternion.Euler(openRotationEuler);



        EventSystem.current.SetSelectedGameObject(questionButtons[0].gameObject);

        if (playerController != null)
            playerController.isMovementLocked = true;

        // 👇 Disable greeting canvas if assigned
        if (robotGreetingCanvas != null)
            robotGreetingCanvas.SetActive(false);
        

        Debug.Log("🤖 Robot chat opened.");
    }


    void CloseChat()
    {
        isMenuOpen = false;
        robotChatCanvas.SetActive(false);
        questionPanel.SetActive(false);
        answerPanel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        
        robotTransform.position = originalRobotPosition;

        if (playerController != null)
            playerController.isMovementLocked = false;
        if (robotTransform != null)
            robotTransform.position = originalRobotPosition;
        if (robotTransform != null)
            robotTransform.rotation = originalRobotRotation;

        // 👇 Re-enable greeting canvas and show "Thank you!" briefly
        if (robotGreetingCanvas != null && greetingText != null)
        {
            robotGreetingCanvas.SetActive(true);

            if (restoreGreetingCoroutine != null)
                StopCoroutine(restoreGreetingCoroutine);

            greetingText.text = "Thanks for spending time with me! I had a great chat—hope you had fun too!";
            restoreGreetingCoroutine = StartCoroutine(RestoreGreetingTextAfterDelay(3f));
        }
        if (robotTransform != null)
            robotTransform.localScale = originalRobotScale;

        Debug.Log("❌ Robot chat closed.");
    }
    IEnumerator RestoreGreetingTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (greetingText != null)
            greetingText.text = originalGreeting;
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
}

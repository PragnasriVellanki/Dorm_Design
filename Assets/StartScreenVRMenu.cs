using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartScreenVRMenu : MonoBehaviour
{
    public Button[] menuButtons; // Assign buttons in Inspector
    private int selectedIndex = 0;
    private bool inputReady = true;

    void Start()
    {
        HighlightSelected();
    }

    void Update()
    {
        HandleNavigation();
        HandleSelection();
    }

    void HandleNavigation()
    {
        float vertical = Input.GetAxis("Vertical");

        bool moveUp = vertical > 0.5f || Input.GetKeyDown(KeyCode.UpArrow);
        bool moveDown = vertical < -0.5f || Input.GetKeyDown(KeyCode.DownArrow);

        if (inputReady)
        {
            if (moveDown)
            {
                selectedIndex++;
                inputReady = false;
                Invoke(nameof(ResetInput), 0.2f);
            }
            else if (moveUp)
            {
                selectedIndex--;
                inputReady = false;
                Invoke(nameof(ResetInput), 0.2f);
            }

            selectedIndex = Mathf.Clamp(selectedIndex, 0, menuButtons.Length - 1);
            HighlightSelected();
        }
    }

    void HandleSelection()
    {
        bool joystickOK = Input.GetKeyDown(KeyCode.JoystickButton3) || Input.GetKeyDown(KeyCode.JoystickButton4);
        bool keyboardOK = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space);

        if (joystickOK || keyboardOK)
        {
            if (selectedIndex == 0)
            {
                // Only one button now: Design Room
                PhotonConnectionManager.Instance.ConnectToPhotonAndStart();
            }
        }
    }



    void HighlightSelected()
    {
        for (int i = 0; i < menuButtons.Length; i++)
        {
            ColorBlock colors = menuButtons[i].colors;
            if (i == selectedIndex)
            {
                colors.normalColor = Color.yellow; // Highlight current selection
            }
            else
            {
                colors.normalColor = Color.white; // Normal
            }
            menuButtons[i].colors = colors;
        }
    }

    void ResetInput()
    {
        inputReady = true;
    }
}

using UnityEngine;
using UnityEngine.UI;

public class StartScreenVRMenu : MonoBehaviour
{
    public Button designRoomButton; // Assign in Inspector
    private bool inputReady = true;

    void Start()
    {
        HighlightButton();
    }

    void Update()
    {
        if (!inputReady) return;

        HandleSelection();
    }

    void HandleSelection()
    {
        // JS 0 = A | JS 7 = Start | Keyboard: Enter or Space
        bool joystickOK = Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.JoystickButton7);
        bool keyboardOK = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space);

        if (joystickOK || keyboardOK)
        {
            inputReady = false;
            PhotonConnectionManager.Instance.ConnectToPhotonAndStart();
        }
    }

    void HighlightButton()
    {
        if (designRoomButton != null)
        {
            ColorBlock colors = designRoomButton.colors;
            colors.normalColor = Color.yellow;
            designRoomButton.colors = colors;
        }
    }
}

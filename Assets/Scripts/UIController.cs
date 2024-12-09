using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class meant for controlling the Game UI in accordance to the game.
/// </summary>
public class UIController : MonoBehaviour
{
    //
    //Properties
    //
    [Header("UI Elements")]
    /// <summary>
    /// UI Pause Image that gets triggered when game is paused
    /// </summary>
    [SerializeField] private Image pauseButtonImage;

    /// <summary>
    /// The text that shows the result when player gets caught or escapes
    /// </summary>
    [SerializeField] private TextMeshProUGUI endText;

    /// <summary>
    /// The background for the end text that gets turned on and off with the end text
    /// </summary>
    [SerializeField] private Image endTextBackground;

    /// <summary>
    /// The Message window for when we display in-game messages to the player, 
    /// such as when player gets close to the padlock without a key
    /// </summary>
    [SerializeField] private TextMeshProUGUI messageWindow;

    /// <summary>
    /// The Window for displaying the controls to the player at the start of the game
    /// </summary>
    [SerializeField] private TextMeshProUGUI controlsWindow;

    /// <summary>
    /// The window for displaying the current amount of keys the player has
    /// </summary>
    [SerializeField] private TextMeshProUGUI keyAmountWindow;

    /// <summary>
    /// The Game Manager Instance
    /// </summary>
    private GameManager gameManager;

    /// <summary>
    /// The seconds that the controls window is going to be on screen
    /// </summary>
    private float timeForControlsOnScreen = 20f;

    /// <summary>
    /// Amount of keys the player has, kept here as a reference as well for the UI
    /// </summary>
    private int amountOfKeys = 0;



    //Methods
    private void Start()
    {
        //Getting the game manager to read the events, so the UI can change accordingly
        gameManager = GameManager.Instance;

        //Setting up the events
        gameManager.PauseButtonPushed.AddListener(ChangePauseImageVisibility);
        gameManager.PlayerCaught.AddListener(TriggerLoseUI);
        gameManager.PlayerEscaped.AddListener(TriggerWinUI);
        gameManager.PlayerCantOpenLock.AddListener(PlayerCantOpenLock);
        gameManager.PlayerInRangeOfEscape.AddListener(PlayerInRangeOfEscape);
        gameManager.PlayerLeftRangeOfEscape.AddListener(ResetMessages);
        gameManager.KeyPickedUp.AddListener(IncreaseUIKeys);
        gameManager.KeyUsed.AddListener(DecreaseUIKeys);

        //Make sure each Ui Element is accounted for
        if (pauseButtonImage == null || endText == null || 
            endTextBackground == null || messageWindow == null || 
            controlsWindow == null)
        {
            Debug.LogError("One or more of your UI Elements are null");
        }

        //Starting the Coroutine to set visibility
        //of the Controls Window to false
        StartCoroutine(TurnOffControlWindow());
    }

    private void ChangePauseImageVisibility()
    {
        //Changing the visibility to the
        //opposite of what it currently is
        if (pauseButtonImage != null)
        {
            if (!pauseButtonImage.gameObject.activeSelf)
            {
                pauseButtonImage.gameObject.SetActive(true);
            }
            else
            {
                pauseButtonImage.gameObject.SetActive(false);
            }
        }
    }

    private void TriggerWinUI()
    {
        //Setting the win text
        string winText = "Victory!! You escaped!";
        endText.gameObject.SetActive(true);
        endTextBackground.gameObject.SetActive(true);
        endText.text = winText;
    }

    private void TriggerLoseUI()
    {
        //Setting the lose text
        string loseText = "You got caught....";
        endText.gameObject.SetActive(true);
        endTextBackground.gameObject.SetActive(true);
        endText.text = loseText;
    }

    private void PlayerCantOpenLock()
    {
        //Saying to player that they can't open this without key
        string message = "Can't open this lock without a key.";
        messageWindow.text = message;

        StartCoroutine(WaitToClearMessage());
    }

    private void PlayerInRangeOfEscape()
    {
        //Saying to player how to input to escape
        string message = "Hit E to escape!";
        messageWindow.text = message; 
    }

    private void IncreaseUIKeys()
    {
        amountOfKeys++;
        keyAmountWindow.text = $"{amountOfKeys}";
    }

    private void DecreaseUIKeys()
    {
        amountOfKeys--;
        keyAmountWindow.text = $"{amountOfKeys}";
    }

    private void ResetMessages()
    {
        //Removing the dialogue to show player the input
        //as player left the range after certain period
        messageWindow.text = string.Empty;
    }

    private IEnumerator WaitToClearMessage()
    {
        yield return new WaitForSeconds(2f);
        ResetMessages();
    }

    private IEnumerator TurnOffControlWindow()
    {
        //Waiting for time
        yield return new WaitForSeconds(timeForControlsOnScreen);

        //Turning off the Control Window
        controlsWindow.gameObject.SetActive(false);
    }
}

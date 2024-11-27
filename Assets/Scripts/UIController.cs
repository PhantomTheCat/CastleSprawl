using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class meant for controlling the Game UI in accordance to the game.
/// </summary>
public class UIController : MonoBehaviour
{
    //Properties
    [SerializeField] private Image pauseButtonImage;
    [SerializeField] private TextMeshProUGUI endText;
    [SerializeField] private Image endTextBackground;
    [SerializeField] private TextMeshProUGUI messageWindow;
    private GameManager gameManager;
    private float typingSpeed = 0.25f;

    //Methods
    protected void Start()
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

        //Make sure each Ui Element is accounted for
        if (pauseButtonImage == null || endText == null || 
            endTextBackground == null || messageWindow == null)
        {
            Debug.LogError("One or more of your UI Elements are null");
        }
    }

    protected void ChangePauseImageVisibility()
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

    protected void TriggerWinUI()
    {
        //Setting the win text
        string winText = "Victory!! You escaped!";
        endText.gameObject.SetActive(true);
        endTextBackground.gameObject.SetActive(true);
        endText.text = winText;
    }

    protected void TriggerLoseUI()
    {
        //Setting the lose text
        string loseText = "You got caught....";
        endText.gameObject.SetActive(true);
        endTextBackground.gameObject.SetActive(true);
        endText.text = loseText;
    }

    protected void PlayerCantOpenLock()
    {
        //Saying to player that they can't open this without key
        string message = "Can't open this lock without a key.";
        messageWindow.text = message;

        StartCoroutine(WaitToClearMessage());
    }

    protected void PlayerInRangeOfEscape()
    {
        //Saying to player how to input to escape
        string message = "Hit E to escape!";
        messageWindow.text = message; 
    }

    protected void ResetMessages()
    {
        //Removing the dialogue to show player the input
        //as player left the range after certain period
        messageWindow.text = string.Empty;
    }

    protected IEnumerator WaitToClearMessage()
    {
        yield return new WaitForSeconds(2f);
        ResetMessages();
    }
}

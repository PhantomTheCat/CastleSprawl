using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Class that gives an overarching reach of the game and 
/// allows for tracking of win and lose conditions as well as scene conditions.
/// </summary>
public class GameManager : Singleton<GameManager>
{
    //
    //Properties
    //
    /// <summary>
    /// Unity event for when the player gets caught and loses
    /// </summary>
    public UnityEvent PlayerCaught = new UnityEvent();

    /// <summary>
    /// Unity event for when the player escapes the building and wins
    /// </summary>
    public UnityEvent PlayerEscaped = new UnityEvent();

    /// <summary>
    /// Unity Event for when the player hits the start button in the splash screen
    /// </summary>
    public UnityEvent StartButtonPushed = new UnityEvent();

    /// <summary>
    /// Unity event for when the player pauses the game
    /// </summary>
    public UnityEvent PauseButtonPushed = new UnityEvent();

    /// <summary>
    /// Unity Event for when the player tries to open the lock without the key. 
    /// Used for the UI to send a message to player
    /// </summary>
    public UnityEvent PlayerCantOpenLock = new UnityEvent();

    /// <summary>
    /// Unity event for when the player is in range of a openable lock, 
    /// which will trigger UI to tell player to hit button to escape
    /// </summary>
    public UnityEvent PlayerInRangeOfEscape = new UnityEvent();

    /// <summary>
    /// Unity even for when player leaves the escape, 
    /// but triggered a few seconds after leaving
    /// </summary>
    public UnityEvent PlayerLeftRangeOfEscape = new UnityEvent();

    private SceneLoader sceneLoader;
    private bool isPaused = false;
    private float secondsBeforeEnding = 3f;



    //Methods
    public override void Awake()
    {
        //Activating our singleton
        base.Awake();

        //Getting our scene loader's instance as it's a singleton
        sceneLoader = SceneLoader.Instance;

        //Tying events to proper methods
        Instance.PlayerCaught.AddListener(EndGame);
        Instance.PlayerEscaped.AddListener(EndGame);
        Instance.StartButtonPushed.AddListener(StartGame);
        Instance.PauseButtonPushed.AddListener(PauseGame);
    }

    public void PushStartButton()
    {
        //This method is for the start button to activate
        //In the designer
        StartButtonPushed.Invoke();
    }

    private void EndGame()
    {
        //Pausing game to restrict player movement
        PauseButtonPushed.Invoke();

        //Starting a coroutine to go back to the splash screen
        StartCoroutine(GoBackToSplashScreen());
    }

    private void StartGame()
    {
        //Loading the main scene after we switch it to that
        sceneLoader.SwitchLoadedScene(SceneLoader.SceneTypes.MAIN);
        sceneLoader.LoadScene();
    }

    private void PauseGame()
    {
        //Checking if paused already or not
        if (!isPaused)
        {
            //If we're not paused, going to pause
            Time.timeScale = 0f;
            isPaused = true;
        }
        else
        {
            //Otherwise, we are going to unpause
            Time.timeScale = 1f;
            isPaused = false;
        }
    }

    private IEnumerator GoBackToSplashScreen()
    {
        //Waiting a few seconds before going back to 
        yield return new WaitForSecondsRealtime(secondsBeforeEnding);

        //Unpausing the game and going back to splash screen
        PauseGame();
        sceneLoader.SwitchLoadedScene(SceneLoader.SceneTypes.SPLASHSCREEN);
        sceneLoader.LoadScene();
    }
}

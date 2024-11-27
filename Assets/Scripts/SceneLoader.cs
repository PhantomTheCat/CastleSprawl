using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Class for moving/transitioning between scenes and 
/// is called in our Game Manager script.
/// </summary>
public class SceneLoader : Singleton<SceneLoader>
{
    //Properties
    /// <summary>
    /// An enum for documenting the scene we're currently on
    /// and/or the one we want to switch to
    /// </summary>
    public enum SceneTypes
    {
        SPLASHSCREEN,
        MAIN,
    }

    /// <summary>
    /// The name of the scene for the splash screen
    /// </summary>
    [SerializeField] private string splashScreenSceneName;

    /// <summary>
    /// The name of our main gameplay scene to call for later
    /// </summary>
    [SerializeField] private string mainSceneName;

    /// <summary>
    /// The current scene we are going to switch to
    /// </summary>
    [SerializeField] private SceneTypes sceneType;


    //Methods
    public void LoadScene()
    {
        //Getting default value
        string sceneName = string.Empty;

        //Switching the name based on our scene type
        switch (sceneType)
        {
            case SceneTypes.SPLASHSCREEN:
                sceneName = splashScreenSceneName;
                Cursor.lockState = CursorLockMode.None;
                break;
            case SceneTypes.MAIN:
                sceneName = mainSceneName;
                Cursor.lockState = CursorLockMode.Locked;
                break;
        }

        //Making sure it doesn't break
        if (sceneName != string.Empty)
        {
            try
            {
                //Loading scene
                SceneManager.LoadScene(sceneName);
            }
            catch
            {
                Debug.LogError("The scene you tried to load was not there. " +
                    "Make sure the name of your scene is correct in Scene Loader.");
            }
        }
        else
        {
            //Throwing error as that meant we didn't get a name
            Debug.LogError("The current scene type doesn't have a string! " +
                "Scene Loader was not able to load scene.");
        }
    }

    public void SwitchLoadedScene(SceneTypes scene)
    {
        //Switching the scene type to desired scene
        sceneType = scene;
    }
}

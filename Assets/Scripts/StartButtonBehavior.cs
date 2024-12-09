using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class for the Start Button in the splash screen 
/// for the button to hook up to the Game Manager's click event
/// </summary>
public class StartButtonBehavior : MonoBehaviour
{
    //Properties
    private Button thisInstance;

    //Methods
    protected void Start()
    {
        //Getting this instance
        thisInstance = GetComponent<Button>();

        //Switching the load scene to the new loaded scene
        thisInstance.onClick.AddListener(GameManager.Instance.StartButtonPushed.Invoke);
    }
}

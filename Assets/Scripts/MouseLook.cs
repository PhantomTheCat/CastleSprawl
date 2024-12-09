using UnityEngine;

/// <summary>
/// Class attached to player for controlling the first-person camera movement. 
/// Seperates the camera movement from the player movement.
/// </summary>
public class MouseLook : MonoBehaviour
{
    //Properties
    [Header("Sensitivity")]
    [SerializeField] private float sensitivityHorizontal = 9f;
    [SerializeField] private float sensitivityVertical = 9f;

    [Header("View Constraints")]
    [SerializeField] private float minimumVert = -45.0f;
    [SerializeField] private float maximumVert = 45.0f;

    [Header("Input")]
    [SerializeField] private float verticalRotation = 0;
    [SerializeField] private float horizontalRotation = 0;

    [Header("Camera")]
    [SerializeField] private Camera playerCamera;
    private bool cameraLocked = false;
    

    //Methods
    protected void Start()
    {
        //Locking the cursor to center, so mouse isn't distracting
        Cursor.lockState = CursorLockMode.Locked;

        //Getting rigidbody and check for it
        Rigidbody body = GetComponent<Rigidbody>();
        if (body != null)
        {
            body.freezeRotation = true;
        }

        //Subscribing to the pause, so our camera won't move during it
        GameManager.Instance.PauseButtonPushed.AddListener(ChangeLockCameraMovement);
    }

    protected void Update()
    {
        //Controlling rotation for player and camera
        if (!cameraLocked)
        {
            //(Player focuses on horizontal rotation because
            //they can't rotate up or otherwise will go upwards)
            PlayerMouseRotation();

            //Camera focuses on vertical rotation as player
            //can't be rotated up (restriction)
            CameraMouseRotation();
        }
    }

    /// <summary>
    /// Controlling vertical rotation on the camera
    /// </summary>
    private void CameraMouseRotation()
    {
        //Getting the vertical rotation (Used for camera)
        verticalRotation -= Input.GetAxis("Mouse Y") * sensitivityVertical;
        verticalRotation = Mathf.Clamp(verticalRotation, minimumVert, maximumVert);

        //Transforming camera rotation
        playerCamera.transform.localEulerAngles = new Vector3(verticalRotation, 0, 0);
    }

    /// <summary>
    /// Controlling horizontal rotation of player on the player.
    /// </summary>
    private void PlayerMouseRotation()
    {
        //Getting the horizontal rotation (Used for player)
        float delta = Input.GetAxis("Mouse X") * sensitivityHorizontal;
        horizontalRotation = transform.localEulerAngles.y + delta;

        //Transforming player rotation
        this.transform.localEulerAngles = new Vector3(0, horizontalRotation, 0);
    }

    /// <summary>
    /// Changing the lock of the camera movement to the other state
    /// </summary>
    public void ChangeLockCameraMovement()
    {
        //Putting the camera lock on the
        //opposite setting of what it was
        if (cameraLocked)
        {
            cameraLocked = false;
        }
        else
        {
            cameraLocked = true;
        }
    }
}

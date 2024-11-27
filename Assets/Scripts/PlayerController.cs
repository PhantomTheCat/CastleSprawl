using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Class for getting input from player and 
/// moving the player based on direction they are facing 
/// </summary>
[RequireComponent(typeof(PlayerBehavior))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    //Properties
    [SerializeField] private const float playerSpeed = 6f;
    [SerializeField] private float gravityMagnitude = -9.8f;
    [SerializeField] private float sprintModifier = 1.5f;
    private InputAction zAxisMove;
    private InputAction xAxisMove;
    private InputAction crouch;
    private InputAction sprint;
    private InputAction pauseMenu;
    private PlayerControls controlMappings;
    private CharacterController characterController;
    private PlayerBehavior player;
    private bool isSprinting = false;
    private bool canEscape = false;
    private float secondsWindowToEscape = 2f;


    //Methods
    protected void Awake()
    {
        //Linking these to instances
        controlMappings = new PlayerControls();
        characterController = GetComponent<CharacterController>();
        player = GetComponent<PlayerBehavior>();

        //Linking the movement to the inputs so we know when player wants to move
        zAxisMove = controlMappings.Movement.MoveForwardAndBack;
        xAxisMove = controlMappings.Movement.MoveLeftAndRight;
        sprint = controlMappings.Movement.Sprint;
        crouch = controlMappings.Movement.Crouch;
        pauseMenu = controlMappings.Movement.Menu;
    }

    protected void OnEnable()
    {
        //Enabling the controls for play
        zAxisMove.Enable();
        xAxisMove.Enable();
        sprint.Enable();
        crouch.Enable();
        pauseMenu.Enable();
    }

    protected void OnDisable()
    {
        //Disabling the controls when this component is (Stops looking for them)
        zAxisMove.Disable();
        xAxisMove.Disable();
        sprint.Disable();
        crouch.Disable();
        pauseMenu.Disable();
    }

    protected void Update()
    {
        //Checking for menu input
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameManager.Instance.PauseButtonPushed.Invoke();
        }

        //Seeing if they input choose to escape
        if (Input.GetKeyDown(KeyCode.E) && canEscape)
        {
            GameManager.Instance.PlayerEscaped.Invoke();
        }
    }

    protected void FixedUpdate()
    {
        //Handling movement
        HandleMovement();
    }

    protected void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //Making sure we can actually perform the operations
        if (characterController == null) return;

        //Seeing if we hit a key
        if (hit.gameObject.tag == "Key")
        {
            Destroy(hit.gameObject);
            player.CollectKey();
            Debug.Log("Player got a key!");
        }

        //Seeing if we hit a padlock
        if (hit.gameObject.tag == "Padlock")
        {
            if (player.CheckIfEnoughKeys())
            {
                canEscape = true;
                GameManager.Instance.PlayerInRangeOfEscape.Invoke();
                StartCoroutine(SetWindowToEscape());
            }
            else
            {
                GameManager.Instance.PlayerCantOpenLock.Invoke();
            }
        }
    }

    private void HandleMovement()
    {
        //Getting their input as floats
        float xAxisInput = xAxisMove.ReadValue<float>() * playerSpeed;
        float zAxisInput = zAxisMove.ReadValue<float>() * playerSpeed;

        //Seeing if sprinting or not
        if (sprint.ReadValue<float>() > 0)
        {
            xAxisInput *= sprintModifier;
            zAxisInput *= sprintModifier;
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }

        //Translating input into Vector3/axis along the x and z axis respectfully
        Vector3 movement = new Vector3(xAxisInput, 0, zAxisInput);

        //Applying gravity
        movement.y = gravityMagnitude;

        //Moving character
        movement *= Time.deltaTime;
        movement = transform.TransformDirection(movement);
        characterController.Move(movement);
    }

    private IEnumerator SetWindowToEscape()
    {
        //Setting a timer for when they can no longer escape
        //Means they stepped away from the lock with a key
        yield return new WaitForSeconds(secondsWindowToEscape);
        GameManager.Instance.PlayerLeftRangeOfEscape.Invoke();
        canEscape = false;
    }
}

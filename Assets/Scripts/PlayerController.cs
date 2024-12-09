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
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(FootstepController))]
public class PlayerController : MonoBehaviour
{
    //
    //Properties
    //
    [Header("Movement Stats")]
    [SerializeField] private float playerSpeed = 6f;
    [SerializeField] private float gravityMagnitude = -9.8f;
    [SerializeField] private float sprintModifier = 1.5f;

    [Header("Camera Parenting")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform crouchedCameraPosition;

    //Input Actions for the new input system
    private InputAction zAxisMove;
    private InputAction xAxisMove;
    private InputAction crouch;
    private InputAction sprint;
    private PlayerControls controlMappings;

    //Components
    private CharacterController characterController;
    private PlayerBehavior player;
    private Animator animator;
    private FootstepController footstepController;

    //Tracking Variables
    private bool isSprinting = false;
    private bool canEscape = false;
    private float secondsWindowToEscape = 2f;
    private Vector3 cameraStartingPosition;

    //
    //Methods
    //
    protected void Awake()
    {
        //Linking these to instances
        controlMappings = new PlayerControls();
        characterController = GetComponent<CharacterController>();
        player = GetComponent<PlayerBehavior>();
        animator = transform.GetComponentInChildren<Animator>();
        footstepController = GetComponent<FootstepController>();

        //Linking the movement to the inputs so we know when player wants to move
        zAxisMove = controlMappings.Movement.MoveForwardAndBack;
        xAxisMove = controlMappings.Movement.MoveLeftAndRight;
        sprint = controlMappings.Movement.Sprint;
        crouch = controlMappings.Movement.Crouch;

        //Making sure we got our animator
        if (animator == null)
        {
            Debug.LogError("Couldn't find our player animator in the children of the player");
        }

        //Getting the camera's starting position
        cameraStartingPosition = playerCamera.transform.localPosition;
    }

    protected void OnEnable()
    {
        //Enabling the controls for play
        zAxisMove.Enable();
        xAxisMove.Enable();
        sprint.Enable();
        crouch.Enable();
    }

    protected void OnDisable()
    {
        //Disabling the controls when this component is (Stops looking for them)
        zAxisMove.Disable();
        xAxisMove.Disable();
        sprint.Disable();
        crouch.Disable();
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
            player.UseKey();
        }
    }

    protected void FixedUpdate()
    {
        //Handling movement
        HandleMovement();

        //Seeing if we're crouched
        CheckForCrouched();
    }

    protected void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //Making sure we can actually perform the operations
        if (characterController == null) return;

        //Seeing if we hit a key
        if (hit.gameObject.tag == "Key")
        {
            //Collecting the key and destroying the key game object
            Destroy(hit.gameObject);
            player.CollectKey();
        }

        //Seeing if we hit a padlock
        if (hit.gameObject.tag == "Padlock")
        {
            //Seeing if we have enough keys
            if (player.CheckIfEnoughKeys())
            {
                //Setting a time window to escape
                canEscape = true;
                GameManager.Instance.PlayerInRangeOfEscape.Invoke();
                StartCoroutine(SetWindowToEscape());
            }
            else
            {
                //Sending message to UI that we can't open this yet
                GameManager.Instance.PlayerCantOpenLock.Invoke();
            }
        }
    }

    private void HandleMovement()
    {
        //Getting their input as floats
        float xAxisInput = xAxisMove.ReadValue<float>() * playerSpeed;
        float zAxisInput = zAxisMove.ReadValue<float>() * playerSpeed;

        //Seeing if we are moving
        if (xAxisInput != 0 || zAxisInput != 0)
        {
            //Seeing if sprinting or not
            if (sprint.ReadValue<float>() > 0)
            {
                xAxisInput *= sprintModifier;
                zAxisInput *= sprintModifier;
                isSprinting = true;
            }

            //Applying the footstep sound if movement is above 0
            footstepController.ChangeMovingState(true, isSprinting);

            //Translating input into Vector3/axis along the x and z axis respectfully
            Vector3 movement = new Vector3(xAxisInput, 0, zAxisInput);

            //Applying gravity
            movement.y = gravityMagnitude;

            //Moving character
            movement *= Time.deltaTime;
            movement = transform.TransformDirection(movement);
            characterController.Move(movement);

        }
        else
        {
            //Else stop the footstep sounds
            footstepController.ChangeMovingState(false, isSprinting);
        }

        //Setting the sprinting bool to default
        isSprinting = false;
    }

    private IEnumerator SetWindowToEscape()
    {
        //Setting a timer for when they can no longer escape
        //Means they stepped away from the lock with a key
        yield return new WaitForSeconds(secondsWindowToEscape);
        GameManager.Instance.PlayerLeftRangeOfEscape.Invoke();
        canEscape = false;
    }

    private void CheckForCrouched()
    {
        //Getting the crouch input
        float crouchInput = crouch.ReadValue<float>();

        //Seeing if crouch input is greater than 0
        //(means we're crouched)
        if (crouchInput > 0)
        {
            animator.SetBool("IsCrouched", true);
            playerCamera.transform.position = crouchedCameraPosition.position;
        }
        else
        {
            animator.SetBool("IsCrouched", false);
            playerCamera.transform.localPosition = cameraStartingPosition;
        }
    }
}

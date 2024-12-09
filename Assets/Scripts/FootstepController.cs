using NUnit.Framework.Internal.Filters;
using UnityEngine;

/// <summary>
/// Class for determing the audio for footsteps for each walkable entity. 
/// Will trigger footsteps for a few seconds into each step 
/// (so the audio isn't cluttered with footsteps)
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class FootstepController : MonoBehaviour
{
    //Properties
    [Header("Sound Features")]
    [SerializeField] private AudioClip footstepSound;
    [SerializeField] private AudioClip sprintSound;
    [SerializeField] private float maxTimeBetweenFootsteps = 2f;
    private float currentTimeBetweenFootsteps = 0f;
    private AudioSource audioSource;
    private bool isMoving = false;
    private bool isSprinting = false;

    //Methods
    private void Awake()
    {
        //Getting the audio source component
        audioSource = GetComponent<AudioSource>();

        //Setting the initial time on clock
        currentTimeBetweenFootsteps = maxTimeBetweenFootsteps;

        //Making the audio source play spatial sound, so player can
        //determine where the sound/enemy is coming from
        audioSource.spatialize = true;
    }

    private void Update()
    {
        if (isMoving)
        {
            //Minusing the time between the footsteps by the time passed
            currentTimeBetweenFootsteps -= Time.deltaTime;

            if (currentTimeBetweenFootsteps <= 0)
            {
                //Stopping the audio source, so we can play a new one
                audioSource.Stop();

                //Seeing if we're sprinting or not
                if (isSprinting)
                {
                    //Playing the sprint sound
                    audioSource.clip = sprintSound;
                }
                else
                {
                    //Playing the footstep sound
                    audioSource.clip = footstepSound;
                }

                //Playing the sound
                audioSource.Play();

                //Reseting the time between footsteps
                currentTimeBetweenFootsteps = maxTimeBetweenFootsteps;
            }
        }
    }

    /// <summary>
    /// Changing the moving state based on whether the character controller is moving or not
    /// </summary>
    /// <param name="moving"></param>
    public void ChangeMovingState(bool moving, bool sprinting)
    {
        //Changing the move
        if (moving)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }

        //Changing the sprint
        if (sprinting)
        {
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }
    }
}

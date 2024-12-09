using UnityEngine;

/// <summary>
/// Class for responding to events and sending audio 
/// messages when events happen in GameManager
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    //Properties
    [Header("Audio Clips")]
    [SerializeField] private AudioClip caughtSound;
    [SerializeField] private AudioClip escapedSound;
    [SerializeField] private AudioClip keyPickUpSound;
    private AudioSource audioSource;
    private GameManager gameManager;


    //Methods
    private void Start()
    {
        //Getting the instances
        gameManager = GameManager.Instance;
        audioSource = GetComponent<AudioSource>();

        //Adding the listeners
        gameManager.KeyPickedUp.AddListener(PlayKeySound);
        gameManager.PlayerCaught.AddListener(PlayCaughtSound);
        gameManager.PlayerEscaped.AddListener(PlayEscapeSound);

    }

    private void PlaySound(AudioClip clip)
    {
        //Playing the sound after adding the clip in
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    private void PlayKeySound()
    {
        PlaySound(keyPickUpSound);
    }

    private void PlayEscapeSound()
    {
        PlaySound(escapedSound);
    }

    private void PlayCaughtSound()
    {
        PlaySound(caughtSound);
    }
}

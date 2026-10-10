using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class SimpleFootsteps : MonoBehaviour
{
    [Header("Audio Clips")]
    [SerializeField] private List<AudioClip> footstepClips = new List<AudioClip>(); // Drop your footstep audio clips here

    [Header("Settings")]
    [SerializeField] private float walkStepInterval = 0.5f;   // Seconds between footsteps when walking
    [SerializeField] private float sprintStepInterval = 0.35f; // Seconds between footsteps when sprinting
    [SerializeField] private float velocityThreshold = 1.0f;  // Minimum speed to trigger footsteps
    [SerializeField] private float runSpeedThreshold = 8.0f;   // Speed above which footsteps switch to sprinting tempo
    [SerializeField] private float groundCheckDistance = 1.1f; // Distance to check for ground (adjust based on player height)
    [SerializeField] private LayerMask groundLayer = ~0;       // What layers count as ground

    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.5f;             // Footstep sound volume


    private AudioSource audioSource;
    private Vector3 lastPosition;
    private float stepTimer;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        lastPosition = transform.position;
        stepTimer = 0f;

        // Configure audio source to not play on awake
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // Make it 3D sound
    }

    void Update()
    {
        // Calculate speed based on position change (independent of controller type)
        Vector3 displacement = transform.position - lastPosition;
        displacement.y = 0; // ignore vertical movement (jumping/falling)
        float currentSpeed = displacement.magnitude / Time.deltaTime;

        lastPosition = transform.position;

        // Check if player is grounded using a downward raycast
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        // Debug visualization in Scene View
        Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);

        if (isGrounded && currentSpeed > velocityThreshold)
        {
            stepTimer += Time.deltaTime;

            // Determine interval (sprinting vs walking)
            float currentInterval = (currentSpeed > runSpeedThreshold) ? sprintStepInterval : walkStepInterval;

            if (stepTimer >= currentInterval)
            {
                PlayFootstep();
                stepTimer = 0f;
            }
        }
        else
        {
            // Reset timer when standing still
            stepTimer = 0f;
        }
    }

    void PlayFootstep()
    {
        if (footstepClips == null || footstepClips.Count == 0) return;

        // Play a random clip from the list
        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Count)];
        
        if (audioSource != null)
        {
            if (!audioSource.enabled)
            {
                audioSource.enabled = true;
            }
            audioSource.PlayOneShot(clip, volume);
        }
    }
}

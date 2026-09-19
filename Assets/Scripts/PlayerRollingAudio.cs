using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerRollingAudio : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;

    [Header("滚动音效")]
    [SerializeField] private AudioSource rollingAudioSource;
    [SerializeField] private float movementThreshold = 0.1f;
    [SerializeField] private float playInterval = 0.18f;
    [SerializeField] private float volumeScale = 0.7f;

    [Header("落地音效")]
    [SerializeField] private AudioSource landingAudioSource;
    [SerializeField] private AudioClip landingClip;
    [SerializeField] private float landingVolume = 0.9f;

    [Header("速度音调")]
    [SerializeField] private bool varyPitch = true;
    [SerializeField] private float minPitch = 0.92f;
    [SerializeField] private float maxPitch = 1.08f;
    [SerializeField] private float referenceSpeed = 5.5f;

    private float nextPlayTime;

    private void Awake()
    {
        characterController = characterController != null
            ? characterController
            : GetComponent<CharacterController>();

        rollingAudioSource = rollingAudioSource != null
            ? rollingAudioSource
            : GetComponent<AudioSource>();

        if (landingAudioSource == null)
        {
            AudioSource[] sources = GetComponents<AudioSource>();

            if (sources.Length > 1)
                landingAudioSource = sources[1];
        }

        if (landingClip == null && landingAudioSource != null)
            landingClip = landingAudioSource.clip;

        rollingAudioSource.playOnAwake = false;
        rollingAudioSource.loop = false;

        if (landingAudioSource != null)
        {
            landingAudioSource.playOnAwake = false;
            landingAudioSource.loop = false;
        }

        // Avoid a first-use decode hitch when the landing clip is an MP3.
        if (landingClip != null && landingClip.loadState == AudioDataLoadState.Unloaded)
            landingClip.LoadAudioData();
    }

    /// <summary>
    /// Called by PlayerController immediately after CharacterController.Move.
    /// This is the exact frame in which the controller changes from airborne
    /// to grounded, so the landing sound starts without an extra frame delay.
    /// </summary>
    public void NotifyMovementResult(bool groundedBeforeMove, bool groundedAfterMove)
    {
        if (groundedAfterMove && !groundedBeforeMove)
            PlayLandingSound();
    }

    private void LateUpdate()
    {
        if (characterController == null)
            return;

        bool isGrounded = characterController.isGrounded;

        if (rollingAudioSource == null || rollingAudioSource.clip == null)
            return;

        Vector3 horizontalVelocity = characterController.velocity;
        horizontalVelocity.y = 0f;

        float speed = horizontalVelocity.magnitude;
        bool isMoving = speed > movementThreshold;

        // 跳跃、离地或停止移动时停止滚动声
        if (!isGrounded || !isMoving)
        {
            if (rollingAudioSource.isPlaying)
                rollingAudioSource.Stop();

            nextPlayTime = Time.time;
            return;
        }

        if (Time.time < nextPlayTime)
            return;

        if (varyPitch)
        {
            float speedFactor = Mathf.InverseLerp(
                movementThreshold,
                referenceSpeed,
                speed);

            rollingAudioSource.pitch = Mathf.Lerp(
                minPitch,
                maxPitch,
                speedFactor);
        }
        else
        {
            rollingAudioSource.pitch = 1f;
        }

        rollingAudioSource.PlayOneShot(
            rollingAudioSource.clip,
            volumeScale);

        nextPlayTime = Time.time + playInterval;
    }

    private void PlayLandingSound()
    {
        if (landingAudioSource == null || landingClip == null)
            return;

        landingAudioSource.PlayOneShot(
            landingClip,
            landingVolume);
    }

    private void OnDisable()
    {
        if (rollingAudioSource != null)
            rollingAudioSource.Stop();

        if (landingAudioSource != null)
            landingAudioSource.Stop();
    }
}

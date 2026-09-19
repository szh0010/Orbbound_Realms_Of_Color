using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5.5f;
    [SerializeField] private float jumpHeight = 2.2f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundedForce = -2f;

    [Header("Input")]
    [SerializeField] private string horizontalAxis = "Horizontal";
    [SerializeField] private string verticalAxis = "Vertical";
    [SerializeField] private string jumpButton = "Jump";

    [Header("Reference")]
    [SerializeField] private Transform cameraTransform;

    private CharacterController characterController;
    private PlayerRollingAudio rollingAudio;
    private float verticalVelocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        rollingAudio = GetComponent<PlayerRollingAudio>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        Vector2 input = new Vector2(
            Input.GetAxisRaw(horizontalAxis),
            Input.GetAxisRaw(verticalAxis));

        input = Vector2.ClampMagnitude(input, 1f);

        Vector3 forward = cameraTransform != null ? cameraTransform.forward : transform.forward;
        Vector3 right = cameraTransform != null ? cameraTransform.right : transform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 horizontalMove = (forward * input.y + right * input.x) * moveSpeed;

        bool groundedBeforeMove = characterController.isGrounded;

        if (groundedBeforeMove && verticalVelocity < 0f)
            verticalVelocity = groundedForce;

        if (groundedBeforeMove && Input.GetButtonDown(jumpButton))
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = horizontalMove + Vector3.up * verticalVelocity;
        characterController.Move(velocity * Time.deltaTime);

        // CharacterController.isGrounded is authoritative only after Move.
        // Notify the audio component in this same frame so landing sound has
        // no extra Update/LateUpdate delay.
        bool groundedAfterMove = characterController.isGrounded;
        rollingAudio?.NotifyMovementResult(groundedBeforeMove, groundedAfterMove);
    }
}

using UnityEngine;

[DisallowMultipleComponent]
public class OrbitCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 6f;
    [SerializeField] private float lookHeight = 0.9f;

    [Header("Mouse")]
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float startingPitch = 18f;
    [SerializeField] private float minimumPitch = -15f;
    [SerializeField] private float maximumPitch = 55f;
    [SerializeField] private bool rotateTargetWithCamera = true;
    [SerializeField] private bool lockCursorOnStart = true;

    private float yaw;
    private float pitch;

    private void Awake()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }

        if (target != null)
            yaw = target.eulerAngles.y;

        pitch = Mathf.Clamp(startingPitch, minimumPitch, maximumPitch);
    }

    private void Start()
    {
        if (lockCursorOnStart)
            SetCursorLocked(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            SetCursorLocked(false);

        if (Input.GetMouseButtonDown(0))
            SetCursorLocked(true);

        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minimumPitch, maximumPitch);
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        if (rotateTargetWithCamera)
            target.rotation = Quaternion.Euler(0f, yaw, 0f);

        Vector3 focus = target.position + Vector3.up * lookHeight;
        Quaternion orbitRotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 cameraOffset = orbitRotation * Vector3.back * distance;

        transform.position = focus + cameraOffset;
        transform.LookAt(focus);
    }

    private static void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}

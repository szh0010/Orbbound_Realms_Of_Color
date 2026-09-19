using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class PortalSpawnController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private CharacterController playerController;
    [SerializeField] private Behaviour movementScript;

    [Header("Animation")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private float spawnDelay = 0.2f;
    [SerializeField] private float duration = 1.1f;
    [SerializeField] private float jumpHeight = 0.8f;
    [SerializeField, Range(0.05f, 1f)] private float startScale = 0.2f;
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField] private float groundOffset = -0.01f;

    private Renderer[] playerRenderers;
    private Vector3 finalScale;
    private Coroutine spawnRoutine;

    private void Awake()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                player = playerObject.transform;
        }

        if (player == null)
            return;

        if (exitPoint == null)
            exitPoint = transform;

        if (playerController == null)
            playerController = player.GetComponent<CharacterController>();

        if (movementScript == null)
            movementScript = player.GetComponent<PlayerController>();

        // Keep the capsule aligned with the white sphere even if the scene was
        // saved with an older CharacterController size.
        if (playerController != null)
        {
            playerController.height = 1f;
            playerController.radius = 0.5f;
            playerController.center = Vector3.zero;
        }

        playerRenderers = player.GetComponentsInChildren<Renderer>(true);
        finalScale = player.localScale;

        SetGameplayEnabled(false);
        SetPlayerVisible(false);

        player.SetPositionAndRotation(transform.position + Vector3.up * 0.05f, exitPoint.rotation);
        player.localScale = finalScale * startScale;
    }

    private void Start()
    {
        if (playOnStart)
            PlaySpawn();
    }

    public void PlaySpawn()
    {
        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);

        spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(spawnDelay);

        Vector3 startPosition = transform.position + Vector3.up * 0.05f;
        Vector3 endPosition = exitPoint != null ? exitPoint.position : transform.position + Vector3.up;
        Quaternion endRotation = exitPoint != null ? exitPoint.rotation : player.rotation;

        SetPlayerVisible(true);

        float elapsed = 0f;
        float safeDuration = Mathf.Max(0.01f, duration);

        while (elapsed < safeDuration)
        {
            float linearT = elapsed / safeDuration;
            float t = Mathf.SmoothStep(0f, 1f, linearT);
            Vector3 position = Vector3.Lerp(startPosition, endPosition, t);
            position += Vector3.up * (Mathf.Sin(t * Mathf.PI) * jumpHeight);

            player.SetPositionAndRotation(position, endRotation);
            player.localScale = Vector3.Lerp(finalScale * startScale, finalScale, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        endPosition = SnapToGround(endPosition);
        player.SetPositionAndRotation(endPosition, endRotation);
        player.localScale = finalScale;
        SetGameplayEnabled(true);
        spawnRoutine = null;
    }

    private Vector3 SnapToGround(Vector3 position)
    {
        if (playerController == null)
            return position;

        Vector3 rayOrigin = position + Vector3.up * 1.5f;
        RaycastHit[] hits = Physics.RaycastAll(
            rayOrigin,
            Vector3.down,
            4f,
            groundLayers,
            QueryTriggerInteraction.Ignore);

        bool foundGround = false;
        RaycastHit nearestGround = default(RaycastHit);
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider hitCollider = hits[i].collider;
            if (hitCollider == null || hitCollider.transform == player || hitCollider.transform.IsChildOf(player))
                continue;

            if (hits[i].distance < nearestDistance)
            {
                foundGround = true;
                nearestDistance = hits[i].distance;
                nearestGround = hits[i];
            }
        }

        if (!foundGround)
            return position;

        float halfHeight = Mathf.Max(playerController.radius, playerController.height * 0.5f);
        position.y = nearestGround.point.y + halfHeight - playerController.center.y + groundOffset;
        return position;
    }

    private void SetGameplayEnabled(bool enabled)
    {
        if (playerController != null)
            playerController.enabled = enabled;

        if (movementScript != null)
            movementScript.enabled = enabled;
    }

    private void SetPlayerVisible(bool visible)
    {
        if (playerRenderers == null)
            return;

        for (int i = 0; i < playerRenderers.Length; i++)
        {
            if (playerRenderers[i] != null)
                playerRenderers[i].enabled = visible;
        }
    }
}

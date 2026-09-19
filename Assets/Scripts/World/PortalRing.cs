using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PortalRing : MonoBehaviour
{
    [Min(8)]
    public int segments = 64;

    [Min(0.01f)]
    public float radius = 0.55f;

    private LineRenderer line;

    private void Awake()
    {
        BuildRing();
    }

    private void OnValidate()
    {
        BuildRing();
    }

    private void BuildRing()
    {
        if (line == null)
            line = GetComponent<LineRenderer>();

        line.loop = true;
        line.useWorldSpace = false;
        line.positionCount = segments;

        for (int i = 0; i < segments; i++)
        {
            float angle = i / (float)segments * Mathf.PI * 2f;

            Vector3 point = new Vector3(
                Mathf.Cos(angle) * radius,
                0f,
                Mathf.Sin(angle) * radius
            );

            line.SetPosition(i, point);
        }
    }
}
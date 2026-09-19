using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PortalBeam : MonoBehaviour
{
    public float height = 6f;
    public float baseRadius = 0.6f;
    public float topRadius = 2.2f;

    [Range(8, 64)]
    public int segments = 32;

    public Material beamMaterial;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh beamMesh;

    private void OnEnable()
    {
        Rebuild();
        ApplyMaterial();
    }

    private void OnValidate()
    {
        Rebuild();
        ApplyMaterial();
    }

    private void Rebuild()
    {
        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();

        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();

        if (beamMesh == null)
        {
            beamMesh = new Mesh();
            beamMesh.name = "PortalBeamMesh";
        }

        beamMesh.Clear();

        int count = Mathf.Max(8, segments);

        Vector3[] vertices = new Vector3[count * 2];
        int[] triangles = new int[count * 6];

        for (int i = 0; i < count; i++)
        {
            float angle = i / (float)count * Mathf.PI * 2f;

            float x = Mathf.Cos(angle);
            float z = Mathf.Sin(angle);

            vertices[i] = new Vector3(
                x * baseRadius,
                0f,
                z * baseRadius
            );

            vertices[i + count] = new Vector3(
                x * topRadius,
                height,
                z * topRadius
            );
        }

        for (int i = 0; i < count; i++)
        {
            int next = (i + 1) % count;
            int index = i * 6;

            triangles[index] = i;
            triangles[index + 1] = next;
            triangles[index + 2] = i + count;

            triangles[index + 3] = next;
            triangles[index + 4] = next + count;
            triangles[index + 5] = i + count;
        }

        beamMesh.vertices = vertices;
        beamMesh.triangles = triangles;
        beamMesh.RecalculateBounds();

        meshFilter.sharedMesh = beamMesh;
    }

    private void ApplyMaterial()
    {
        if (beamMaterial != null)
            meshRenderer.sharedMaterial = beamMaterial;
    }
}
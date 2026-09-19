using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PortalDisc : MonoBehaviour
{
    [Min(0.1f)]
    public float radius = 0.55f;

    [Range(16, 128)]
    public int segments = 64;

    public Material discMaterial;

    private Mesh mesh;

    private void OnEnable()
    {
        BuildDisc();
    }

    private void OnValidate()
    {
        BuildDisc();
    }

    private void BuildDisc()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "PortalDiscMesh";
        }

        mesh.Clear();

        int count = Mathf.Max(16, segments);

        Vector3[] vertices = new Vector3[count + 1];
        Vector2[] uv = new Vector2[count + 1];
        int[] triangles = new int[count * 3];

        vertices[0] = Vector3.zero;
        uv[0] = new Vector2(0.5f, 0.5f);

        for (int i = 0; i < count; i++)
        {
            float angle = i / (float)count * Mathf.PI * 2f;

            float x = Mathf.Cos(angle);
            float z = Mathf.Sin(angle);

            vertices[i + 1] = new Vector3(
                x * radius,
                0f,
                z * radius
            );

            uv[i + 1] = new Vector2(
                x * 0.5f + 0.5f,
                z * 0.5f + 0.5f
            );
        }

        for (int i = 0; i < count; i++)
        {
            int next = (i + 1) % count;

            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = next + 1;
            triangles[i * 3 + 2] = i + 1;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh;

        if (discMaterial != null)
            meshRenderer.sharedMaterial = discMaterial;
    }
}
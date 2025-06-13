using UnityEngine;

public static class HexMeshGenerator
{
    private static Mesh _sharedMesh;

    public static Mesh GetSharedHexMesh(float height = 0.1f)
    {
        if (_sharedMesh != null)
            return _sharedMesh;

        _sharedMesh = new Mesh();
        _sharedMesh.name = "HexPrism";

        const int sides = 6;
        float radius = 1f;

        Vector3[] vertices = new Vector3[sides * 2 + 2]; // top + bottom + centers
        int vert = 0;

        // Top center
        vertices[vert++] = new Vector3(0, height, 0);
        // Top outer ring
        for (int i = 0; i < sides; i++)
        {
            float angle = Mathf.Deg2Rad * (60 * i);
            vertices[vert++] = new Vector3(Mathf.Cos(angle) * radius, height, Mathf.Sin(angle) * radius);
        }

        // Bottom center
        vertices[vert++] = new Vector3(0, 0, 0);
        // Bottom outer ring
        for (int i = 0; i < sides; i++)
        {
            float angle = Mathf.Deg2Rad * (60 * i);
            vertices[vert++] = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
        }

        // Triangles
        int[] triangles = new int[6 * 12]; // top + bottom + sides
        int t = 0;

        // Top face
        for (int i = 0; i < sides; i++)
        {
            triangles[t++] = 0;
            triangles[t++] = 1 + (i + 1) % sides;
            triangles[t++] = 1 + i;
        }

        // Bottom face
        int bottomCenter = sides + 1;
        for (int i = 0; i < sides; i++)
        {
            triangles[t++] = bottomCenter;
            triangles[t++] = bottomCenter + i + 1;
            triangles[t++] = bottomCenter + ((i + 1) % sides) + 1;
        }

        // Side walls
        for (int i = 0; i < sides; i++)
        {
            int topA = 1 + i;
            int topB = 1 + (i + 1) % sides;
            int bottomA = bottomCenter + i + 1;
            int bottomB = bottomCenter + ((i + 1) % sides) + 1;

            // First triangle
            triangles[t++] = topA;
            triangles[t++] = bottomB;
            triangles[t++] = bottomA;

            // Second triangle
            triangles[t++] = topA;
            triangles[t++] = topB;
            triangles[t++] = bottomB;
        }

        _sharedMesh.vertices = vertices;
        _sharedMesh.triangles = triangles;
        _sharedMesh.RecalculateNormals();
        _sharedMesh.RecalculateBounds();

        return _sharedMesh;
    }
}

using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class HexGridMat : MonoBehaviour
{
    [Tooltip("Color of the mat under the hex grid.")]
    public Color matColor = new Color(0.2f, 0.2f, 0.2f, 1f); // dark grey

    [Tooltip("Extra padding around the grid in world units.")]
    public float border = 0.5f;

    public void CreateMat(float totalWidth, float totalHeight)
    {
        // Cleanup existing mat mesh if re-generating
        MeshFilter filter = GetComponent<MeshFilter>();
        MeshRenderer renderer = GetComponent<MeshRenderer>();

        // Create flat quad mesh
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-0.5f, 0, -0.5f),
            new Vector3(-0.5f, 0, 0.5f),
            new Vector3(0.5f, 0, 0.5f),
            new Vector3(0.5f, 0, -0.5f)
        };

        int[] triangles = new int[]
        {
            0, 1, 2,
            0, 2, 3
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        filter.sharedMesh = mesh;

        // Resize and position
        float width = totalWidth + border * 2f;
        float height = totalHeight + border * 2f;
        transform.localScale = new Vector3(width, 1f, height);
        transform.localPosition = Vector3.zero;

        // Assign a simple material if none present
        if (renderer.sharedMaterial == null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", matColor);
            renderer.sharedMaterial = mat;
        }
        else
        {
            renderer.sharedMaterial.SetColor("_BaseColor", matColor);
        }
    }
}

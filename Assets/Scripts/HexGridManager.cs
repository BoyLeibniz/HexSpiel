using UnityEngine;
using System.Collections.Generic;

public class HexGridManager : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public float hexSize = 1f;
    public GameObject hexPrefab;

    public HexGridMat mat;  // drag in from scene or prefab

    // Constants for flat-topped hex layout
    private const float HEX_WIDTH = 1.5f;      // width between hex centers (flat-top)
    private const float HEX_HEIGHT = 1.732f;   // height of hex (≈ sqrt(3))

    void Start()
    {
        GenerateGrid();
    }

    public void GenerateGrid()
    {
        List<Vector3> positions = new List<Vector3>();

        for (int q = 0; q < width; q++)
        {
            for (int r = 0; r < height; r++)
            {
                float x = HEX_WIDTH * q;
                float z = HEX_HEIGHT * (r + 0.5f * (q % 2));
                Vector3 pos = new Vector3(x, 0, z) * hexSize;
                positions.Add(pos);
            }
        }

        // Calculate grid center
        Vector3 centerOffset = Vector3.zero;
        foreach (var p in positions)
            centerOffset += p;
        centerOffset /= positions.Count;

        // Generate tiles centered at origin
        int i = 0;
        for (int q = 0; q < width; q++)
        {
            for (int r = 0; r < height; r++)
            {
                Vector3 worldPos = positions[i++] - centerOffset;

                HexCoord coord = new HexCoord(q, r);
                GameObject hexGO = Instantiate(hexPrefab, worldPos, Quaternion.identity, transform);
                hexGO.name = $"Hex_{q},{r}";

                HexCell cell = hexGO.GetComponent<HexCell>();
                cell.Init(coord, worldPos);
            }
        }

        // Resize and center the mat to match the grid bounds
        if (mat != null && positions.Count > 0)
        {
            Vector3 min = positions[0];
            Vector3 max = positions[0];
            foreach (var pos in positions)
            {
                min = Vector3.Min(min, pos);
                max = Vector3.Max(max, pos);
            }

            // Add a margin of 0.5 hex unit around
            float margin = hexSize * HEX_HEIGHT * 0.5f; // ~half hex height, gives clean border

            float gridWidth = (max.x - min.x) + margin * 2f;
            float gridHeight = (max.z - min.z) + margin * 2f;

            mat.transform.localPosition = -centerOffset + new Vector3(0, -0.01f, 0);
            mat.CreateMat(gridWidth, gridHeight);
        }

    }

    public void ClearGrid()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}


// Assets/Scripts/HexMap/HexGridManager.cs

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the generation, layout, and cleanup of a 2D hexagonal grid using flat-topped hexes
/// and Axial notation.
/// </summary>
public class HexGridManager : MonoBehaviour, IDataPersistence
{
    /// <summary>
    /// Number of hex tiles horizontally (q-axis).
    /// </summary>
    public int width = 10;

    /// <summary>
    /// Number of hex tiles vertically (r-axis).
    /// </summary>
    public int height = 10;

    /// <summary>
    /// The size (scaling factor) of each hex tile in Unity units (default is 1 = 1 meter).
    /// </summary>
    public float hexSize = 1f;

    /// <summary>
    /// Reference to the hex tile prefab. Must be assigned in the Unity Editor.
    /// </summary>
    public GameObject hexPrefab;

    /// <summary>
    /// Optional reference to a HexGridMat component used to create a visual mat beneath the grid.
    /// Drag this in from the scene or prefab.
    /// </summary>
    public HexGridMat mat;

    /// <summary>
    /// Reference to the HexInspectorController for managing tile selection and properties.
    /// Must be assigned in the Unity Editor.
    /// </summary>
    public HexInspectorController inspectorController;

    /// <summary>
    /// Reference to the MapDataService for handling save/load operations.
    /// </summary>
    [Header("Save System")]
    public MapDataService mapDataService;

    private const float HEX_WIDTH = 1.5f;    // Distance between centers of adjacent hexes horizontally
    private const float HEX_HEIGHT = 1.732f; // Full height of a hex (approx. sqrt(3)) for vertical spacing

    /// <summary>
    /// Reference to the tooltip manager for handling label display.
    /// </summary>
    [Header("Tooltip System")]
    public HexTooltipManager tooltipManager;

    /// <summary>
    /// Unity lifecycle method. Initializes dependencies and triggers grid generation.
    /// If MapDataService is not assigned, it will be created automatically.
    /// </summary>
    void Start()
    {
        Debug.Log($"[HexGridManager] Default Data Path (Application.persistentDataPath): {Application.persistentDataPath}");
        GenerateGrid();
    }

    /// <summary>
    /// Generates a centered flat-topped hex grid using the configured prefab, size, and dimensions.
    /// Also resizes and positions an optional underlying mat for visual reference.
    /// </summary>
    public void GenerateGrid()
    {
        List<Vector3> positions = new List<Vector3>();

        // Step 1: Calculate all hex positions using axial coordinates (q, r)
        for (int q = 0; q < width; q++)
        {
            for (int r = 0; r < height; r++)
            {
                float x = HEX_WIDTH * q;
                float z = HEX_HEIGHT * (r + 0.5f * (q % 2)); // Offset every other column
                Vector3 pos = new Vector3(x, 0, z) * hexSize;
                positions.Add(pos);
            }
        }

        // Step 2: Compute average position to center the grid around the origin
        Vector3 centerOffset = Vector3.zero;
        foreach (var p in positions)
            centerOffset += p;
        centerOffset /= positions.Count;

        // Step 3: Instantiate each hex tile at its world-space position
        int i = 0;
        for (int q = 0; q < width; q++)
        {
            for (int r = 0; r < height; r++)
            {
                Vector3 worldPos = positions[i++] - centerOffset;

                HexCoord coord = new HexCoord(q, r);
                GameObject hexGO = Instantiate(hexPrefab, worldPos, Quaternion.identity, transform);
                hexGO.name = $"Hex_{q},{r}";

                // Initialize the tile's logic script (HexCell) with its coordinates and world position
                HexCell cell = hexGO.GetComponent<HexCell>();
                cell.Init(coord, worldPos);

                // Share the Controller with the Hex
                var visuals = hexGO.GetComponent<HexTileVisuals>();
                if (visuals != null && inspectorController != null)
                    visuals.SetInspector(inspectorController);
            }
        }

        // Step 4: Resize and position the grid mat to fit beneath the tile layout
        if (mat != null && positions.Count > 0)
        {
            Vector3 min = positions[0];
            Vector3 max = positions[0];
            foreach (var pos in positions)
            {
                min = Vector3.Min(min, pos);
                max = Vector3.Max(max, pos);
            }

            // Add a margin of 0.5 hex height units to frame the grid cleanly
            float margin = hexSize * HEX_HEIGHT * 0.5f;

            float gridWidth = (max.x - min.x) + margin * 2f;
            float gridHeight = (max.z - min.z) + margin * 2f;

            // Offset the mat slightly below the hexes for visual layering
            mat.transform.localPosition = -centerOffset + new Vector3(0, -0.01f, 0);
            mat.CreateMat(gridWidth, gridHeight);
        }
    }

    #region IDataPersistence Implementation

    /// <summary>
    /// Returns the current grid layout as a serializable MapData object.
    /// </summary>
    public MapData SaveData()
    {
        return mapDataService.CreateMapDataFromGrid(this);
    }

    /// <summary>
    /// Applies the provided MapData to this grid. Returns false if data is invalid.
    /// </summary>
    public bool LoadData(MapData data)
    {
        if (data == null || data.tiles == null || data.tiles.Count == 0)
        {
            Debug.LogError("HexGridManager: Invalid map data.");
            return false;
        }

        mapDataService.ApplyMapDataToGrid(data, this);

        // Refresh tooltips after loading
        if (tooltipManager != null)
        {
            tooltipManager.RefreshAllTooltips();
        }

        return true;
    }

    #endregion

    #region Public API

    /// <summary>
    /// Immediately removes all hex tiles from the scene by destroying all child objects.
    /// Useful for regenerating or resetting the grid.
    /// </summary>
    public void ClearGrid()
    {
        // Clear tooltips before destroying grid
        if (tooltipManager != null)
        {
            tooltipManager.ClearAllTooltips();
        }

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.GetComponent<DoNotClear>() != null)
                continue;

            DestroyImmediate(child.gameObject);
        }
    }

    /// <summary>
    /// Regenerates the hex grid with new dimensions, clearing the old grid first.
    /// </summary>
    public void RegenerateGrid(int newWidth, int newHeight)
    {
        this.width = newWidth;
        this.height = newHeight;

        ClearGrid();
        GenerateGrid();

        // Refresh tooltips after regeneration
        if (tooltipManager != null)
        {
            tooltipManager.RefreshAllTooltips();
        }
    }

    /// <summary>
    /// Saves the current grid to file using the MapDataService. Returns success state.
    /// </summary>
    public bool SaveMap(string fileName = "default_map")
    {
        return mapDataService?.SaveCurrentMap(this, fileName) ?? false;
    }

    /// <summary>
    /// Loads a grid from file using the MapDataService. Returns success state.
    /// </summary>
    public bool LoadMap(string fileName = "default_map")
    {
        return mapDataService?.LoadMap(this, fileName) ?? false;
    }

    #endregion
}

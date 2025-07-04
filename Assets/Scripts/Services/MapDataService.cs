// Assets/Scripts/Services/MapDataService.cs

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages conversion between HexGridManager runtime state and serializable MapData structures.
/// Delegates file-level persistence to an ISaveSystem implementation.
/// </summary>
public class MapDataService 
{
    private ISaveSystem saveSystem;

    private HexTileData CellToData(HexCell cell)
    {
        return new HexTileData
        {
            q = cell.coord.q,
            r = cell.coord.r,
            type = cell.terrainType,
            cost = cell.movementCost,
            label = cell.label ?? "",
            alpha = cell.alpha
        };
    }

    private void ApplyDataToCell(HexTileData data, HexCell cell)
    {
        cell.coord = new HexCoord(data.q, data.r);
        cell.SetProperties(data.type, data.cost);
        cell.label = data.label ?? "";
        cell.alpha = data.alpha;
    }

    /// <summary>
    /// Creates a new MapData structure based on the state of the given HexGridManager.
    /// </summary>
    public MapData CreateMapDataFromGrid(HexGridManager gridManager)
    {
        MapData mapData = new MapData
        {
            width = gridManager.width,
            height = gridManager.height,
            hexSize = gridManager.hexSize,
            tiles = new List<HexTileData>()
        };

        foreach (Transform child in gridManager.transform)
        {
            HexCell cell = child.GetComponent<HexCell>();
            if (cell != null)
            {
                mapData.tiles.Add(CellToData(cell));
            }
        }
        mapData.version = 2;
        return mapData;
    }

    /// <summary>
    /// Applies a MapData structure to a grid by regenerating tiles and applying per-tile values.
    /// </summary>
    public void ApplyMapDataToGrid(MapData mapData, HexGridManager gridManager)
    {
        if (mapData == null)
        {
            Debug.LogError("Cannot apply null map data");
            return;
        }

        gridManager.width = mapData.width;
        gridManager.height = mapData.height;
        gridManager.hexSize = mapData.hexSize;
        gridManager.RegenerateGrid(mapData.width, mapData.height);

        Dictionary<string, HexTileData> tileDataLookup = mapData.tiles
            .ToDictionary(t => $"{t.q},{t.r}", t => t);

        foreach (Transform child in gridManager.transform)
        {
            HexCell cell = child.GetComponent<HexCell>();
            if (cell != null)
            {
                string key = $"{cell.coord.q},{cell.coord.r}";
                if (tileDataLookup.TryGetValue(key, out HexTileData tileData))
                {
                    ApplyDataToCell(tileData, cell);

                    HexTileVisuals visuals = cell.GetComponent<HexTileVisuals>();
                    if (visuals != null)
                    {
                        // Refresh visuals to apply transparency immediately
                        visuals.RefreshVisuals();
                    }
                }
            }
        }

        Debug.Log($"Applied map data: {mapData.width}x{mapData.height} grid with {mapData.tiles.Count} tiles");
    }

    /// <summary>
    /// Returns a representative terrain color for a given terrain type string.
    /// </summary>
    private Color GetTerrainColor(string terrainType)
    {
        return terrainType switch
        {
            "Plain" => new Color(0.5f, 0.6f, 0.5f),
            "Forest" => new Color(0.2f, 0.5f, 0.2f),
            "Mountain" => new Color(0.6f, 0.6f, 0.6f),
            "Water" => new Color(0.3f, 0.5f, 1f),
            _ => Color.white
        };
    }
}

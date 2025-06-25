// Assets/Scripts/HexCell.cs

using UnityEngine;

/// <summary>
/// Represents a single tile in the hex grid.
/// Stores position, terrain type, and movement cost.
/// </summary>
public class HexCell : MonoBehaviour
{
    /// <summary>
    /// Axial grid coordinates of the hex tile.
    /// </summary>
    public HexCoord coord;

    /// <summary>
    /// World-space position of the tile.
    /// </summary>
    public Vector3 worldPosition;

    /// <summary>
    /// Type of terrain assigned to this tile.
    /// </summary>
    public string terrainType = "Plain";

    /// <summary>
    /// Movement cost associated with this tile's terrain.
    /// </summary>
    public int movementCost = 1;

    /// <summary>
    /// Optional label for this tile.
    /// </summary>
    public string label = "";

    /// <summary>
    /// Initializes tile data and sets its world position.
    /// </summary>
    public void Init(HexCoord coord, Vector3 position)
    {
        this.coord = coord;
        this.worldPosition = position;
        transform.position = position;
        name = $"Hex {coord.q},{coord.r}";
    }

    /// <summary>
    /// Updates the terrain type and movement cost for the tile.
    /// </summary>
    public void SetProperties(string type, int cost)
    {
        terrainType = type;
        movementCost = cost;
    }
}

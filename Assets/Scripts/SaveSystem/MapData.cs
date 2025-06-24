// Assets/Scripts/SaveSystem/MapData.cs

using System.Collections.Generic;

/// <summary>
/// Serializable structure representing an entire hex map layout.
/// Includes size, tile data, and metadata.
/// </summary>
[System.Serializable]
public class MapData
{
    /// <summary>
    /// Number of tiles across the horizontal axis.
    /// </summary>
    public int width;

    /// <summary>
    /// Number of tiles down the vertical axis.
    /// </summary>
    public int height;

    /// <summary>
    /// The size (scale) of individual hex tiles.
    /// </summary>
    public float hexSize;

    /// <summary>
    /// Serialized tile data for every hex in the grid.
    /// </summary>
    public List<HexTileData> tiles = new();

    /// <summary>
    /// Optional: map display name or identifier.
    /// </summary>
    public string mapName;

    /// <summary>
    /// Optional: creation timestamp string.
    /// </summary>
    public string createdDate;

    /// <summary>
    /// Optional: last modified timestamp string.
    /// </summary>
    public string lastModified;

    /// <summary>
    /// Map format version number, used for compatibility.
    /// </summary>
    public int version = 1;
}

/// <summary>
/// Serializable structure representing a single hex tile on the map.
/// Includes coordinates and gameplay-relevant properties.
/// </summary>
[System.Serializable]
public class HexTileData
{
    public int q, r;
    public string type;
    public int cost;

    /// <summary>
    /// Creates a new tile data object from raw values.
    /// </summary>
    public HexTileData(int q, int r, string type, int cost)
    {
        this.q = q;
        this.r = r;
        this.type = type;
        this.cost = cost;
    }

    /// <summary>
    /// Required for JSON deserialization.
    /// </summary>
    public HexTileData() { }
}

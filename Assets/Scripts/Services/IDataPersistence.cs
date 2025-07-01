// Assets/Scripts/SaveSystem/IDataPersistence.cs

/// <summary>
/// Interface for components that participate in saving/loading persistent map data.
/// </summary>
public interface IDataPersistence
{
    /// <summary>
    /// Returns the current state of the object as a MapData structure.
    /// </summary>
    MapData SaveData();

    /// <summary>
    /// Applies loaded MapData to restore this object's state.
    /// </summary>
    bool LoadData(MapData data);
}

/// <summary>
/// Interface for the central save/load system.
/// </summary>
public interface ISaveSystem
{
    /// <summary>
    /// Saves the given MapData to a file.
    /// </summary>
    bool SaveMap(MapData mapData, string fileName = "default_map");

    /// <summary>
    /// Loads and returns map data from the given file.
    /// </summary>
    MapData LoadMap(string fileName = "default_map");

    /// <summary>
    /// Checks whether a map file exists.
    /// </summary>
    bool MapExists(string fileName = "default_map");

    /// <summary>
    /// Returns all available map file names.
    /// </summary>
    string[] GetAvailableMaps();
}

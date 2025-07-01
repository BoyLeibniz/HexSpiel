// Assets/Scripts/Services/JsonSaveSystem.cs

using UnityEngine;
using System.IO;
using System;
using System.Linq;

/// <summary>
/// Implementation of ISaveSystem that saves and loads MapData using JSON serialization.
/// Files are stored in Application.persistentDataPath/Maps.
/// </summary>
public class JsonSaveSystem : ISaveSystem
{
    private readonly string saveDirectory;
    private const string FILE_EXTENSION = ".json";

    /// <summary>
    /// Initializes the save system and ensures the save directory exists.
    /// </summary>
    public JsonSaveSystem()
    {
        saveDirectory = Path.Combine(Application.persistentDataPath, "Maps");

        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }
    }

    /// <summary>
    /// Serializes and saves the given MapData to disk.
    /// </summary>
    public bool SaveMap(MapData mapData, string fileName = "default_map")
    {
        try
        {
            mapData.lastModified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            if (string.IsNullOrEmpty(mapData.createdDate))
            {
                mapData.createdDate = mapData.lastModified;
            }
            if (string.IsNullOrEmpty(mapData.mapName))
            {
                mapData.mapName = fileName;
            }

            string json = JsonUtility.ToJson(mapData, true);
            string filePath = GetFilePath(fileName);
            File.WriteAllText(filePath, json);

            Debug.Log($"Map saved successfully to: {filePath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save map: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Loads MapData from disk by deserializing the JSON file.
    /// </summary>
    public MapData LoadMap(string fileName = "default_map")
    {
        try
        {
            string filePath = GetFilePath(fileName);

            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"Map file not found: {filePath}");
                return null;
            }

            string json = File.ReadAllText(filePath);
            MapData mapData = JsonUtility.FromJson<MapData>(json);

            Debug.Log($"Map loaded successfully from: {filePath}");
            return mapData;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load map: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Returns true if the specified map file exists.
    /// </summary>
    public bool MapExists(string fileName = "default_map")
    {
        return File.Exists(GetFilePath(fileName));
    }

    /// <summary>
    /// Returns a list of available map filenames in the save directory.
    /// </summary>
    public string[] GetAvailableMaps()
    {
        try
        {
            if (!Directory.Exists(saveDirectory))
            {
                Debug.LogWarning($"Map directory not found: {saveDirectory}");
                return Array.Empty<string>();
            }

            var files = Directory.GetFiles(saveDirectory, $"*{FILE_EXTENSION}");

            return files.Select(Path.GetFileNameWithoutExtension)
                        .Where(name => !string.IsNullOrWhiteSpace(name))
                        .Select(name => name.Replace("_", " ")) // Show friendly display names
                        .Distinct()
                        .OrderBy(name => name)
                        .ToArray();
        }
        catch (Exception e)
        {
            Debug.LogError($"[GetAvailableMaps] Failed to list maps: {e}");
            return Array.Empty<string>();
        }
    }

    /// <summary>
    /// Returns the full file path for a given map name.
    /// </summary>
    private string GetFilePath(string fileName)
    {
        return Path.Combine(saveDirectory, fileName + FILE_EXTENSION);
    }
}

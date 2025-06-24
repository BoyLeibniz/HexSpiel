// Assets/Scripts/SaveSystem/DataPersistenceManager.cs

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages save/load delegation between the save system and a single IDataPersistence target in the scene.
/// Automatically locates the first available implementer on Awake.
/// </summary>
public class DataPersistenceManager : MonoBehaviour
{
    [Header("Save Settings")]
    [SerializeField] private string fileName = "default_map";

    private ISaveSystem saveSystem;
    private IDataPersistence dataPersistenceTarget;

    public string GetFileName() => fileName;

    public void SetFileName(string name)
    {
        fileName = name;
    }

    void Awake()
    {
        saveSystem = new JsonSaveSystem();

        var found = FindObjectsOfType<MonoBehaviour>(true)
            .OfType<IDataPersistence>()
            .ToList();

        if (found.Count == 0)
        {
            Debug.LogError("No IDataPersistence implementers found.");
        }
        else if (found.Count > 1)
        {
            Debug.LogWarning("Multiple IDataPersistence implementers found; using the first.");
        }

        dataPersistenceTarget = found.FirstOrDefault();
    }

    /// <summary>
    /// Triggers save via the active IDataPersistence implementer and writes to disk.
    /// </summary>
    public bool SaveGame()
    {
        if (dataPersistenceTarget == null) return false;
        MapData data = dataPersistenceTarget.SaveData();
        return saveSystem.SaveMap(data, fileName);
    }

    /// <summary>
    /// Loads default MapData from disk and passes it to the active IDataPersistence implementer.
    /// </summary>
    public bool LoadGame()
    {
        MapData data = saveSystem.LoadMap(fileName);
        if (data == null) return false;

        return dataPersistenceTarget?.LoadData(data) ?? false;
    }

    /// <summary>
    /// Loads the named MapData from disk and passes it to the active IDataPersistence implementer.
    /// </summary>
    public bool LoadGame(string name)
    {
        MapData data = saveSystem.LoadMap(name);
        if (data == null) return false;

        SetFileName(name); // Keep filename in sync for future saves
        return dataPersistenceTarget?.LoadData(data) ?? false;
    }
}

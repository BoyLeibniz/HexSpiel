// Assets/Scripts/HexMap/HexTooltipManager.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the display of tooltips for all hex tiles in the grid.
/// Handles toggle state, efficient creation/destruction, and lifecycle events.
/// </summary>
public class HexTooltipManager : MonoBehaviour
{
    [Header("Tooltip Settings")]
    [SerializeField] public HexGridManager gridManager;
    [SerializeField] private GameObject tooltipPrefab;
    [SerializeField] private KeyCode toggleKey = KeyCode.T;
    [SerializeField] private bool requireCtrlModifier = true;

    private bool tooltipsEnabled = false;
    private Dictionary<HexCell, HexTooltip> activeTooltips = new Dictionary<HexCell, HexTooltip>();
    private Queue<HexTooltip> tooltipPool = new();

    void Awake()
    {
        // Create default tooltip prefab if none assigned
        if (tooltipPrefab == null)
        {
            CreateDefaultTooltipPrefab();
        }
    }

    void Update()
    {
        // Handle toggle input
        bool ctrlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool shouldToggle = requireCtrlModifier ? (ctrlPressed && Input.GetKeyDown(toggleKey)) : Input.GetKeyDown(toggleKey);

        if (shouldToggle)
        {
            ToggleTooltips();
        }
    }

    /// <summary>
    /// Toggle tooltip display on/off.
    /// </summary>
    public void ToggleTooltips()
    {
        tooltipsEnabled = !tooltipsEnabled;

        if (tooltipsEnabled)
        {
            ShowTooltips();
        }
        else
        {
            HideTooltips();
        }

        Debug.Log($"Hex tooltips {(tooltipsEnabled ? "enabled" : "disabled")}");
    }

    /// <summary>
    /// Force tooltips on.
    /// </summary>
    public void ShowTooltips()
    {
        tooltipsEnabled = true;
        RefreshAllTooltips();
    }

    /// <summary>
    /// Force tooltips off.
    /// </summary>
    public void HideTooltips()
    {
        tooltipsEnabled = false;
        foreach (var tooltip in activeTooltips.Values)
        {
            if (tooltip != null)
                tooltip.SetVisible(false);
        }
    }

    /// <summary>
    /// Refresh all tooltips based on current grid state.
    /// Call this after loading a map or when cell properties change.
    /// </summary>
    public void RefreshAllTooltips()
    {
        if (gridManager == null) return;

        // Clean up destroyed tooltips
        CleanupDestroyedTooltips();

        // Process all hex cells in the grid
        foreach (Transform child in gridManager.transform)
        {
            HexCell cell = child.GetComponent<HexCell>();
            if (cell == null) continue;

            bool hasContent = !string.IsNullOrEmpty(cell.label);

            if (hasContent)
            {
                // Create or update tooltip
                if (!activeTooltips.ContainsKey(cell))
                {
                    CreateTooltipForCell(cell);
                }
                else
                {
                    activeTooltips[cell].UpdateContent();
                }

                // Set visibility based on toggle state
                activeTooltips[cell].SetVisible(tooltipsEnabled);
            }
            else
            {
                // Remove tooltip if no content
                RemoveTooltipForCell(cell);
            }
        }
    }

    /// <summary>
    /// Create a tooltip for a specific cell.
    /// </summary>
    private void CreateTooltipForCell(HexCell cell)
    {
        if (activeTooltips.ContainsKey(cell)) return;

        HexTooltip tooltip;
        if (tooltipPool.Count > 0)
        {
            tooltip = tooltipPool.Dequeue();
            tooltip.gameObject.SetActive(true);
        }
        else
        {
            GameObject tooltipGO = Instantiate(tooltipPrefab, transform);
            tooltip = tooltipGO.GetComponent<HexTooltip>() ?? tooltipGO.AddComponent<HexTooltip>();
        }

        tooltip.Initialize(cell);
        activeTooltips[cell] = tooltip;
    }

    /// <summary>
    /// Remove tooltip for a specific cell.
    /// </summary>
    private void RemoveTooltipForCell(HexCell cell)
    {
        if (activeTooltips.TryGetValue(cell, out HexTooltip tooltip))
        {
            if (tooltip != null)
            {
                tooltip.gameObject.SetActive(false);
                tooltipPool.Enqueue(tooltip);
            }
            activeTooltips.Remove(cell);
        }
    }

    /// <summary>
    /// Clean up tooltips for destroyed cells.
    /// </summary>
    private void CleanupDestroyedTooltips()
    {
        var keysToRemove = new List<HexCell>();

        foreach (var kvp in activeTooltips)
        {
            if (kvp.Key == null || kvp.Value == null)
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            if (activeTooltips.TryGetValue(key, out HexTooltip tooltip) && tooltip != null)
            {
                DestroyImmediate(tooltip.gameObject);
            }
            activeTooltips.Remove(key);
        }
    }

    /// <summary>
    /// Clear all tooltips. Call this when the grid is destroyed/reset.
    /// </summary>
    public void ClearAllTooltips()
    {
        foreach (var tooltip in activeTooltips.Values)
        {
            if (tooltip != null)
            {
                tooltip.gameObject.SetActive(false);
                tooltipPool.Enqueue(tooltip);
            }
        }
        activeTooltips.Clear();
    }

    /// <summary>
    /// Create a basic tooltip prefab if none is assigned.
    /// </summary>
    private void CreateDefaultTooltipPrefab()
    {
        GameObject prefab = new GameObject("DefaultTooltipPrefab");
        prefab.AddComponent<HexTooltip>();

        // Store as prefab reference
        tooltipPrefab = prefab;

        // Don't leave it in the scene
        prefab.SetActive(false);
    }

    /// <summary>
    /// Update a specific cell's tooltip when its properties change.
    /// </summary>
    public void UpdateCellTooltip(HexCell cell)
    {
        if (cell == null) return;

        bool hasContent = !string.IsNullOrEmpty(cell.label);

        if (hasContent)
        {
            if (!activeTooltips.ContainsKey(cell))
            {
                CreateTooltipForCell(cell);
            }
            else
            {
                activeTooltips[cell].UpdateContent();
            }

            activeTooltips[cell].SetVisible(tooltipsEnabled);
        }
        else
        {
            RemoveTooltipForCell(cell);
        }
    }

    void OnDestroy()
    {
        ClearAllTooltips();
    }
}
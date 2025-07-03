// Assets/UI/HexMap/HexInspectorController.cs
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Controls the inspector UI for interacting with and modifying hex tiles.
/// Handles grid sizing, tile selection, terrain type assignment, and save/load.
/// </summary>
public class HexInspectorController : MonoBehaviour
{
    public UIDocument uiDocument;
    public HexGridManager hexGridManager; // <- Drag reference in Inspector
    public DataPersistenceManager dataPersistenceManager;
    public MapDataService mapDataService;

    // UI element references
    private ComboBoxField mapNameField;

    private IntegerField gridWidthField;
    private IntegerField gridHeightField;
    private Button saveButton;
    private Button loadButton;
    private Button resetButton;
    private Button applyButton;

    private Label coordLabel;
    private DropdownField typeDropdown;
    private IntegerField costField;
    private TextField labelField;
    private Slider alphaSlider;
    private Label alphaValueLabel;
    private Toggle showTransparencyToggle;

    // Currently selected hexes and drag state
    private List<HexCell> selectedHexes = new();

    // Terrain templates available to apply to tiles
    private List<TerrainTemplate> templates = new()
    {
        new TerrainTemplate("Plain",    new Color(0.5f, 0.6f, 0.5f), 1),
        new TerrainTemplate("Forest",   new Color(0.2f, 0.5f, 0.2f), 2),
        new TerrainTemplate("Mountain", new Color(0.6f, 0.6f, 0.6f), 3),
        new TerrainTemplate("Water",    new Color(0.3f, 0.5f, 1f),  999)
    };

    private void OnEnable()
    {
        // Initialize UI document root and keyboard listeners
        var root = uiDocument.rootVisualElement;
        root.focusable = true;
        root.Focus();

        // Setup map name input field
        var mapContainer = root.Q<VisualElement>("mapNameFieldContainer");
        mapNameField = new ComboBoxField(uiDocument.rootVisualElement);  // No callback needed, you handle on click
        mapNameField.OnDropdownOpening = () =>
        {
            var maps = mapDataService?.GetAvailableMaps() ?? Array.Empty<string>();
            mapNameField.SetItems(new List<string>(maps));
        };

        mapContainer.Clear();
        mapContainer.Add(mapNameField);

        // Setup grid size input fields
        gridWidthField = root.Q<IntegerField>("gridWidthField");
        gridHeightField = root.Q<IntegerField>("gridHeightField");
        gridWidthField.value = hexGridManager.width;
        gridHeightField.value = hexGridManager.height;

        // Setup Save button
        saveButton = root.Q<Button>("saveButton");
        saveButton.clicked += () =>
        {
            string rawName = mapNameField?.value?.Trim();
            string safeName = string.IsNullOrEmpty(rawName)
                ? "default_map"
                : rawName.Replace(" ", "_");

            dataPersistenceManager?.SetFileName(safeName);

            bool success = dataPersistenceManager.SaveGame();
            Debug.Log(success ? $"Map saved as '{safeName}'" : "Failed to save map!");
        };

        // Setup Load button
        loadButton = root.Q<Button>("loadButton");
        loadButton.clicked += () =>
        {
            string rawName = mapNameField?.value?.Trim();
            string safeName = string.IsNullOrEmpty(rawName)
                ? "default_map"
                : rawName.Replace(" ", "_");

            bool success = dataPersistenceManager.LoadGame(safeName);

            if (success)
            {
                Debug.Log("Map loaded successfully!");
                UpdateGridUI(hexGridManager.width, hexGridManager.height);
                ClearSelection();
                RefreshAllHexVisuals(); // Refresh visuals after load
            }
            else
            {
                Debug.LogError("Failed to load map!");
            }
        };

        // Setup Reset button
        resetButton = root.Q<Button>("resetButton");
        resetButton.clicked += () =>
        {
            int width = Mathf.Max(1, gridWidthField.value);
            int height = Mathf.Max(1, gridHeightField.value);
            Debug.Log($"Regenerating grid: {width} x {height}");
            hexGridManager.RegenerateGrid(width, height);
        };

        // Bind hex properties controls
        coordLabel = root.Q<Label>("coordLabel");
        typeDropdown = root.Q<DropdownField>("typeDropdown");
        costField = root.Q<IntegerField>("costField");
        labelField = root.Q<TextField>("labelField");
        costField.value = 1;
        labelField.value = "";
        alphaSlider = root.Q<Slider>("alphaSlider");
        alphaSlider.pageSize = 0.01f; // Allow fine values for slider
        alphaSlider.value = 1f; // Default alpha value
        alphaValueLabel = root.Q<Label>("alphaValueLabel");

        // Setup Show Transparency toggle
        showTransparencyToggle = root.Q<Toggle>("showTransparencyToggle");
        showTransparencyToggle.value = true; // Default to showing transparency
        showTransparencyToggle.RegisterValueChangedCallback(evt =>
        {
            RefreshAllHexVisuals();
        });

        // Setup the value label update
        alphaSlider.RegisterValueChangedCallback(evt =>
        {
            alphaValueLabel.text = $"{evt.newValue:0.00}";
        });

        // Populate dropdown and default selection
        typeDropdown.choices = templates.Select(t => t.name).ToList();
        typeDropdown.SetValueWithoutNotify(templates[0].name);
        costField.SetValueWithoutNotify(templates[0].movementCost);

        // When dropdown changes, update cost + highlight selection
        typeDropdown.RegisterValueChangedCallback(evt =>
        {
            TerrainTemplate template = templates.FirstOrDefault(t => t.name == evt.newValue);
            costField.SetValueWithoutNotify(template.movementCost);
            foreach (var cell in selectedHexes)
                cell.GetComponent<HexTileVisuals>()?.SetSelected(true);
        });

        // Apply terrain properties to all selected tiles
        applyButton = root.Q<Button>("applyButton");
        applyButton.clicked += () =>
        {
            string selectedType = typeDropdown.value;
            TerrainTemplate template = templates.FirstOrDefault(t => t.name == selectedType);
            if (template == null) return;

            string rawLabel = labelField.value ?? "";
            bool applyLabel = rawLabel != "-- Mixed --";
            float newAlpha = alphaSlider.value;
            bool applyAlpha = alphaValueLabel.text != "-- Mixed";

            foreach (var cell in selectedHexes)
            {
                // Apply terrain type and cost to model
                cell.SetProperties(template.name, template.movementCost);

                // Apply label if eligible
                if (applyLabel)
                    cell.label = rawLabel;

                // Apply alpha if changed
                if (applyAlpha)
                    cell.alpha = newAlpha;

                // Update tooltip
                if (hexGridManager.tooltipManager != null)
                    hexGridManager.tooltipManager.UpdateCellTooltip(cell);

                // Trigger visual refresh from model state
                var vis = cell.GetComponent<HexTileVisuals>();
                vis?.RefreshVisuals();
            }

            ClearSelection();
        };
    }

    /// <summary>
    /// Update UI values with the width and height of the map grid in hexes.
    /// </summary>
    public void UpdateGridUI(int width, int height)
    {
        gridWidthField.value = width;
        gridHeightField.value = height;
    }

    /// <summary>
    /// Update UI values with basic hex properties.
    /// </summary>
    public void UpdateSelectedHex(Vector2Int coords, string type, int cost)
    {
        coordLabel.text = $"({coords.x}, {coords.y})";
        typeDropdown.value = type;
        costField.value = cost;
    }

    /// <summary>
    /// Add a hex to the selection list without duplication.
    /// </summary>
    public void AddToSelection(HexCell cell)
    {
        if (!selectedHexes.Contains(cell))
        {
            selectedHexes.Add(cell);
            UpdateSelectionUI();
        }
    }

    /// <summary>
    /// Remove a hex safely from the selection list.
    /// </summary>
    public void RemoveFromSelection(HexCell cell)
    {
        if (selectedHexes.Contains(cell))
        {
            selectedHexes.Remove(cell);
            UpdateSelectionUI();
        }
    }

    /// <summary>
    /// Clear the entire hex selection list.
    /// </summary>
    public void ClearSelection()
    {
        // Clean up any destroyed cells before processing
        selectedHexes = selectedHexes
            .Where(cell => cell != null)
            .ToList();

        foreach (var cell in selectedHexes)
        {
            var vis = cell.GetComponent<HexTileVisuals>();
            if (vis != null)
            {
                vis.SetSelected(false);
            }
        }
        selectedHexes.Clear();
        UpdateSelectionUI();
    }

    /// <summary>
    /// Add all available hexes to the selection list.
    /// </summary>
    public void SelectAllHexes()
    {
        selectedHexes.Clear();

        foreach (Transform child in hexGridManager.transform)
        {
            HexCell cell = child.GetComponent<HexCell>();
            if (cell != null)
                selectedHexes.Add(cell);

            HexTileVisuals vis = child.GetComponent<HexTileVisuals>();
            if (vis != null)
            {
                vis.SetSelected(true);
            }
        }

        UpdateSelectionUI();
    }

    // Refreshes the UI to reflect the current selection state
    private void UpdateSelectionUI()
    {
        if (selectedHexes.Count == 0)
        {
            coordLabel.text = ""; // No hex selected
            typeDropdown.SetValueWithoutNotify(templates[0].name);
            costField.SetValueWithoutNotify(templates[0].movementCost);
            labelField.SetValueWithoutNotify("");
            alphaSlider.SetEnabled(false);
            alphaValueLabel.text = "";
            return;
        }

        if (selectedHexes.Count == 1)
        {
            var cell = selectedHexes[0];
            coordLabel.text = $"({cell.coord.q}, {cell.coord.r})";
            typeDropdown.SetValueWithoutNotify(cell.terrainType);
            costField.SetValueWithoutNotify(cell.movementCost);
            labelField.SetValueWithoutNotify(cell.label ?? "");
            alphaSlider.SetValueWithoutNotify(cell.alpha);
            alphaValueLabel.text = $"{cell.alpha:0.00}";
            alphaValueLabel.RemoveFromClassList("mixed");
            alphaSlider.SetEnabled(true);
        }
        else
        {
            coordLabel.text = $"[{selectedHexes.Count} selected]";

            var firstType = selectedHexes[0].terrainType;
            bool allSameType = selectedHexes.All(h => h.terrainType == firstType);
            typeDropdown.SetValueWithoutNotify(allSameType ? firstType : "-- Mixed --");

            costField.SetValueWithoutNotify(allSameType ? selectedHexes[0].movementCost : 0);

            var firstLabel = selectedHexes[0].label ?? "";
            bool allSameLabel = selectedHexes.All(h => (h.label ?? "") == firstLabel);
            labelField.SetValueWithoutNotify(allSameLabel ? firstLabel : "-- Mixed --");

            alphaSlider.SetEnabled(true);
            var uniqueAlphas = selectedHexes.Select(h => h.alpha).Distinct().ToList();
            if (uniqueAlphas.Count == 1)
            {
                float alpha = uniqueAlphas[0];
                alphaSlider.SetValueWithoutNotify(alpha);
                alphaValueLabel.text = $"{alpha:0.00}";
                alphaValueLabel.RemoveFromClassList("mixed");
            }
            else
            {
                alphaSlider.SetValueWithoutNotify(0.5f); // Parked visually
                alphaValueLabel.text = "-- Mixed --";
                alphaValueLabel.AddToClassList("mixed");
            }
        }
    }

    // Handle keyboard input while the UI has focus
    private void Update()
    {
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) &&
            Input.GetKeyDown(KeyCode.A))
        {
            SelectAllHexes();
            Debug.Log("[HexInspectorController.UpdateSelectionUI.Update] Ctrl+A pressed - All hexes selected.");
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClearSelection();
            Debug.Log("[HexInspectorController.UpdateSelectionUI.Update] Escape pressed - All hexes deselected.");
        }
    }

    /// <summary>
    /// Returns the color of the hex Type currently selected in the UI.
    /// </summary>
    public Color GetSelectedTerrainColor()
    {
        var selectedType = typeDropdown?.value;
        if (string.IsNullOrEmpty(selectedType) || selectedType == "-- Mixed --")
        {
            selectedType = selectedHexes[0].terrainType;  // First tile selected 
        }

        return GetTerrainColor(selectedType);
    }

    /// <summary>
    /// Returns the color of the passed hex Type 
    /// </summary>
    public Color GetTerrainColor(string type)
    {
        var template = templates.FirstOrDefault(t => t.name == type);
        return template?.color ?? new Color(0.5f, 0.5f, 0.5f);  // Default gray
    }

    /// <summary>
    /// Returns whether transparency should be shown based on the toggle state.
    /// </summary>
    public bool GetShowTransparency()
    {
        return showTransparencyToggle?.value ?? true;
    }

    /// <summary>
    /// Refreshes all hex visuals in the grid (useful after loading or toggling transparency).
    /// </summary>
    private void RefreshAllHexVisuals()
    {
        foreach (Transform child in hexGridManager.transform)
        {
            var visuals = child.GetComponent<HexTileVisuals>();
            visuals?.RefreshVisuals();
        }
    }

}

// Assets/UI/HexMap/HexEditorController.cs
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Controls the UI for interacting with and modifying hex tiles.
/// Handles grid sizing, tile selection, terrain type assignment, and save/load.
/// </summary>
public class HexEditorController : MonoBehaviour
{
    public UIDocument uiDocument; // <- Drag references in Inspector for these public fields
    public DataPersistenceManager dataPersistenceManager;
    public HexGridManager hexGridManager; 
    public UITooltipManager tooltipManager;
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
    private TextField costField;
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
            var maps = dataPersistenceManager?.GetAvailableMaps() ?? Array.Empty<string>();
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
        costField = root.Q<TextField>("costField");
        costField.value = "1";
        costField.tooltip = "Movement cost of the hex tile.\n" +
                            "Must be a positive integer.\n" +
                            "Use '-- Mixed --' to indicate mixed selection.";
        tooltipManager.ObserveTooltip(costField);
        labelField = root.Q<TextField>("labelField");
        labelField.value = "";

        // Add validation for cost field
        costField.RegisterValueChangedCallback(evt => ValidateCostField());

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
        costField.SetValueWithoutNotify(templates[0].movementCost.ToString());

        // When dropdown changes, update cost + highlight selection
        typeDropdown.RegisterValueChangedCallback(evt =>
        {
            TerrainTemplate template = templates.FirstOrDefault(t => t.name == evt.newValue);
            costField.SetValueWithoutNotify(template.movementCost.ToString());
            foreach (var cell in selectedHexes)
                cell.GetComponent<HexTileVisuals>()?.SetSelected(true);
            UpdateApplyButtonState(); // Update button state when terrain type changes
        });

        // Register change handlers for other input fields to update apply button state
        costField.RegisterValueChangedCallback(evt => UpdateApplyButtonState());
        labelField.RegisterValueChangedCallback(evt => UpdateApplyButtonState());
        alphaSlider.RegisterValueChangedCallback(evt => UpdateApplyButtonState());

        // Apply terrain properties to all selected tiles
        applyButton = root.Q<Button>("applyButton");
        applyButton.tooltip = "Click to apply changes to selected hexes";
        applyButton.clicked += () =>
        {
            if (selectedHexes.Count == 0) return;

            // Validate cost field before applying any changes
            if (!ValidateCostField())
            {
                Debug.LogWarning("Cannot apply changes: Invalid movement cost value");
                return;
            }

            string selectedType = typeDropdown.value;
            string rawLabel = labelField.value ?? "";
            float newAlpha = alphaSlider.value;
            string rawCost = costField.value ?? "";
            int newCost = 1;

            // Determine what to apply based on mixed field states
            bool applyTerrain = selectedType != "-- Mixed --" &&
                selectedHexes[0].terrainType != typeDropdown.value;
            bool applyLabel = rawLabel != "-- Mixed --" &&
                (selectedHexes[0].label ?? "") != labelField.value;
            bool applyCost = rawCost != "-- Mixed --" &&
                int.TryParse(rawCost, out newCost) &&
                selectedHexes[0].movementCost != newCost;
            bool applyAlpha = alphaValueLabel.text != "-- Mixed --" &&
                !Mathf.Approximately(selectedHexes[0].alpha, newAlpha);

            TerrainTemplate template = null;
            if (applyTerrain)
            {
                template = templates.FirstOrDefault(t => t.name == selectedType);
            }

            foreach (var cell in selectedHexes)
            {
                // Apply terrain type and cost only if not mixed
                if (applyTerrain && template != null)
                {
                    cell.SetProperties(template.name, template.movementCost);
                }
                else if (applyCost)
                {
                    // Apply cost independently if terrain isn't being changed
                    cell.movementCost = newCost;

                }

                // Apply label only if not mixed
                if (applyLabel)
                {
                    cell.label = rawLabel;
                }

                // Apply alpha only if not mixed
                if (applyAlpha)
                {
                    cell.alpha = newAlpha;
                }

                // Update tooltip
                if (hexGridManager.tooltipManager != null)
                    hexGridManager.tooltipManager.UpdateCellTooltip(cell);

                // Trigger visual refresh from model state
                cell.GetComponent<HexTileVisuals>()?.RefreshVisuals();
            }

            ClearSelection();
        };
        UpdateSelectionUI(); // ensure the UI reflects the cleared state
    }

    /// <summary>
    /// Validates the cost field input and applies visual feedback.
    /// Returns true if valid, false otherwise.
    /// </summary>
    private bool ValidateCostField()
    {

        string value = costField.value?.Trim() ?? "";

        // Allow mixed state placeholder
        if (value == "-- Mixed --")
        {
            costField.RemoveFromClassList("invalid-input");
            costField.tooltip = "";
            return true;
        }

        // Validate positive integer
        if (int.TryParse(value, out int cost) && cost > 0)
        {
            costField.RemoveFromClassList("invalid-input");
            costField.tooltip = "";
            return true;
        }

        // Invalid input - apply error styling
        costField.AddToClassList("invalid-input");
        costField.tooltip = "Movement cost must be a positive integer";
        return false;
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
        costField.value = cost.ToString();
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
            coordLabel.text = "";

            typeDropdown.SetValueWithoutNotify(templates[0].name);
            typeDropdown.SetEnabled(false);

            costField.SetValueWithoutNotify(templates[0].movementCost.ToString());
            costField.SetEnabled(false);

            labelField.SetValueWithoutNotify("");
            labelField.SetEnabled(false);

            alphaSlider.SetValueWithoutNotify(1f);
            alphaSlider.SetEnabled(false);
            alphaValueLabel.text = "";

            // Disable apply button when no hexes selected
            applyButton.SetEnabled(false);
            return;
        }
        if (selectedHexes.Count == 1)
        {
            var cell = selectedHexes[0];
            coordLabel.text = $"({cell.coord.q}, {cell.coord.r})";
            typeDropdown.SetValueWithoutNotify(cell.terrainType);
            typeDropdown.SetEnabled(true);
            costField.SetValueWithoutNotify(cell.movementCost.ToString());
            costField.SetEnabled(true);
            labelField.SetValueWithoutNotify(cell.label ?? "");
            labelField.SetEnabled(true);
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
            typeDropdown.SetEnabled(true);

            var firstCost = selectedHexes[0].movementCost;
            bool allSameCost = selectedHexes.All(h => h.movementCost == firstCost);
            costField.SetValueWithoutNotify(allSameCost ? firstCost.ToString() : "-- Mixed --");
            costField.SetEnabled(true);

            var firstLabel = selectedHexes[0].label ?? "";
            bool allSameLabel = selectedHexes.All(h => (h.label ?? "") == firstLabel);
            labelField.SetValueWithoutNotify(allSameLabel ? firstLabel : "-- Mixed --");
            labelField.SetEnabled(true);

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
            alphaSlider.SetEnabled(true);
        }

        // Validate cost field after updating
        ValidateCostField();

        // Update apply button state based on whether there are changes to apply
        UpdateApplyButtonState();
    }

    /// <summary>
    /// Updates the enabled state of the Apply button based on whether there are changes to apply.
    /// </summary>
    private void UpdateApplyButtonState()
    {
        if (selectedHexes.Count == 0)
        {
            applyButton.SetEnabled(false);
            return;
        }

        bool hasChanges = HasChangesToApply();
        applyButton.SetEnabled(hasChanges);
    }

    /// <summary>
    /// Checks if there are any changes that can be applied to the selected hexes.
    /// Returns true if there are changes to apply, false otherwise.
    /// </summary>
    private bool HasChangesToApply()
    {
        if (selectedHexes.Count == 0) return false;

        string selectedType = typeDropdown.value;
        string rawLabel = labelField.value ?? "";
        float newAlpha = alphaSlider.value;
        string rawCost = costField.value ?? "";
        int newCost = 1;

        // Check if any field has changed from the current hex values
        bool terrainChanged = selectedType != "-- Mixed --" &&
            selectedHexes[0].terrainType != typeDropdown.value;
        bool labelChanged = rawLabel != "-- Mixed --" &&
            (selectedHexes[0].label ?? "") != labelField.value;
        bool costChanged = rawCost != "-- Mixed --" &&
            int.TryParse(rawCost, out newCost) &&
            selectedHexes[0].movementCost != newCost;
        bool alphaChanged = alphaValueLabel.text != "-- Mixed --" &&
            !Mathf.Approximately(selectedHexes[0].alpha, newAlpha);

        return terrainChanged || labelChanged || costChanged || alphaChanged;
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

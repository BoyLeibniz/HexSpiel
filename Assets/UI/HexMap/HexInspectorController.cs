// Assets/UI/HexMap/HexInspectorController.cs

using UnityEngine;
using UnityEngine.UIElements;
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

    // UI element references
    private TextField mapNameField;
    private IntegerField gridWidthField;
    private IntegerField gridHeightField;
    private Button saveButton;
    private Button loadButton;
    private Button regenerateButton;
    private Button applyButton;

    private Label coordLabel;
    private DropdownField typeDropdown;
    private IntegerField costField;
    private TextField labelField;
    // Currently selected hexes and drag state
    private List<HexCell> selectedHexes = new();
    private bool isDragging = false;

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
        root.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);

        // Setup map name input field
        mapNameField = root.Q<TextField>("mapNameField");
        string currentName = dataPersistenceManager != null
            ? dataPersistenceManager.GetFileName()
            : "default_map";
        // Display as blank if it's "default_map"
        mapNameField.SetValueWithoutNotify(
            currentName == "default_map" ? "" : currentName.Replace("_", " ")
        );

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
            }
            else
            {
                Debug.LogError("Failed to load map!");
            }
        };

        // Setup Regenerate button
        regenerateButton = root.Q<Button>("regenerateButton");
        regenerateButton.clicked += () =>
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
            foreach (var cell in selectedHexes)
            {
                cell.SetProperties(template.name, template.movementCost);
                if (applyLabel)
                    cell.label = rawLabel;
                var vis = cell.GetComponent<HexTileVisuals>();
                if (vis != null)
                    vis.baseColor = template.color;
            }
            ClearSelection();
        };
    }

    // Handles keyboard shortcuts while the UI document is focused
    private void OnKeyDown(KeyDownEvent evt)
    {
        // ESC clears current selection
        if (evt.keyCode == KeyCode.Escape)
        {
            ClearSelection();
            evt.StopPropagation();
        }

        // Ctrl+A selects all hexes
        if (evt.keyCode == KeyCode.A && (evt.ctrlKey || evt.commandKey))
        {
            SelectAllHexes();
            evt.StopPropagation();
        }
    }

    public void UpdateGridUI(int width, int height)
    {
        gridWidthField.value = width;
        gridHeightField.value = height;
    }

    public void UpdateSelectedHex(Vector2Int coords, string type, int cost)
    {
        coordLabel.text = $"({coords.x}, {coords.y})";
        typeDropdown.value = type;
        costField.value = cost;
    }

    public void AddToSelection(HexCell cell)
    {
        if (!selectedHexes.Contains(cell))
        {
            selectedHexes.Add(cell);
            UpdateSelectionUI();
        }
    }

    public void RemoveFromSelection(HexCell cell)
    {
        if (selectedHexes.Contains(cell))
        {
            selectedHexes.Remove(cell);
            UpdateSelectionUI();
        }
    }

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

    private void UpdateSelectionUI()
    {
        if (selectedHexes.Count == 0)
        {
            coordLabel.text = ""; // No hex selected
            typeDropdown.SetValueWithoutNotify(templates[0].name);
            costField.SetValueWithoutNotify(templates[0].movementCost);
            labelField.SetValueWithoutNotify("");
            return;
        }

        if (selectedHexes.Count == 1)
        {
            var cell = selectedHexes[0];
            coordLabel.text = $"({cell.coord.q}, {cell.coord.r})";
            typeDropdown.SetValueWithoutNotify(cell.terrainType);
            costField.SetValueWithoutNotify(cell.movementCost);
            labelField.SetValueWithoutNotify(cell.label ?? "");
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
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) isDragging = true;
        if (Input.GetMouseButtonUp(0)) isDragging = false;
        if (Input.GetMouseButton(0)) TrySelectHoveredTile();

        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) &&
            Input.GetKeyDown(KeyCode.A))
        {
            SelectAllHexes();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClearSelection();
        }
    }

    private void TrySelectHoveredTile()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            HexCell cell = hit.collider.GetComponent<HexCell>();
            if (cell != null && !selectedHexes.Contains(cell))
            {
                selectedHexes.Add(cell);
                var vis = cell.GetComponent<HexTileVisuals>();
                if (vis != null)
                    vis.SetSelected(true);
            }
        }
    }

    public Color GetSelectedTerrainColor()
    {
        var selectedType = typeDropdown?.value;
        if (string.IsNullOrEmpty(selectedType) || selectedType == "-- Mixed --")
        {
            selectedType = selectedHexes[0].terrainType;  // First tile selected 
        }

        var template = templates.FirstOrDefault(t => t.name == selectedType);
        return template?.color ?? new Color(0.5f, 0.5f, 0.5f);  // Default gray
    }
    
}

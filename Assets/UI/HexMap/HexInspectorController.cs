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
    private IntegerField gridWidthField;
    private IntegerField gridHeightField;
    private Button saveButton;
    private Button loadButton;
    private Button regenerateButton;
    private Button applyButton;

    private Label coordLabel;
    private DropdownField typeDropdown;
    private Slider costSlider;
    private Label costValue;

    // Currently selected hexes and drag state
    private List<HexCell> selectedHexes = new();
    private bool isDragging = false;

    // Terrain templates available to apply to tiles
    private List<TerrainTemplate> templates = new()
    {
        new TerrainTemplate("Plain",    new Color(0.5f, 0.6f, 0.5f), 1f),
        new TerrainTemplate("Forest",   new Color(0.2f, 0.5f, 0.2f), 2f),
        new TerrainTemplate("Mountain", new Color(0.6f, 0.6f, 0.6f), 3f),
        new TerrainTemplate("Water",    new Color(0.3f, 0.5f, 1f),  999f)
    };

    private void OnEnable()
    {
        // Initialize UI document root and keyboard listeners
        var root = uiDocument.rootVisualElement;
        root.focusable = true;
        root.Focus();
        root.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);

        // Setup grid size input fields
        gridWidthField = root.Q<IntegerField>("gridWidthField");
        gridHeightField = root.Q<IntegerField>("gridHeightField");
        gridWidthField.value = hexGridManager.width;
        gridHeightField.value = hexGridManager.height;

        // Setup Save button
        saveButton = root.Q<Button>("saveButton");
        saveButton.clicked += () =>
        {
            bool success = dataPersistenceManager.SaveGame();
            Debug.Log(success ? "Map saved successfully!" : "Failed to save map!");
        };

        // Setup Load button
        loadButton = root.Q<Button>("loadButton");
        loadButton.clicked += () =>
        {
            bool success = dataPersistenceManager.LoadGame();
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
        costSlider = root.Q<Slider>("costSlider");
        costValue = root.Q<Label>("costValue");

        // Populate dropdown and default selection
        typeDropdown.choices = templates.Select(t => t.name).ToList();
        typeDropdown.SetValueWithoutNotify(templates[0].name);
        costSlider.SetValueWithoutNotify(templates[0].movementCost);
        costValue.text = templates[0].movementCost.ToString("0");

        // When dropdown changes, update cost + highlight selection
        typeDropdown.RegisterValueChangedCallback(evt =>
        {
            TerrainTemplate template = templates.FirstOrDefault(t => t.name == evt.newValue);
            costSlider.SetValueWithoutNotify(template.movementCost);
            costValue.text = template.movementCost.ToString("0");
            foreach (var cell in selectedHexes)
                cell.GetComponent<HexTileVisuals>()?.SetSelected(true);
        });

        // Update cost value when slider changes
        costSlider.RegisterValueChangedCallback(evt =>
        {
            costValue.text = evt.newValue.ToString("0");
        });

        // Apply terrain properties to all selected tiles
        applyButton = root.Q<Button>("applyButton");
        applyButton.clicked += () =>
        {
            string selectedType = typeDropdown.value;
            TerrainTemplate template = templates.FirstOrDefault(t => t.name == selectedType);
            if (template == null) return;

            foreach (var cell in selectedHexes)
            {
                cell.SetProperties(template.name, template.movementCost);
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

    public void UpdateSelectedHex(Vector2Int coords, string type, float cost)
    {
        coordLabel.text = $"({coords.x}, {coords.y})";
        typeDropdown.value = type;
        costSlider.value = cost;
        costValue.text = cost.ToString("0");
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
            coordLabel.text = "No hex selected";
            typeDropdown.SetValueWithoutNotify(templates[0].name);
            costSlider.SetValueWithoutNotify(templates[0].movementCost);
            costValue.text = templates[0].movementCost.ToString("0");
            return;
        }

        if (selectedHexes.Count == 1)
        {
            var cell = selectedHexes[0];
            coordLabel.text = $"({cell.coord.q}, {cell.coord.r})";
            typeDropdown.SetValueWithoutNotify(cell.terrainType);
            costSlider.SetValueWithoutNotify(cell.movementCost);
            costValue.text = cell.movementCost.ToString("0");
        }
        else
        {
            coordLabel.text = $"[{selectedHexes.Count} selected]";

            var firstType = selectedHexes[0].terrainType;
            bool allSameType = selectedHexes.All(h => h.terrainType == firstType);
            typeDropdown.SetValueWithoutNotify(allSameType ? firstType : "-- Mixed --");
            costSlider.SetValueWithoutNotify(allSameType ? selectedHexes[0].movementCost : 0f);
            costValue.text = allSameType ? selectedHexes[0].movementCost.ToString("0") : "";
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

    public Color GetCurrentTerrainColor()
    {
        var selectedType = typeDropdown?.value;
        if (string.IsNullOrEmpty(selectedType) || selectedType == "-- Mixed --")
            return Color.white;

        var template = templates.FirstOrDefault(t => t.name == selectedType);
        return template?.color ?? Color.white;
    }
}

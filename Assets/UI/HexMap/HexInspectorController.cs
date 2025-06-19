using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

public class HexInspectorController : MonoBehaviour
{
    public UIDocument uiDocument;
    public HexGridManager hexGridManager; // <- Drag reference in Inspector

    private IntegerField gridWidthField;
    private IntegerField gridHeightField;
    private Button saveButton;
    private Button loadButton;
    private Button regenerateButton;

    private List<TerrainTemplate> templates = new()
{
    new TerrainTemplate("Plain",   new Color(0.5f, 0.6f, 0.5f), 1f),
    new TerrainTemplate("Forest",  new Color(0.2f, 0.5f, 0.2f), 2f),
    new TerrainTemplate("Mountain",new Color(0.6f, 0.6f, 0.6f), 3f),
    new TerrainTemplate("Water",   new Color(0.3f, 0.5f, 1f),  999f)
};
    private Label coordLabel;
    private DropdownField typeDropdown;
    private Slider costSlider;
    private Label costValue;
    private List<HexCell> selectedHexes = new();
    private Button applyButton;
    private bool isDragging = false;
    private void OnEnable()
    {
        var root = uiDocument.rootVisualElement;
        root.focusable = true;
        root.Focus();

        root.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);

        // Grid controls
        gridWidthField = root.Q<IntegerField>("gridWidthField");
        gridHeightField = root.Q<IntegerField>("gridHeightField");
        gridWidthField.value = hexGridManager.width;
        gridHeightField.value = hexGridManager.height;

        saveButton = root.Q<Button>("saveButton");

        saveButton.clicked += () =>
        {
            Debug.Log($"Saving Map");
            // Here you would call your save logic
        };

        loadButton = root.Q<Button>("loadButton");

        loadButton.clicked += () =>
        {
            Debug.Log($"Loading Map");
            // Here you would call your load logic
        };

        regenerateButton = root.Q<Button>("regenerateButton");

        regenerateButton.clicked += () =>
        {
            int width = Mathf.Max(1, gridWidthField.value);
            int height = Mathf.Max(1, gridHeightField.value);
            Debug.Log($"Regenerating grid: {width} x {height}");
            hexGridManager.RegenerateGrid(width, height);
        };

        // Sample Hex controls
        coordLabel = root.Q<Label>("coordLabel");
        typeDropdown = root.Q<DropdownField>("typeDropdown");
        costSlider = root.Q<Slider>("costSlider");
        costValue = root.Q<Label>("costValue");

        // Populate type dropdown
        typeDropdown.choices = templates.Select(t => t.name).ToList();
        // Default to first type
        typeDropdown.SetValueWithoutNotify(templates[0].name);
        costSlider.SetValueWithoutNotify(templates[0].movementCost);
        costValue.text = templates[0].movementCost.ToString("0");
        typeDropdown.RegisterValueChangedCallback(evt =>
        {
            Debug.Log($"Hex type changed to {evt.newValue}");
            TerrainTemplate template = templates.FirstOrDefault(t => t.name == evt.newValue);
            costSlider.SetValueWithoutNotify(template.movementCost);
            costValue.text = template.movementCost.ToString("0");
            // Refresh the selection color
            foreach (var cell in selectedHexes)
            {
                var vis = cell.GetComponent<HexTileVisuals>();
                if (vis != null)
                {
                    vis.SetSelected(true);
                }
            }
        });

        costSlider.RegisterValueChangedCallback(evt =>
        {
            costValue.text = evt.newValue.ToString("0");
            // TODO: Apply to selected hex
        });

        applyButton = root.Q<Button>("applyButton");
        applyButton.clicked += () =>
        {
            string selectedType = typeDropdown.value;
            TerrainTemplate template = templates.FirstOrDefault(t => t.name == selectedType);
            if (template == null) return;

            foreach (var cell in selectedHexes)
            {
                cell.SetProperties(template.name, template.movementCost);

                // Optional: colorize visual (if using HexTileVisuals)
                var vis = cell.GetComponent<HexTileVisuals>();
                if (vis != null)
                {
                    vis.baseColor = template.color;
                }
            }

            ClearSelection();
        };
    }

    // Call this if needed to reflect current size
    public void UpdateGridUI(int width, int height)
    {
        gridWidthField.value = width;
        gridHeightField.value = height;
    }

    // Call this to update the selected hex
    public void UpdateSelectedHex(Vector2Int coords, string type, float cost)
    {
        coordLabel.text = $"({coords.x}, {coords.y})";
        typeDropdown.value = type;
        costSlider.value = cost;
        costValue.text = cost.ToString("0");
    }

    // Call this to add a hex to the selection
    public void AddToSelection(HexCell cell)
    {
        if (!selectedHexes.Contains(cell))
        {
            selectedHexes.Add(cell);
            UpdateSelectionUI();
        }
    }

    // Call this to remove a hex from the selection
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
        // Reset required properties on the currently selected hexes
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
            costSlider.SetValueWithoutNotify(allSameType ? selectedHexes[0].movementCost : 0f); // or disable it
            costValue.text = allSameType ? selectedHexes[0].movementCost.ToString("0") : "";
        }
    }

    private void OnKeyDown(KeyDownEvent evt)
    {
        // ESC: clear selection
        if (evt.keyCode == KeyCode.Escape)
        {
            ClearSelection();
            evt.StopPropagation(); // prevent further processing
        }

        // Ctrl+A: select all
        if (evt.keyCode == KeyCode.A && (evt.ctrlKey || evt.commandKey))
        {
            SelectAllHexes();
            evt.StopPropagation();
        }
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

    void Update()
    {
        // Start drag
        if (Input.GetMouseButtonDown(0))
            isDragging = true;

        // End drag
        if (Input.GetMouseButtonUp(0))
            isDragging = false;

        if (Input.GetMouseButton(0))
            TrySelectHoveredTile();

        // Ctrl+A: Select All
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) &&
            Input.GetKeyDown(KeyCode.A))
        {
            SelectAllHexes();
        }

        // Escape: Clear Selection
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
                AddToSelection(cell);

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

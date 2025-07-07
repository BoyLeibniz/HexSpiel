// Assets/Scripts/HexMap/HexTileVisuals.cs
using UnityEngine;

/// <summary>
/// Controls visual appearance and interaction feedback for a single hex tile.
/// Responds to hover, selection, and terrain settings with layered transparency support.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class HexTileVisuals : MonoBehaviour
{
    /// <summary>
    /// Base color for untyped tiles (used if terrainType is empty).
    /// Do not rely on the field's initial value, it's overridden by serialized prefab
    /// Set via the Unity Inspector, should match the base material color 
    /// </summary>
    [Tooltip("Used for fallback visuals if tile has no type.")]
    public Color baseColor; // Default: #727C96, alpha 153/0.6f
    /// <summary>
    /// Highlight color used when hovering.
    /// Set in the Inspector via prefab to match desired glow intensity.
    /// </summary>
    [Tooltip("Highlight color shown when hovering.")]
    public Color hoverColor; // Default: #FFFF66, alpha 153/0.6f
    /// <summary>
    /// Highlight color used when selected.
    /// Set in the Inspector via prefab to match selection visual style.
    /// </summary>
    [Tooltip("Color used when tile is selected.")]
    public Color selectedColor; // Default: #80FF80, alpha 153/0.6f

    private HexEditorController editorController;
    private HexCell hexCell;

    /// <summary>
    /// Assigns the controller responsible for managing this tile's selection logic.
    /// </summary>
    public void SetEditor(HexEditorController controller)
    {
        this.editorController = controller;
    }

    private MaterialPropertyBlock props;
    private Renderer rend;
    private bool isHovered = false;
    private bool isSelected = false;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        hexCell = GetComponent<HexCell>();
        props = new MaterialPropertyBlock();

        // Initialize with base color and transparency
        UpdateVisualState();
    }

    void OnMouseEnter()
    {
        // If dragging, add to controller selection
        if (Input.GetMouseButton(0) && TryGetComponent<HexCell>(out var cell) && editorController != null)
        {
            isSelected = true;
            editorController.AddToSelection(cell);
        }
        isHovered = true;
        UpdateVisualState();
    }

    void OnMouseExit()
    {
        isHovered = false;
        UpdateVisualState();
    }

    void OnMouseDown()
    {
        isSelected = !isSelected;
        UpdateVisualState();

        if (TryGetComponent<HexCell>(out var cell) && editorController != null)
        {
            if (isSelected)
                editorController.AddToSelection(cell);
            else
                editorController.RemoveFromSelection(cell);
        }
    }

    /// <summary>
    /// Updates selection state from external scripts.
    /// </summary>
    public void SetSelected(bool value)
    {
        isSelected = value;
        UpdateVisualState();
    }

    /// <summary>
    /// Applies current hover/selection state to tile visuals with layered transparency.
    /// </summary>
    void UpdateVisualState()
    {
        if (editorController == null || hexCell == null)
            return;

        //Debug.Log($"{name} UpdateVisualState → " +
        //    $"terrainType: '{hexCell.terrainType}', " +
        //    $"isSelected: {isSelected}, isHovered: {isHovered}, " +
        //    $"baseColor: {baseColor}, " +
        //    $"GetTerrainColor: {editorController.GetTerrainColor(hexCell.terrainType)}, " +
        //    $"hexAlpha: {hexCell.alpha}, " +
        //    $"showTransparency: {editorController.GetShowTransparency()}");

        // Determine effective terrain color
        Color terrainColor = baseColor; // fallback for untyped tiles
        if (!string.IsNullOrEmpty(hexCell.terrainType))
        {
            terrainColor = editorController.GetTerrainColor(hexCell.terrainType);
        }

        // Determine effective alpha (with override)
        float alpha = 1.0f;
        if (editorController.GetShowTransparency())
        {
            if (!string.IsNullOrEmpty(hexCell.terrainType))
                alpha = hexCell.alpha;
            else
                alpha = baseColor.a;
        }
        Color baseWithAlpha = new Color(terrainColor.r, terrainColor.g, terrainColor.b, alpha);

        // Determine overlay color (glow)
        Color overlay = Color.black;
        if (isSelected)
        {
            overlay = new Color(selectedColor.r, selectedColor.g, selectedColor.b, selectedColor.a * 0.8f);
        }
        else if (isHovered)
        {
            overlay = new Color(hoverColor.r, hoverColor.g, hoverColor.b, hoverColor.a * 0.6f);
        }

        // Apply to renderer
        rend.GetPropertyBlock(props);
        props.SetColor("_BaseColor", baseWithAlpha);
        props.SetColor("_EmissionColor", overlay.linear * 1.2f);
        rend.SetPropertyBlock(props);
    }

    /// <summary>
    /// Called when hex cell properties change to refresh visuals.
    /// </summary>
    public void RefreshVisuals()
    {
        UpdateVisualState();
    }
}

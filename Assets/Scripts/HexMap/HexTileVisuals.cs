using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class HexTileVisuals : MonoBehaviour
{
    public Color baseColor = new Color(0.8f, 1f, 0.8f, 0.6f);      // normal
    public Color hoverColor = new Color(1f, 1f, 0.4f, 0.6f);       // light yellow
    public Color selectedColor = new Color(0.5f, 1f, 0.5f, 0.7f);  // light green

    public bool affectBaseColorOnHover = true;

    private HexInspectorController inspectorController;
    public void SetInspector(HexInspectorController controller)
    {
        this.inspectorController = controller;
    }

    private MaterialPropertyBlock props;
    private Renderer rend;
    private bool isHovered = false;
    private bool isSelected = false;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        props = new MaterialPropertyBlock();
        SetColor(baseColor);
        SetGlow(Color.black);              // Set base glow to black (no emission)
    }

    void OnMouseEnter()
    {
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

        if (TryGetComponent<HexCell>(out var cell) && inspectorController != null)
            if (isSelected)
                inspectorController.AddToSelection(cell);
            else
                inspectorController.RemoveFromSelection(cell);
    }

    void SetColor(Color color)
    {
        rend.GetPropertyBlock(props);
        props.SetColor("_BaseColor", color);
        rend.SetPropertyBlock(props);
    }

    void SetGlow(Color color)
    {
        rend.GetPropertyBlock(props);

        Color.RGBToHSV(color, out float h, out float s, out float v);
        float target = 1.2f;
        float gain = target / Mathf.Max(0.01f, v);

        gain = Mathf.Clamp(gain, 0.8f, 5.0f); // optional: prevent excessive bloom
        Color emission = color.linear * gain;

        props.SetColor("_EmissionColor", emission);
        rend.SetPropertyBlock(props);
    }

    public void SetSelected(bool value)
    {
        isSelected = value;
        UpdateVisualState();
    }

    void UpdateVisualState()
    {
        if (inspectorController == null)
            return;

        Color current = inspectorController.GetCurrentTerrainColor();

        if (isSelected)
        {
            SetColor(current);
            SetGlow(current * 1.1f);
        }
        else if (isHovered)
        {
            SetColor(current);
            SetGlow(current * 1.2f); // Slightly stronger glow
        }
        else
        {
            SetColor(baseColor);
            SetGlow(Color.black);
        }
    }

}

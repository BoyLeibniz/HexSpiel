using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class HexTileVisuals : MonoBehaviour
{
    public Color baseColor = new Color(0.8f, 1f, 0.8f, 0.6f);      // normal
    public Color hoverColor = new Color(1f, 1f, 0.4f, 0.6f);       // light yellow
    public Color selectedColor = new Color(0.5f, 1f, 0.5f, 0.7f);  // light green

    public bool affectBaseColorOnHover = true;

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
    }

    void SetColor(Color color)
    {
        rend.GetPropertyBlock(props);
        props.SetColor("_BaseColor", color);
        rend.SetPropertyBlock(props);
    }

    void SetGlow(Color glowColor)
    {
        rend.GetPropertyBlock(props);
        props.SetColor("_EmissionColor", glowColor);
        rend.SetPropertyBlock(props);
    }

    void UpdateVisualState()
    {
        if (isSelected)
        {
            SetColor(selectedColor);
            SetGlow(selectedColor * 1.2f);
        }
        else if (isHovered)
        {
            if (affectBaseColorOnHover && hoverColor.a > 0.01f)
                SetColor(hoverColor);

            SetGlow(hoverColor * 1.1f);
        }
        else
        {
            SetColor(baseColor);
            SetGlow(Color.black); // Turn off glow
        }
    }

}

using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class HexTileHighlighter : MonoBehaviour
{
    private Color originalColor;
    private Renderer rend;

    public Color highlightColor = Color.yellow;

    void Start()
    {
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;
    }

    void OnMouseEnter()
    {
        rend.material.color = highlightColor;
    }

    void OnMouseExit()
    {
        rend.material.color = originalColor;
    }
}

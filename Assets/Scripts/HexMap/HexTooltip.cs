// Assets/Scripts/HexMap/HexTooltip.cs
using UnityEngine;
using TMPro;

/// <summary>
/// Displays floating text above a hex tile, always facing the camera.
/// </summary>
public class HexTooltip : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMesh;
    [SerializeField] private Canvas canvas;

    private Camera mainCamera;
    private HexCell parentCell;

    void Awake()
    {
        if (textMesh == null)
        {
            var textGO = new GameObject("TooltipText");
            textGO.transform.SetParent(transform, false); // keep local transform clean
            textGO.transform.localPosition = Vector3.zero;

            textMesh = textGO.AddComponent<TextMeshPro>();
            textMesh.fontSize = 3f;
            textMesh.color = Color.white;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.enableWordWrapping = false;
            textMesh.text = "";
        }

        mainCamera = Camera.main ?? FindObjectOfType<Camera>();
    }
    void Update()
    {
        // Skip update if tooltip is not active
        if (!gameObject.activeInHierarchy) return;

        // Billboard effect - always face camera
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                           mainCamera.transform.rotation * Vector3.up);
        }
    }

    /// <summary>
    /// Initialize tooltip with parent cell reference and position offset.
    /// </summary>
    public void Initialize(HexCell cell)
    {
        parentCell = cell;
        transform.position = cell.worldPosition + Vector3.up * 0.5f;
        UpdateContent();
    }

    /// <summary>
    /// Update tooltip content based on current cell properties.
    /// Override this method to add more properties in the future.
    /// </summary>
    public virtual void UpdateContent()
    {
        if (parentCell == null) return;

        string content = GetTooltipText();
        textMesh.text = content;

        // Hide if no content
        gameObject.SetActive(!string.IsNullOrEmpty(content));
    }

    /// <summary>
    /// Generate tooltip text. Override for custom formatting.
    /// </summary>
    protected virtual string GetTooltipText()
    {
        if (parentCell == null) return "";

        // Currently only shows label, but extensible for future properties
        if (!string.IsNullOrEmpty(parentCell.label))
            return parentCell.label;

        return "";
    }

    /// <summary>
    /// Set visibility of this tooltip.
    /// </summary>
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible && !string.IsNullOrEmpty(GetTooltipText()));
    }
}
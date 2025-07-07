// Assets/UI/HexMap/UITooltipManager.cs
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

/// <summary>
/// Provides basic runtime tooltip support for UI Toolkit elements.
/// Renders a floating label near the mouse cursor when a tooltip is triggered.
/// </summary>
public class UITooltipManager : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    private VisualElement root;
    private Label tooltipLabel;
    private Coroutine tooltipRoutine;

    private const float DefaultDelay = 0.4f;
    private const float OffsetX = 12f;
    private const float OffsetY = 30f;

    void Awake()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogError("UITooltipManager: No UIDocument assigned or found!");
                return;
            }
        }

        root = uiDocument.rootVisualElement;

        tooltipLabel = new Label();
        tooltipLabel.name = "ui-tooltip-label";
        tooltipLabel.style.position = Position.Absolute;
        tooltipLabel.style.backgroundColor = new Color(0f, 0f, 0f, 0.85f);
        tooltipLabel.style.color = Color.white;
        tooltipLabel.style.paddingLeft = 6;
        tooltipLabel.style.paddingRight = 6;
        tooltipLabel.style.paddingTop = 3;
        tooltipLabel.style.paddingBottom = 3;
        tooltipLabel.style.unityFontStyleAndWeight = FontStyle.Normal;
        tooltipLabel.style.borderBottomLeftRadius = 4;
        tooltipLabel.style.borderBottomRightRadius = 4;
        tooltipLabel.style.borderTopLeftRadius = 4;
        tooltipLabel.style.borderTopRightRadius = 4;
        tooltipLabel.style.display = DisplayStyle.None;
        tooltipLabel.pickingMode = PickingMode.Ignore;

        root.Add(tooltipLabel);
    }

    /// <summary>
    /// Observes the tooltip property of a VisualElement and displays it at runtime.
    /// </summary>
    public void ObserveTooltip(VisualElement element, float delaySeconds = DefaultDelay)
    {
        element.RegisterCallback<MouseEnterEvent>(_ =>
        {
            ShowTooltip(element, element.tooltip, delaySeconds);
        });

        element.RegisterCallback<MouseLeaveEvent>(_ =>
        {
            if (tooltipRoutine != null)
                StopCoroutine(tooltipRoutine);

            tooltipLabel.style.display = DisplayStyle.None;
        });
    }

    public void ShowTooltip(VisualElement target, string tooltipText, float delaySeconds = DefaultDelay)
    {
        if (tooltipRoutine != null)
            StopCoroutine(tooltipRoutine);

        tooltipRoutine = StartCoroutine(TooltipRoutine(target, tooltipText, delaySeconds));
    }

    private IEnumerator TooltipRoutine(VisualElement target, string tooltipText, float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        if (string.IsNullOrWhiteSpace(tooltipText))
        {
            tooltipLabel.style.display = DisplayStyle.None;
            yield break;
        }

        tooltipLabel.text = tooltipText;
        tooltipLabel.style.display = DisplayStyle.Flex;
        tooltipLabel.BringToFront();

        yield return null; // let layout update

        var targetBounds = target.worldBound;
        Vector2 localPos = root.WorldToLocal(targetBounds.position);

        float tooltipWidth = tooltipLabel.resolvedStyle.width;
        float tooltipHeight = tooltipLabel.resolvedStyle.height;

        float proposedLeft = localPos.x + OffsetX;
        float proposedTop = localPos.y + OffsetY;

        float maxX = root.resolvedStyle.width - tooltipWidth - 5f;
        float maxY = root.resolvedStyle.height - tooltipHeight - 5f;

        tooltipLabel.style.left = Mathf.Clamp(proposedLeft, 5f, maxX);
        tooltipLabel.style.top = Mathf.Clamp(proposedTop, 5f, maxY);
    }
}

// Assets/UI/Components/ComboBoxField.cs
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;

public class ComboBoxField : VisualElement
{
    private readonly TextField inputField;
    private readonly Button dropdownButton;
    private readonly VisualElement uiRoot;

    private readonly VisualElement popupContainer;
    private readonly ListView listView;

    private List<string> items = new();
    
    public Action OnDropdownOpening;

    public string value => inputField.value;
    public void SetValueWithoutNotify(string val) => inputField.SetValueWithoutNotify(val);
    public void SetItems(List<string> newItems)
    {
        items = newItems ?? new();
        listView.itemsSource = items;
        listView.Rebuild();
    }

    public ComboBoxField(VisualElement root, Action<string> onValueChanged = null)
    {
        uiRoot = root;

        AddToClassList("combo-box-field");

        var row = new VisualElement();
        row.AddToClassList("combo-box-row");
        Add(row);

        inputField = new TextField();
        inputField.AddToClassList("combo-box-input");
        row.Add(inputField);

        // Patch the internal <TextInput> element to remove unwanted padding
        var textInput = inputField.Q("unity-text-input");
        if (textInput != null)
        {
            textInput.style.paddingTop = 0;
            textInput.style.paddingBottom = 0;
            textInput.style.paddingLeft = 5;
            textInput.style.paddingRight = 5;
            textInput.style.unityTextAlign = TextAnchor.MiddleLeft;
        }

        dropdownButton = new Button(TogglePopup) { text = "▾" };
        dropdownButton.AddToClassList("combo-box-button");
        row.Add(dropdownButton);

        // Popup list setup
        popupContainer = new VisualElement();
        popupContainer.AddToClassList("combo-box-popup");
        popupContainer.style.display = DisplayStyle.None;
        uiRoot.Add(popupContainer); // <- no longer inside combo-box-row

        listView = new ListView();
        listView.makeItem = () => new Label();
        listView.bindItem = (e, i) => ((Label)e).text = items[i];
        listView.selectionType = SelectionType.Single;
        listView.style.flexGrow = 1;
        listView.selectionChanged += selectedItems =>
        {
            foreach (string selected in selectedItems)
            {
                inputField.value = selected;
                onValueChanged?.Invoke(selected);
                break;
            }
            HidePopup();
        }; popupContainer.Add(listView);

        inputField.RegisterValueChangedCallback(evt =>
        {
            onValueChanged?.Invoke(evt.newValue);
        });
    }

    private void TogglePopup()
    {
        if (popupContainer.style.display == DisplayStyle.Flex)
        {
            popupContainer.style.display = DisplayStyle.None;
        }
        else
        {
            OnDropdownOpening?.Invoke();  // <-- Trigger map fetch

            var bounds = this.worldBound;
            popupContainer.style.position = Position.Absolute;
            popupContainer.style.left = bounds.x;
            popupContainer.style.top = bounds.y + bounds.height;
            popupContainer.style.width = bounds.width;
            popupContainer.style.display = DisplayStyle.Flex;
            popupContainer.BringToFront();
        }
    }
    private void HidePopup() => popupContainer.style.display = DisplayStyle.None;
}

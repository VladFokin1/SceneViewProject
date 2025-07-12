using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectUIElement : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI _nameText;
    [SerializeField] private Toggle _visibilityButton;
    [SerializeField] private Toggle _selectionToggle;

    private SceneObject _target;
    private bool _isVisible = true;

    public void Initialize(SceneObject target)
    {
        _target = target;
        _nameText.text = target.DisplayName;
        _selectionToggle.isOn = target.IsSelected;

        _visibilityButton.onValueChanged.AddListener(ChangeVisibility);
        _selectionToggle.onValueChanged.AddListener(ToggleSelection);
    }

    public void ChangeVisibility(bool value)
    {
        _target.ToggleVisibility(value);
    }

    private void ToggleSelection(bool isSelected)
    {
        ObjectsManager.Instance.ToggleObjectSelection(_target);
    }

    public void ToggleSelectionUI(bool value)
    {
        _selectionToggle.isOn = value;
    }

    public void ChangeVisibilityUI(bool value)
    {
        _visibilityButton.isOn = value;
    }


}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject _objectUIPrefab;
    [SerializeField] private Transform _contentPanel;
    [SerializeField] private Slider _transparencySlider;
    [SerializeField] private Button _colorButton;
    [SerializeField] private Toggle _visibilityToggle;
    [SerializeField] private FlexibleColorPicker _colorPicker;
    [SerializeField] private Toggle _selectAllToggle;

    private void Start()
    {
        GenerateObjectList();
        SetupEventListeners();
    }

    private void GenerateObjectList()
    {
        foreach (var obj in ObjectsManager.Instance.GetAllObjects())
        {
            var uiElement = Instantiate(_objectUIPrefab, _contentPanel);
            uiElement.GetComponent<ObjectUIElement>().Initialize(obj);
        }
    }

    private void SetupEventListeners()
    {
        _transparencySlider.onValueChanged.AddListener(SetTransparencyToAllSelected);
        _colorButton.onClick.AddListener(OpenColorPickerForAllSelected);
        _visibilityToggle.onValueChanged.AddListener(ToggleVisibilityForAllSelected);
        _selectAllToggle.onValueChanged.AddListener(ToggleSelectionForAll);
    }

    private void SetTransparencyToAllSelected(float value)
    {
        ObjectsManager.Instance.ApplyToAllSelected(obj => obj.SetTransparency(value));
    }

    private void ToggleVisibilityForAllSelected(bool value)
    {
        ObjectsManager.Instance.ApplyToAllSelected(obj => obj.ToggleVisibility(value));
    }

    private void OpenColorPickerForAllSelected()
    {
        _colorPicker.gameObject.SetActive(true);
        _colorPicker.onColorChange.AddListener(SetColorToAllSelected);
    }

    private void ToggleSelectionForAll(bool value)
    {
        var objectsUI = FindObjectsOfType<ObjectUIElement>();
        foreach (var obj in objectsUI)
        {
            obj.ToggleSelectionUI(value);
        }
    }

    private void SetColorToAllSelected(Color color)
    {
        ObjectsManager.Instance.ApplyToAllSelected(obj => obj.SetColor(color));
        //_colorPicker.OnColorSelected -= SetColorToAllSelected;
    }
}
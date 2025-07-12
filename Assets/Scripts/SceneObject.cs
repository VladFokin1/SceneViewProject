using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneObject : MonoBehaviour
{
    [SerializeField] private string _displayName;
    private Renderer _renderer;
    private Color _originalColor;
    private bool _isSelected;

    public string DisplayName => _displayName;
    public bool IsSelected => _isSelected;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _originalColor = _renderer.material.color;
        _displayName = gameObject.name;
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        UpdateSelectionVisual();
    }

    private void UpdateSelectionVisual()
    {
        if (TryGetComponent<Outline>(out var outline))
        {
            outline.enabled = _isSelected;
        }
    }

    public void SetColor(Color color)
    {
        _renderer.material.color = new Color(color.r, color.g, color.b, _renderer.material.color.a);
    }

    public void SetTransparency(float alpha)
    {
        var color = _renderer.material.color;
        _renderer.material.color = new Color(color.r, color.g, color.b, alpha);
    }

    public void ToggleVisibility(bool isVisible)
    {
        gameObject.SetActive(isVisible);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsManager : MonoBehaviour
{
    public static ObjectsManager Instance { get; private set; }



    [SerializeField] private List<SceneObject> _sceneObjects ;
    private List<SceneObject> _selectedObjects = new();

    private void Awake()
    {
        Instance = this;
        _sceneObjects = new List<SceneObject>(FindObjectsOfType<SceneObject>());
    }


    public List<SceneObject> GetAllObjects() => _sceneObjects;
    public List<SceneObject> SelectedObjects() => _selectedObjects;



    public void ToggleObjectSelection(SceneObject obj)
    {
        if (obj.IsSelected)
        {
            _selectedObjects.Remove(obj);
            obj.SetSelected(false);
        }
        else
        {
            _selectedObjects.Add(obj);
            obj.SetSelected(true);
        }

        UpdateCameraFocus();
    }

    public void ApplyToAllSelected(System.Action<SceneObject> action)
    {
        foreach (var obj in _selectedObjects)
        {
            action?.Invoke(obj);
        }
    }

    public Vector3 GetSelectionCenter()
    {
        if (_selectedObjects.Count == 0) return Vector3.zero;

        Vector3 center = Vector3.zero;
        foreach (var obj in _selectedObjects)
        {
            center += obj.transform.position;
        }
        return center / _selectedObjects.Count;
    }

    private void UpdateCameraFocus()
    {
        if (_selectedObjects.Count > 0)
        {
            CameraController.Instance.FocusOnSelection(GetSelectionCenter(),
                CalculateBoundingSphereRadius());
        }
        else
        {
            CameraController.Instance.ResetCamera();
        }
    }

    public float CalculateBoundingSphereRadius()
    {
        if (_selectedObjects.Count == 0) return 0f;
        if (_selectedObjects.Count == 1) return 1.5f;

        Vector3 center = GetSelectionCenter();
        float maxDistance = 0f;

        foreach (var obj in _selectedObjects)
        {
            float distance = Vector3.Distance(center, obj.transform.position);
            if (distance > maxDistance) maxDistance = distance;
        }

        return maxDistance;
    }

}

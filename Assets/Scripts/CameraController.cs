using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    public enum CameraMode { Free, Focused, Transitioning }
    public CameraMode CurrentMode { get; private set; } = CameraMode.Free;

    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _rotationSpeed = 100f;
    [SerializeField] private float _zoomSpeed = 5f;
    [SerializeField] private float _minZoom = 1f;
    [SerializeField] private float _maxZoom = 20f;
    [SerializeField] private float _focusPadding = 1.5f;

    [Header("Smooth Transition")]
    [SerializeField] private float _transitionDuration = 1f;
    [SerializeField] private AnimationCurve _transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector3 _focusPoint;
    private float _focusRadius;
    private Vector3 _offset;
    private EventSystem _eventSystem;
    private Coroutine _transitionCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _eventSystem = EventSystem.current;
    }

    private void Update()
    {
        if (CurrentMode == CameraMode.Transitioning) return;
        if (IsPointerOverUI()) return;

        switch (CurrentMode)
        {
            case CameraMode.Free:
                HandleFreeMovement();
                break;
            case CameraMode.Focused:
                HandleFocusedMovement();
                break;
        }
        HandleZoom();
    }

    private bool IsPointerOverUI()
    {
        if (_eventSystem == null) return false;
        return _eventSystem.IsPointerOverGameObject();
    }

    private void HandleFreeMovement()
    {
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * _rotationSpeed * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * _rotationSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up, mouseX, Space.World);
            transform.Rotate(Vector3.left, mouseY, Space.Self);
        }

        if (Input.GetMouseButton(2))
        {
            float mouseX = Input.GetAxis("Mouse X") * _moveSpeed * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * _moveSpeed * Time.deltaTime;
            transform.Translate(-mouseX, -mouseY, 0, Space.Self);
        }
    }

    private void HandleFocusedMovement()
    {
        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X") * _rotationSpeed * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * _rotationSpeed * Time.deltaTime;

            transform.RotateAround(_focusPoint, Vector3.up, mouseX);
            transform.RotateAround(_focusPoint, transform.right, -mouseY);

            _offset = transform.position - _focusPoint;
        }

        transform.position = _focusPoint + _offset;
        transform.LookAt(_focusPoint);
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            float zoomAmount = scroll * _zoomSpeed;

            if (CurrentMode == CameraMode.Focused)
            {
                float currentDistance = _offset.magnitude;
                float minDistance = _focusRadius * _focusPadding;
                float newDistance = Mathf.Clamp(currentDistance - zoomAmount, minDistance, _maxZoom);

                _offset = _offset.normalized * newDistance;
                transform.position = _focusPoint + _offset;
            }
            else
            {
                transform.Translate(Vector3.forward * zoomAmount, Space.Self);
            }
        }
    }

    public void FocusOnSelection(Vector3 center, float radius)
    {
        // Рассчитываем целевую позицию камеры
        Vector3 targetOffset = CalculateTargetOffset(center, radius);
        Vector3 targetPosition = center + targetOffset;

        // Запускаем плавный переход
        if (_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
        }
        _transitionCoroutine = StartCoroutine(SmoothTransitionToTarget(targetPosition, center));
    }

    private Vector3 CalculateTargetOffset(Vector3 center, float radius)
    {
        // Рассчитываем оптимальное расстояние
        float requiredDistance = (radius * _focusPadding) / Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
        requiredDistance = Mathf.Clamp(requiredDistance, _minZoom, _maxZoom);

        // Используем текущее направление камеры или направление к цели
        Vector3 direction = (_focusPoint != Vector3.zero) ?
            (transform.position - _focusPoint).normalized :
            (transform.position - center).normalized;

        return direction * requiredDistance;
    }

    private IEnumerator SmoothTransitionToTarget(Vector3 targetPosition, Vector3 focusPoint)
    {
        CurrentMode = CameraMode.Transitioning;

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        Vector3 endPosition = targetPosition;

        // Рассчитываем целевой поворот (камера смотрит на точку фокуса)
        Quaternion endRotation = Quaternion.LookRotation(focusPoint - targetPosition);

        float elapsed = 0f;

        while (elapsed < _transitionDuration)
        {
            float t = _transitionCurve.Evaluate(elapsed / _transitionDuration);

            // Плавная интерполяция позиции и поворота
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Финализируем позицию и поворот
        transform.position = endPosition;
        transform.rotation = endRotation;

        // Обновляем состояние камеры
        _focusPoint = focusPoint;
        _offset = transform.position - _focusPoint;
        CurrentMode = CameraMode.Focused;

        _transitionCoroutine = null;
    }

    public void ResetCamera()
    {
        CurrentMode = CameraMode.Free;
        _focusPoint = Vector3.zero;
        _focusRadius = 0f;
    }

   
}
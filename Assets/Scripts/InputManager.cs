using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class InputManager : MonoBehaviour
{

    public static InputManager Instance { get; private set; }

    #region Drag Direction
    public event Action<Vector2> OnDragEnd;

    private Vector2 startPosition;
    private Vector2 _currentPosition;
    private bool _isDragging = false;

    private InputAction _contactAction;
    private InputAction _positionAction;

    #endregion

    #region Tap

    public event Action<bool> OnTapEvent;

    private bool _isTouching = false;

    #endregion

    #region Continuos

    public event Action<Vector2> OnContinuousEvent;

    #endregion

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();
        #region Drag Direction
        var dragMap = playerInput.actions.FindActionMap("Drag");

        _contactAction = dragMap.FindAction("PrimaryContact");
        _positionAction = dragMap.FindAction("PrimaryPosition");

        _contactAction.started += OnTouchStart;
        _contactAction.canceled += OnTouchEnd;

        _positionAction.performed += OnPositionChanged;
        #endregion


    }

    #region Drag Direction
    private void OnTouchStart(InputAction.CallbackContext obj)
    {
        _isDragging = true;
        startPosition = _positionAction.ReadValue<Vector2>();
        _currentPosition = startPosition;
    }
    private void OnPositionChanged(InputAction.CallbackContext obj)
    {
        if (_isDragging)
            _currentPosition = obj.ReadValue<Vector2>();
    }
    private void OnTouchEnd(InputAction.CallbackContext context)
    {
        if (!_isDragging) return;

        _isDragging = false;

        Vector2 dragDirection = _currentPosition - startPosition;

        if (dragDirection.sqrMagnitude > 0)
            dragDirection.Normalize();

        OnDragEnd?.Invoke(dragDirection);
    }

    #endregion


    #region Tap

    public void OnTap(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (GameManager.Instance == null || !GameManager.Instance.PartidaActiva || PointerOverControl()) return;
            _isTouching = true;
            OnTapEvent?.Invoke(true);
        }
        else if (context.canceled)
        {
            OnTapEvent?.Invoke(false);
            _isTouching = false;
        }
    }

    readonly List<RaycastResult> uiHits = new List<RaycastResult>();
    bool PointerOverControl()
    {
        if (EventSystem.current == null || Pointer.current == null) return false;
        var data = new PointerEventData(EventSystem.current) { position = Pointer.current.position.ReadValue() };
        uiHits.Clear();
        EventSystem.current.RaycastAll(data, uiHits);
        foreach (var hit in uiHits)
            if (hit.gameObject.GetComponentInParent<UnityEngine.UI.Selectable>() != null) return true;
        return false;
    }

    public void ReleaseTap()
    {
        _isTouching = false;
        _isDragging = false;
        OnTapEvent?.Invoke(false);
    }
    void OnApplicationFocus(bool focused) { if (!focused) ReleaseTap(); }

    #endregion


    #region Continuos

    public void OnContinuous(InputAction.CallbackContext context)
    {
        Vector2 value = context.ReadValue<Vector2>();
        Debug.Log($"Continuous input value: {value}");
        OnContinuousEvent?.Invoke(value);
    }

    #endregion



}

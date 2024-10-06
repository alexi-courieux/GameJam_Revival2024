using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public EventHandler OnInteract;
    public EventHandler OnInteractAlt;
    public EventHandler OnPause;
    public EventHandler OnNext;
    public EventHandler OnPrevious;

    private InputActions _inputActions;

    private void Awake()
    {
        Instance = this;
        _inputActions = new InputActions();
        _inputActions.Enable();

        _inputActions.Player.Interact.performed += Interact_OnPerformed;
        _inputActions.Player.InteractAlt.performed += InteractAlt_OnPerformed;
        _inputActions.Player.Pause.performed += Pause_OnPerformed;
        _inputActions.Player.PreviousNext.performed += PreviousNext_OnPerformed;
    }

    private void OnDestroy()
    {
        _inputActions.Player.Interact.performed -= Interact_OnPerformed;
        _inputActions.Player.InteractAlt.performed -= InteractAlt_OnPerformed;
        _inputActions.Player.Pause.performed -= Pause_OnPerformed;

        _inputActions.Dispose();
    }

    public Vector2 GetMovementVectorNormalized()
    {
        return _inputActions.Player.Move.ReadValue<Vector2>().normalized;
    }

    public Vector2 GetRotationVectorNormalized()
    {
        return _inputActions.Player.Rotate.ReadValue<Vector2>().normalized;
    }

    private void Interact_OnPerformed(InputAction.CallbackContext obj)
    {
        OnInteract?.Invoke(this, EventArgs.Empty);
    }

    private void InteractAlt_OnPerformed(InputAction.CallbackContext obj)
    {
        OnInteractAlt?.Invoke(this, EventArgs.Empty);
    }

    private void Pause_OnPerformed(InputAction.CallbackContext obj)
    {
        OnPause?.Invoke(this, EventArgs.Empty);
    }

    private void PreviousNext_OnPerformed(InputAction.CallbackContext obj)
    {
        if (obj.ReadValue<float>() > 0)
        {
            OnNext?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            OnPrevious?.Invoke(this, EventArgs.Empty);
        }
    }
    
    public void DisableGameplayInput()
    {
        _inputActions.Player.Disable();
    }
    
    public void EnableGameplayInput()
    {
        _inputActions.Player.Enable();
    }
}
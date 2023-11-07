using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.InputSystem;

[SuppressMessage("ReSharper", "CheckNamespace")]
public class GameInput : MonoBehaviour
{
    private const string PLAYER_PREFS_BINDINGS = "InputBindings";

    public static GameInput Instance { get; private set; }

    public enum Binding
    {
        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight,
        Attack,
        Interact,
        Sprint,
        Pause,
        ChangeMovementMode,
        ChangeCurrentWeapon,
        OpenCharacterInfo,
        UpgradesStartDragging,
        DropWeapon,
        MouseScroll
    }

    public event EventHandler OnAttackAction;
    public event EventHandler OnInteractAction;
    public event EventHandler OnSprintAction;
    public event EventHandler OnPauseAction;
    public event EventHandler OnChangeMovementModeAction;
    public event EventHandler OnChangeCurrentWeaponAction;
    public event EventHandler OnOpenCharacterInfoAction;
    public event EventHandler OnChangeCameraModeAction;
    public event EventHandler OnDropWeaponAction;
    public event EventHandler OnUpgradesStartDragging;

    private GameInputActions gameInputActions;

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;


        gameInputActions = new GameInputActions();

        if (PlayerPrefs.HasKey(PLAYER_PREFS_BINDINGS))
            gameInputActions.LoadBindingOverridesFromJson(PlayerPrefs.GetString(PLAYER_PREFS_BINDINGS));

        gameInputActions.Player.Enable();

        gameInputActions.Player.CameraModeChange.performed += CameraModeChange_OnPerformed;
        gameInputActions.Player.Sprint.performed += Sprint_OnPerformed;
        gameInputActions.Player.ChangeMovementMode.performed += ChangeMovementMode_OnPerformed;
        gameInputActions.Player.Attack.performed += Attack_OnPerformed;
        gameInputActions.Player.Interact.performed += Interact_OnPerformed;
        gameInputActions.Player.ChangeCurrentWeapon.performed += ChangeCurrentWeapon_OnPerformed;
        gameInputActions.Player.DropCurrentWeapon.performed += DropCurrentWeapon_OnPerformed;
        gameInputActions.Player.OpenCharacterInfo.performed += OpenCharacterInfo_OnPerformed;
        gameInputActions.Player.UpgradesStartDraging.performed += UpgradesStartDragging_OnPerformed;
        gameInputActions.Player.GamePause.performed += GamePause_OnPerformed;
    }

    private void GamePause_OnPerformed(InputAction.CallbackContext obj)
    {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }

    private void UpgradesStartDragging_OnPerformed(InputAction.CallbackContext obj)
    {
        OnUpgradesStartDragging?.Invoke(this, EventArgs.Empty);
    }

    private void OpenCharacterInfo_OnPerformed(InputAction.CallbackContext obj)
    {
        OnOpenCharacterInfoAction?.Invoke(this, EventArgs.Empty);
    }

    private void DropCurrentWeapon_OnPerformed(InputAction.CallbackContext obj)
    {
        OnDropWeaponAction?.Invoke(this, EventArgs.Empty);
    }

    private void ChangeCurrentWeapon_OnPerformed(InputAction.CallbackContext obj)
    {
        OnChangeCurrentWeaponAction?.Invoke(this, EventArgs.Empty);
    }

    private void Interact_OnPerformed(InputAction.CallbackContext obj)
    {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }

    private void Attack_OnPerformed(InputAction.CallbackContext obj)
    {
        OnAttackAction?.Invoke(this, EventArgs.Empty);
    }

    private void ChangeMovementMode_OnPerformed(InputAction.CallbackContext obj)
    {
        OnChangeMovementModeAction?.Invoke(this, EventArgs.Empty);
    }

    private void Sprint_OnPerformed(InputAction.CallbackContext obj)
    {
        OnSprintAction?.Invoke(this, EventArgs.Empty);
    }

    private void CameraModeChange_OnPerformed(InputAction.CallbackContext obj)
    {
        OnChangeCameraModeAction?.Invoke(this, EventArgs.Empty);
    }

    private void OnDestroy()
    {
        gameInputActions.Player.CameraModeChange.performed -= CameraModeChange_OnPerformed;
        gameInputActions.Player.Sprint.performed -= Sprint_OnPerformed;
        gameInputActions.Player.ChangeMovementMode.performed -= ChangeMovementMode_OnPerformed;
        gameInputActions.Player.Attack.performed -= Attack_OnPerformed;
        gameInputActions.Player.Interact.performed -= Interact_OnPerformed;
        gameInputActions.Player.ChangeCurrentWeapon.performed -= ChangeCurrentWeapon_OnPerformed;
        gameInputActions.Player.DropCurrentWeapon.performed -= DropCurrentWeapon_OnPerformed;

        gameInputActions.Dispose();
    }

    public Vector2 GetMovementVectorNormalized()
    {
        var inputVector = gameInputActions.Player.Movement.ReadValue<Vector2>();

        inputVector = inputVector.normalized;

        return inputVector;
    }

    public Vector2 GetMousePosition()
    {
        var mousePosition = gameInputActions.Player.MousePosition.ReadValue<Vector2>();

        return mousePosition;
    }

    public Vector2 GetCurrentMousePosition()
    {
        var currentMousePosition = gameInputActions.Player.CurrentMousePosition.ReadValue<Vector2>();

        return currentMousePosition;
    }

    public float GetMouseScroll()
    {
        var mouseScroll = -gameInputActions.Player.MouseScroll.ReadValue<Vector2>().y;

        return mouseScroll;
    }

    public string GetBindingText(Binding binding)
    {
        switch (binding)
        {
            default:
            case Binding.MoveUp:
                return gameInputActions.Player.Movement.bindings[1].ToDisplayString();
            case Binding.MoveDown:
                return gameInputActions.Player.Movement.bindings[2].ToDisplayString();
            case Binding.MoveLeft:
                return gameInputActions.Player.Movement.bindings[3].ToDisplayString();
            case Binding.MoveRight:
                return gameInputActions.Player.Movement.bindings[4].ToDisplayString();
            case Binding.MouseScroll:
                return gameInputActions.Player.MouseScroll.bindings[0].ToDisplayString();
            case Binding.Sprint:
                return gameInputActions.Player.Sprint.bindings[0].ToDisplayString();
            case Binding.ChangeMovementMode:
                return gameInputActions.Player.ChangeMovementMode.bindings[0].ToDisplayString();
            case Binding.Attack:
                return gameInputActions.Player.Attack.bindings[0].ToDisplayString();
            case Binding.Interact:
                return gameInputActions.Player.Interact.bindings[0].ToDisplayString();
            case Binding.ChangeCurrentWeapon:
                return gameInputActions.Player.ChangeCurrentWeapon.bindings[0].ToDisplayString();
            case Binding.DropWeapon:
                return gameInputActions.Player.DropCurrentWeapon.bindings[0].ToDisplayString();
            case Binding.OpenCharacterInfo:
                return gameInputActions.Player.OpenCharacterInfo.bindings[0].ToDisplayString();
            case Binding.UpgradesStartDragging:
                return gameInputActions.Player.UpgradesStartDraging.bindings[0].ToDisplayString();
            case Binding.Pause:
                return gameInputActions.Player.GamePause.bindings[0].ToDisplayString();
        }
    }

    public float GetBindingValue(Binding binding)
    {
        var inputValue = 0f;
        switch (binding)
        {
            default:
            case Binding.MoveUp:
                inputValue = gameInputActions.Player.Movement.ReadValue<Vector2>().x > 0
                    ? gameInputActions.Player.Movement.ReadValue<Vector2>().x
                    : 0;
                break;
            case Binding.MoveDown:
                inputValue = gameInputActions.Player.Movement.ReadValue<Vector2>().x < 0
                    ? gameInputActions.Player.Movement.ReadValue<Vector2>().x
                    : 0;
                break;
            case Binding.MoveLeft:
                inputValue = gameInputActions.Player.Movement.ReadValue<Vector2>().y > 0
                    ? gameInputActions.Player.Movement.ReadValue<Vector2>().y
                    : 0;
                break;
            case Binding.MoveRight:
                inputValue = gameInputActions.Player.Movement.ReadValue<Vector2>().y < 0
                    ? gameInputActions.Player.Movement.ReadValue<Vector2>().y
                    : 0;
                break;
            case Binding.MouseScroll:
                inputValue = gameInputActions.Player.MouseScroll.ReadValue<Vector2>().y;
                break;
            case Binding.Sprint:
                inputValue = gameInputActions.Player.Sprint.IsPressed() ? 1f : 0f;
                break;
            case Binding.ChangeMovementMode:
                inputValue = gameInputActions.Player.Sprint.IsPressed() ? 1f : 0f;
                break;
            case Binding.Attack:
                inputValue = gameInputActions.Player.Attack.IsPressed() ? 1f : 0f;
                break;
            case Binding.Interact:
                inputValue = gameInputActions.Player.Interact.IsPressed() ? 1f : 0f;
                break;
            case Binding.ChangeCurrentWeapon:
                inputValue = gameInputActions.Player.ChangeCurrentWeapon.IsPressed() ? 1f : 0f;
                break;
            case Binding.DropWeapon:
                inputValue = gameInputActions.Player.DropCurrentWeapon.IsPressed() ? 1f : 0f;
                break;
            case Binding.OpenCharacterInfo:
                inputValue = gameInputActions.Player.OpenCharacterInfo.IsPressed() ? 1f : 0f;
                break;
            case Binding.UpgradesStartDragging:
                inputValue = gameInputActions.Player.UpgradesStartDraging.IsPressed() ? 1f : 0f;
                break;
            case Binding.Pause:
                inputValue = gameInputActions.Player.GamePause.IsPressed() ? 1f : 0f;
                break;
        }

        return inputValue;
    }

    public void RebindBinding(Binding binding, Action onActionRebound)
    {
        gameInputActions.Player.Disable();

        InputAction inputAction;
        int bindingIndex;

        switch (binding)
        {
            default:
            case Binding.MoveUp:
                inputAction = gameInputActions.Player.Movement;
                bindingIndex = 1;
                break;
            case Binding.MoveDown:
                inputAction = gameInputActions.Player.Movement;
                bindingIndex = 2;
                break;
            case Binding.MoveLeft:
                inputAction = gameInputActions.Player.Movement;
                bindingIndex = 3;
                break;
            case Binding.MoveRight:
                inputAction = gameInputActions.Player.Movement;
                bindingIndex = 4;
                break;
            case Binding.MouseScroll:
                inputAction = gameInputActions.Player.MouseScroll;
                bindingIndex = 0;
                break;
            case Binding.Sprint:
                inputAction = gameInputActions.Player.Sprint;
                bindingIndex = 0;
                break;
            case Binding.ChangeMovementMode:
                inputAction = gameInputActions.Player.ChangeMovementMode;
                bindingIndex = 0;
                break;
            case Binding.Attack:
                inputAction = gameInputActions.Player.Attack;
                bindingIndex = 0;
                break;
            case Binding.Interact:
                inputAction = gameInputActions.Player.Interact;
                bindingIndex = 0;
                break;
            case Binding.ChangeCurrentWeapon:
                inputAction = gameInputActions.Player.ChangeCurrentWeapon;
                bindingIndex = 0;
                break;
            case Binding.DropWeapon:
                inputAction = gameInputActions.Player.DropCurrentWeapon;
                bindingIndex = 0;
                break;
        }

        inputAction.PerformInteractiveRebinding(bindingIndex)
            .OnComplete(callback =>
            {
                callback.Dispose();
                gameInputActions.Player.Enable();
                onActionRebound();

                PlayerPrefs.SetString(PLAYER_PREFS_BINDINGS, gameInputActions.SaveBindingOverridesAsJson());
                PlayerPrefs.Save();
            })
            .Start();
    }
}

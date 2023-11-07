//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.7.0
//     from Assets/Scripts/Input/GameInputActions.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @GameInputActions: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @GameInputActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""GameInputActions"",
    ""maps"": [
        {
            ""name"": ""Player"",
            ""id"": ""a122400b-bc96-4c7f-a695-fd024ba5c548"",
            ""actions"": [
                {
                    ""name"": ""Movement"",
                    ""type"": ""Value"",
                    ""id"": ""9f532cab-b404-41fb-b130-4ad8326fb6dc"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""MousePosition"",
                    ""type"": ""Value"",
                    ""id"": ""eb8bb52d-286b-44bf-86b8-b4fa2aeb4355"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""CameraModeChange"",
                    ""type"": ""Button"",
                    ""id"": ""c764700d-b561-41d4-b80f-8cb457c24975"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""MouseScroll"",
                    ""type"": ""Value"",
                    ""id"": ""7e02e9ca-43fe-4df7-9cd9-9370eb7853a1"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Sprint"",
                    ""type"": ""Button"",
                    ""id"": ""0ba3f926-cca5-4eac-b157-0f8dced43198"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ChangeMovementMode"",
                    ""type"": ""Button"",
                    ""id"": ""80ea2ca4-aa27-4c49-90a9-6ff90317619d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Attack"",
                    ""type"": ""Button"",
                    ""id"": ""a8b9f28d-992d-40a4-a5e6-a6f6943e56a8"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Interact"",
                    ""type"": ""Button"",
                    ""id"": ""d15f757a-d5cd-437f-8ddc-ed8ebb27dba6"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ChangeCurrentWeapon"",
                    ""type"": ""Button"",
                    ""id"": ""4a4e596b-3a05-46d0-a758-ef690b80c5c5"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""DropCurrentWeapon"",
                    ""type"": ""Button"",
                    ""id"": ""b258c008-ed1e-45aa-afc0-254b59aa7df1"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""OpenCharacterInfo"",
                    ""type"": ""Button"",
                    ""id"": ""04337846-4680-47f5-9bc2-b2903dd188af"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""UpgradesStartDraging"",
                    ""type"": ""Button"",
                    ""id"": ""badc3559-18d4-4a88-ba26-877aa5fcc500"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""CurrentMousePosition"",
                    ""type"": ""Value"",
                    ""id"": ""3732c4e1-cbb1-4995-99a7-01b2abc77a31"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""GamePause"",
                    ""type"": ""Button"",
                    ""id"": ""a250d364-8583-4189-ad37-0cbed85d2871"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""84e8b2c0-e34c-4ebc-96d0-81ba525c6cfa"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""51431d53-05e7-4d00-8160-c5e95bbd3b00"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""d741f0ce-a055-4ba5-b66d-47127c525f25"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""9c14f4ae-a2f1-4f92-872e-e6d244830724"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""4ab40528-e0ca-4b6b-a41f-599eb9db5b47"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""df00c343-e606-4786-94d1-ea1e6d340af4"",
                    ""path"": ""<Mouse>/delta"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MousePosition"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c9c3aff0-7d9a-4701-be3b-9cabf881ae9d"",
                    ""path"": ""<Keyboard>/v"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CameraModeChange"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""987c61c0-2ef6-4deb-bb83-d0a76eda708b"",
                    ""path"": ""<Mouse>/scroll"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MouseScroll"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""9e1a06fe-d4e3-4e83-9268-64ad2febf562"",
                    ""path"": ""<Keyboard>/shift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Sprint"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c6f7d470-bb26-4414-a924-d4e94da460e9"",
                    ""path"": ""<Keyboard>/ctrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ChangeMovementMode"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0ab903a2-14a5-4b6d-a844-73f9da5f0284"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Attack"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""bedf6a14-a274-4ed7-b47d-0802b58bc198"",
                    ""path"": ""<Keyboard>/f"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Interact"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7e4148fc-b6e5-49a8-97b4-ff3b6f6a9969"",
                    ""path"": ""<Keyboard>/r"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ChangeCurrentWeapon"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""62688597-fb3f-4911-9406-6a663ed65326"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""DropCurrentWeapon"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""cdeb258a-ec0a-43af-b953-881793bcfda3"",
                    ""path"": ""<Keyboard>/c"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""OpenCharacterInfo"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c52739be-e568-44dd-b038-fc03ba3cef32"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""UpgradesStartDraging"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""78341644-eac6-4d34-8762-e4feed70bf4d"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CurrentMousePosition"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""24bf4b3f-4f7f-49c7-9270-fa686239fa4d"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""GamePause"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Player
        m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
        m_Player_Movement = m_Player.FindAction("Movement", throwIfNotFound: true);
        m_Player_MousePosition = m_Player.FindAction("MousePosition", throwIfNotFound: true);
        m_Player_CameraModeChange = m_Player.FindAction("CameraModeChange", throwIfNotFound: true);
        m_Player_MouseScroll = m_Player.FindAction("MouseScroll", throwIfNotFound: true);
        m_Player_Sprint = m_Player.FindAction("Sprint", throwIfNotFound: true);
        m_Player_ChangeMovementMode = m_Player.FindAction("ChangeMovementMode", throwIfNotFound: true);
        m_Player_Attack = m_Player.FindAction("Attack", throwIfNotFound: true);
        m_Player_Interact = m_Player.FindAction("Interact", throwIfNotFound: true);
        m_Player_ChangeCurrentWeapon = m_Player.FindAction("ChangeCurrentWeapon", throwIfNotFound: true);
        m_Player_DropCurrentWeapon = m_Player.FindAction("DropCurrentWeapon", throwIfNotFound: true);
        m_Player_OpenCharacterInfo = m_Player.FindAction("OpenCharacterInfo", throwIfNotFound: true);
        m_Player_UpgradesStartDraging = m_Player.FindAction("UpgradesStartDraging", throwIfNotFound: true);
        m_Player_CurrentMousePosition = m_Player.FindAction("CurrentMousePosition", throwIfNotFound: true);
        m_Player_GamePause = m_Player.FindAction("GamePause", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }

    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // Player
    private readonly InputActionMap m_Player;
    private List<IPlayerActions> m_PlayerActionsCallbackInterfaces = new List<IPlayerActions>();
    private readonly InputAction m_Player_Movement;
    private readonly InputAction m_Player_MousePosition;
    private readonly InputAction m_Player_CameraModeChange;
    private readonly InputAction m_Player_MouseScroll;
    private readonly InputAction m_Player_Sprint;
    private readonly InputAction m_Player_ChangeMovementMode;
    private readonly InputAction m_Player_Attack;
    private readonly InputAction m_Player_Interact;
    private readonly InputAction m_Player_ChangeCurrentWeapon;
    private readonly InputAction m_Player_DropCurrentWeapon;
    private readonly InputAction m_Player_OpenCharacterInfo;
    private readonly InputAction m_Player_UpgradesStartDraging;
    private readonly InputAction m_Player_CurrentMousePosition;
    private readonly InputAction m_Player_GamePause;
    public struct PlayerActions
    {
        private @GameInputActions m_Wrapper;
        public PlayerActions(@GameInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Movement => m_Wrapper.m_Player_Movement;
        public InputAction @MousePosition => m_Wrapper.m_Player_MousePosition;
        public InputAction @CameraModeChange => m_Wrapper.m_Player_CameraModeChange;
        public InputAction @MouseScroll => m_Wrapper.m_Player_MouseScroll;
        public InputAction @Sprint => m_Wrapper.m_Player_Sprint;
        public InputAction @ChangeMovementMode => m_Wrapper.m_Player_ChangeMovementMode;
        public InputAction @Attack => m_Wrapper.m_Player_Attack;
        public InputAction @Interact => m_Wrapper.m_Player_Interact;
        public InputAction @ChangeCurrentWeapon => m_Wrapper.m_Player_ChangeCurrentWeapon;
        public InputAction @DropCurrentWeapon => m_Wrapper.m_Player_DropCurrentWeapon;
        public InputAction @OpenCharacterInfo => m_Wrapper.m_Player_OpenCharacterInfo;
        public InputAction @UpgradesStartDraging => m_Wrapper.m_Player_UpgradesStartDraging;
        public InputAction @CurrentMousePosition => m_Wrapper.m_Player_CurrentMousePosition;
        public InputAction @GamePause => m_Wrapper.m_Player_GamePause;
        public InputActionMap Get() { return m_Wrapper.m_Player; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerActions set) { return set.Get(); }
        public void AddCallbacks(IPlayerActions instance)
        {
            if (instance == null || m_Wrapper.m_PlayerActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_PlayerActionsCallbackInterfaces.Add(instance);
            @Movement.started += instance.OnMovement;
            @Movement.performed += instance.OnMovement;
            @Movement.canceled += instance.OnMovement;
            @MousePosition.started += instance.OnMousePosition;
            @MousePosition.performed += instance.OnMousePosition;
            @MousePosition.canceled += instance.OnMousePosition;
            @CameraModeChange.started += instance.OnCameraModeChange;
            @CameraModeChange.performed += instance.OnCameraModeChange;
            @CameraModeChange.canceled += instance.OnCameraModeChange;
            @MouseScroll.started += instance.OnMouseScroll;
            @MouseScroll.performed += instance.OnMouseScroll;
            @MouseScroll.canceled += instance.OnMouseScroll;
            @Sprint.started += instance.OnSprint;
            @Sprint.performed += instance.OnSprint;
            @Sprint.canceled += instance.OnSprint;
            @ChangeMovementMode.started += instance.OnChangeMovementMode;
            @ChangeMovementMode.performed += instance.OnChangeMovementMode;
            @ChangeMovementMode.canceled += instance.OnChangeMovementMode;
            @Attack.started += instance.OnAttack;
            @Attack.performed += instance.OnAttack;
            @Attack.canceled += instance.OnAttack;
            @Interact.started += instance.OnInteract;
            @Interact.performed += instance.OnInteract;
            @Interact.canceled += instance.OnInteract;
            @ChangeCurrentWeapon.started += instance.OnChangeCurrentWeapon;
            @ChangeCurrentWeapon.performed += instance.OnChangeCurrentWeapon;
            @ChangeCurrentWeapon.canceled += instance.OnChangeCurrentWeapon;
            @DropCurrentWeapon.started += instance.OnDropCurrentWeapon;
            @DropCurrentWeapon.performed += instance.OnDropCurrentWeapon;
            @DropCurrentWeapon.canceled += instance.OnDropCurrentWeapon;
            @OpenCharacterInfo.started += instance.OnOpenCharacterInfo;
            @OpenCharacterInfo.performed += instance.OnOpenCharacterInfo;
            @OpenCharacterInfo.canceled += instance.OnOpenCharacterInfo;
            @UpgradesStartDraging.started += instance.OnUpgradesStartDraging;
            @UpgradesStartDraging.performed += instance.OnUpgradesStartDraging;
            @UpgradesStartDraging.canceled += instance.OnUpgradesStartDraging;
            @CurrentMousePosition.started += instance.OnCurrentMousePosition;
            @CurrentMousePosition.performed += instance.OnCurrentMousePosition;
            @CurrentMousePosition.canceled += instance.OnCurrentMousePosition;
            @GamePause.started += instance.OnGamePause;
            @GamePause.performed += instance.OnGamePause;
            @GamePause.canceled += instance.OnGamePause;
        }

        private void UnregisterCallbacks(IPlayerActions instance)
        {
            @Movement.started -= instance.OnMovement;
            @Movement.performed -= instance.OnMovement;
            @Movement.canceled -= instance.OnMovement;
            @MousePosition.started -= instance.OnMousePosition;
            @MousePosition.performed -= instance.OnMousePosition;
            @MousePosition.canceled -= instance.OnMousePosition;
            @CameraModeChange.started -= instance.OnCameraModeChange;
            @CameraModeChange.performed -= instance.OnCameraModeChange;
            @CameraModeChange.canceled -= instance.OnCameraModeChange;
            @MouseScroll.started -= instance.OnMouseScroll;
            @MouseScroll.performed -= instance.OnMouseScroll;
            @MouseScroll.canceled -= instance.OnMouseScroll;
            @Sprint.started -= instance.OnSprint;
            @Sprint.performed -= instance.OnSprint;
            @Sprint.canceled -= instance.OnSprint;
            @ChangeMovementMode.started -= instance.OnChangeMovementMode;
            @ChangeMovementMode.performed -= instance.OnChangeMovementMode;
            @ChangeMovementMode.canceled -= instance.OnChangeMovementMode;
            @Attack.started -= instance.OnAttack;
            @Attack.performed -= instance.OnAttack;
            @Attack.canceled -= instance.OnAttack;
            @Interact.started -= instance.OnInteract;
            @Interact.performed -= instance.OnInteract;
            @Interact.canceled -= instance.OnInteract;
            @ChangeCurrentWeapon.started -= instance.OnChangeCurrentWeapon;
            @ChangeCurrentWeapon.performed -= instance.OnChangeCurrentWeapon;
            @ChangeCurrentWeapon.canceled -= instance.OnChangeCurrentWeapon;
            @DropCurrentWeapon.started -= instance.OnDropCurrentWeapon;
            @DropCurrentWeapon.performed -= instance.OnDropCurrentWeapon;
            @DropCurrentWeapon.canceled -= instance.OnDropCurrentWeapon;
            @OpenCharacterInfo.started -= instance.OnOpenCharacterInfo;
            @OpenCharacterInfo.performed -= instance.OnOpenCharacterInfo;
            @OpenCharacterInfo.canceled -= instance.OnOpenCharacterInfo;
            @UpgradesStartDraging.started -= instance.OnUpgradesStartDraging;
            @UpgradesStartDraging.performed -= instance.OnUpgradesStartDraging;
            @UpgradesStartDraging.canceled -= instance.OnUpgradesStartDraging;
            @CurrentMousePosition.started -= instance.OnCurrentMousePosition;
            @CurrentMousePosition.performed -= instance.OnCurrentMousePosition;
            @CurrentMousePosition.canceled -= instance.OnCurrentMousePosition;
            @GamePause.started -= instance.OnGamePause;
            @GamePause.performed -= instance.OnGamePause;
            @GamePause.canceled -= instance.OnGamePause;
        }

        public void RemoveCallbacks(IPlayerActions instance)
        {
            if (m_Wrapper.m_PlayerActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IPlayerActions instance)
        {
            foreach (var item in m_Wrapper.m_PlayerActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_PlayerActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public PlayerActions @Player => new PlayerActions(this);
    public interface IPlayerActions
    {
        void OnMovement(InputAction.CallbackContext context);
        void OnMousePosition(InputAction.CallbackContext context);
        void OnCameraModeChange(InputAction.CallbackContext context);
        void OnMouseScroll(InputAction.CallbackContext context);
        void OnSprint(InputAction.CallbackContext context);
        void OnChangeMovementMode(InputAction.CallbackContext context);
        void OnAttack(InputAction.CallbackContext context);
        void OnInteract(InputAction.CallbackContext context);
        void OnChangeCurrentWeapon(InputAction.CallbackContext context);
        void OnDropCurrentWeapon(InputAction.CallbackContext context);
        void OnOpenCharacterInfo(InputAction.CallbackContext context);
        void OnUpgradesStartDraging(InputAction.CallbackContext context);
        void OnCurrentMousePosition(InputAction.CallbackContext context);
        void OnGamePause(InputAction.CallbackContext context);
    }
}

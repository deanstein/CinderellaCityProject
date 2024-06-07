// GENERATED AUTOMATICALLY FROM 'Assets/Scripts/InputMaster.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @InputMaster : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @InputMaster()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""InputMaster"",
    ""maps"": [
        {
            ""name"": ""Player"",
            ""id"": ""b0cabd14-0965-4b8b-b30f-bb53eb46735b"",
            ""actions"": [
                {
                    ""name"": ""Jump"",
                    ""type"": ""Button"",
                    ""id"": ""144999de-c8f1-4d06-b182-df36f21575fe"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Time Travel Forward"",
                    ""type"": ""Value"",
                    ""id"": ""2506a877-31d8-4dca-8adb-b415ceca5afb"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Look"",
                    ""type"": ""Value"",
                    ""id"": ""b6b00544-5500-4c55-9ca1-c2a63f530035"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Time Travel Backward"",
                    ""type"": ""Value"",
                    ""id"": ""f9e4dfe1-6048-4a73-8a21-7b1e6627b08d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Pause"",
                    ""type"": ""Value"",
                    ""id"": ""33987f7b-f18b-4534-adf9-bfc90692aa73"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Invert Y Axis"",
                    ""type"": ""Button"",
                    ""id"": ""ec3f4b76-1581-4e50-842b-afe477a8637a"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""04770b1a-cd21-4fc0-979f-566e078d5304"",
                    ""path"": ""<XInputController>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox Control Scheme"",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""61fc5951-f868-4cc9-8028-4b4ae96ec5ce"",
                    ""path"": ""<XInputController>/rightShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox Control Scheme"",
                    ""action"": ""Time Travel Forward"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""289bdd39-a830-4bab-8431-cd9e722469ec"",
                    ""path"": ""<Gamepad>/rightStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox Control Scheme"",
                    ""action"": ""Look"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2d3b8aa7-5362-4117-b880-14fbdc78dbfb"",
                    ""path"": ""<XInputController>/leftShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox Control Scheme"",
                    ""action"": ""Time Travel Backward"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c806b6e8-d65f-4deb-bc60-d8f7b8d47a09"",
                    ""path"": ""<Gamepad>/start"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox Control Scheme"",
                    ""action"": ""Pause"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""6d316fe7-4964-49af-97c2-ac56794271b5"",
                    ""path"": ""<XInputController>/select"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox Control Scheme"",
                    ""action"": ""Invert Y Axis"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Xbox Control Scheme"",
            ""bindingGroup"": ""Xbox Control Scheme"",
            ""devices"": [
                {
                    ""devicePath"": ""<XInputController>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // Player
        m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
        m_Player_Jump = m_Player.FindAction("Jump", throwIfNotFound: true);
        m_Player_TimeTravelForward = m_Player.FindAction("Time Travel Forward", throwIfNotFound: true);
        m_Player_Look = m_Player.FindAction("Look", throwIfNotFound: true);
        m_Player_TimeTravelBackward = m_Player.FindAction("Time Travel Backward", throwIfNotFound: true);
        m_Player_Pause = m_Player.FindAction("Pause", throwIfNotFound: true);
        m_Player_InvertYAxis = m_Player.FindAction("Invert Y Axis", throwIfNotFound: true);
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

    // Player
    private readonly InputActionMap m_Player;
    private IPlayerActions m_PlayerActionsCallbackInterface;
    private readonly InputAction m_Player_Jump;
    private readonly InputAction m_Player_TimeTravelForward;
    private readonly InputAction m_Player_Look;
    private readonly InputAction m_Player_TimeTravelBackward;
    private readonly InputAction m_Player_Pause;
    private readonly InputAction m_Player_InvertYAxis;
    public struct PlayerActions
    {
        private @InputMaster m_Wrapper;
        public PlayerActions(@InputMaster wrapper) { m_Wrapper = wrapper; }
        public InputAction @Jump => m_Wrapper.m_Player_Jump;
        public InputAction @TimeTravelForward => m_Wrapper.m_Player_TimeTravelForward;
        public InputAction @Look => m_Wrapper.m_Player_Look;
        public InputAction @TimeTravelBackward => m_Wrapper.m_Player_TimeTravelBackward;
        public InputAction @Pause => m_Wrapper.m_Player_Pause;
        public InputAction @InvertYAxis => m_Wrapper.m_Player_InvertYAxis;
        public InputActionMap Get() { return m_Wrapper.m_Player; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerActions set) { return set.Get(); }
        public void SetCallbacks(IPlayerActions instance)
        {
            if (m_Wrapper.m_PlayerActionsCallbackInterface != null)
            {
                @Jump.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJump;
                @Jump.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJump;
                @Jump.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJump;
                @TimeTravelForward.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnTimeTravelForward;
                @TimeTravelForward.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnTimeTravelForward;
                @TimeTravelForward.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnTimeTravelForward;
                @Look.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnLook;
                @Look.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnLook;
                @Look.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnLook;
                @TimeTravelBackward.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnTimeTravelBackward;
                @TimeTravelBackward.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnTimeTravelBackward;
                @TimeTravelBackward.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnTimeTravelBackward;
                @Pause.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnPause;
                @Pause.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnPause;
                @Pause.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnPause;
                @InvertYAxis.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnInvertYAxis;
                @InvertYAxis.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnInvertYAxis;
                @InvertYAxis.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnInvertYAxis;
            }
            m_Wrapper.m_PlayerActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Jump.started += instance.OnJump;
                @Jump.performed += instance.OnJump;
                @Jump.canceled += instance.OnJump;
                @TimeTravelForward.started += instance.OnTimeTravelForward;
                @TimeTravelForward.performed += instance.OnTimeTravelForward;
                @TimeTravelForward.canceled += instance.OnTimeTravelForward;
                @Look.started += instance.OnLook;
                @Look.performed += instance.OnLook;
                @Look.canceled += instance.OnLook;
                @TimeTravelBackward.started += instance.OnTimeTravelBackward;
                @TimeTravelBackward.performed += instance.OnTimeTravelBackward;
                @TimeTravelBackward.canceled += instance.OnTimeTravelBackward;
                @Pause.started += instance.OnPause;
                @Pause.performed += instance.OnPause;
                @Pause.canceled += instance.OnPause;
                @InvertYAxis.started += instance.OnInvertYAxis;
                @InvertYAxis.performed += instance.OnInvertYAxis;
                @InvertYAxis.canceled += instance.OnInvertYAxis;
            }
        }
    }
    public PlayerActions @Player => new PlayerActions(this);
    private int m_XboxControlSchemeSchemeIndex = -1;
    public InputControlScheme XboxControlSchemeScheme
    {
        get
        {
            if (m_XboxControlSchemeSchemeIndex == -1) m_XboxControlSchemeSchemeIndex = asset.FindControlSchemeIndex("Xbox Control Scheme");
            return asset.controlSchemes[m_XboxControlSchemeSchemeIndex];
        }
    }
    public interface IPlayerActions
    {
        void OnJump(InputAction.CallbackContext context);
        void OnTimeTravelForward(InputAction.CallbackContext context);
        void OnLook(InputAction.CallbackContext context);
        void OnTimeTravelBackward(InputAction.CallbackContext context);
        void OnPause(InputAction.CallbackContext context);
        void OnInvertYAxis(InputAction.CallbackContext context);
    }
}

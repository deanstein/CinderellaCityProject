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
                    ""name"": ""Time Travel Forward"",
                    ""type"": ""Button"",
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
                    ""type"": ""Button"",
                    ""id"": ""f9e4dfe1-6048-4a73-8a21-7b1e6627b08d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Pause"",
                    ""type"": ""Button"",
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
                },
                {
                    ""name"": ""Toggle Historic Photos"",
                    ""type"": ""Button"",
                    ""id"": ""25021652-9b97-4476-9926-ef1b3b58c8e6"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Run"",
                    ""type"": ""Button"",
                    ""id"": ""b8e7e7d3-8415-4780-b9f9-a4968b73741c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Toggle People"",
                    ""type"": ""Button"",
                    ""id"": ""2d37f9b8-7948-453a-a8ef-169f49b2259e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Start Guided Tour"",
                    ""type"": ""Button"",
                    ""id"": ""e065c0b7-79e9-4656-8d2b-572e8248440e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""End Guided Tour"",
                    ""type"": ""Button"",
                    ""id"": ""f3465d85-53ca-4859-bc81-b6a6e3f65b23"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Toggle Guided Tour"",
                    ""type"": ""Button"",
                    ""id"": ""274fbeda-e318-45ac-be33-385cdb5a72be"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Next Photo"",
                    ""type"": ""Button"",
                    ""id"": ""081fbecd-6bfa-4775-954a-b72ac805bff1"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Previous Photo"",
                    ""type"": ""Button"",
                    ""id"": ""c10b0693-ef50-42a9-937a-8576f4363d71"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
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
                },
                {
                    ""name"": """",
                    ""id"": ""51b92423-d964-4fa7-ad21-e8574170b73c"",
                    ""path"": ""<XInputController>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox Control Scheme"",
                    ""action"": ""Toggle Historic Photos"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ee0d0e04-16b9-480f-94d2-4c8941774934"",
                    ""path"": ""<XInputController>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox Control Scheme"",
                    ""action"": ""Run"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ec7f7450-c887-4f5d-95e2-914024e16215"",
                    ""path"": ""<XInputController>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox Control Scheme"",
                    ""action"": ""Toggle People"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1a5cebc1-34a4-4ae0-910a-e32efc190e07"",
                    ""path"": ""<XInputController>/dpad/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox Control Scheme"",
                    ""action"": ""Start Guided Tour"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""5e6a9b1b-85df-406b-aca9-ad1074df40a0"",
                    ""path"": ""<XInputController>/dpad/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox Control Scheme"",
                    ""action"": ""End Guided Tour"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1a7cb498-0bae-4f49-a043-4236c6f3bc6e"",
                    ""path"": ""<XInputController>/dpad/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox Control Scheme"",
                    ""action"": ""Next Photo"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2f0b932f-9c5b-4c87-a76e-411fdc3de876"",
                    ""path"": ""<XInputController>/dpad/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox Control Scheme"",
                    ""action"": ""Previous Photo"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""03d942e9-0ded-4249-95aa-8e0130dd2def"",
                    ""path"": ""<Gamepad>/start"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Xbox Control Scheme"",
                    ""action"": ""Toggle Guided Tour"",
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
        m_Player_TimeTravelForward = m_Player.FindAction("Time Travel Forward", throwIfNotFound: true);
        m_Player_Look = m_Player.FindAction("Look", throwIfNotFound: true);
        m_Player_TimeTravelBackward = m_Player.FindAction("Time Travel Backward", throwIfNotFound: true);
        m_Player_Pause = m_Player.FindAction("Pause", throwIfNotFound: true);
        m_Player_InvertYAxis = m_Player.FindAction("Invert Y Axis", throwIfNotFound: true);
        m_Player_ToggleHistoricPhotos = m_Player.FindAction("Toggle Historic Photos", throwIfNotFound: true);
        m_Player_Run = m_Player.FindAction("Run", throwIfNotFound: true);
        m_Player_TogglePeople = m_Player.FindAction("Toggle People", throwIfNotFound: true);
        m_Player_StartGuidedTour = m_Player.FindAction("Start Guided Tour", throwIfNotFound: true);
        m_Player_EndGuidedTour = m_Player.FindAction("End Guided Tour", throwIfNotFound: true);
        m_Player_ToggleGuidedTour = m_Player.FindAction("Toggle Guided Tour", throwIfNotFound: true);
        m_Player_NextPhoto = m_Player.FindAction("Next Photo", throwIfNotFound: true);
        m_Player_PreviousPhoto = m_Player.FindAction("Previous Photo", throwIfNotFound: true);
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
    private readonly InputAction m_Player_TimeTravelForward;
    private readonly InputAction m_Player_Look;
    private readonly InputAction m_Player_TimeTravelBackward;
    private readonly InputAction m_Player_Pause;
    private readonly InputAction m_Player_InvertYAxis;
    private readonly InputAction m_Player_ToggleHistoricPhotos;
    private readonly InputAction m_Player_Run;
    private readonly InputAction m_Player_TogglePeople;
    private readonly InputAction m_Player_StartGuidedTour;
    private readonly InputAction m_Player_EndGuidedTour;
    private readonly InputAction m_Player_ToggleGuidedTour;
    private readonly InputAction m_Player_NextPhoto;
    private readonly InputAction m_Player_PreviousPhoto;
    public struct PlayerActions
    {
        private @InputMaster m_Wrapper;
        public PlayerActions(@InputMaster wrapper) { m_Wrapper = wrapper; }
        public InputAction @TimeTravelForward => m_Wrapper.m_Player_TimeTravelForward;
        public InputAction @Look => m_Wrapper.m_Player_Look;
        public InputAction @TimeTravelBackward => m_Wrapper.m_Player_TimeTravelBackward;
        public InputAction @Pause => m_Wrapper.m_Player_Pause;
        public InputAction @InvertYAxis => m_Wrapper.m_Player_InvertYAxis;
        public InputAction @ToggleHistoricPhotos => m_Wrapper.m_Player_ToggleHistoricPhotos;
        public InputAction @Run => m_Wrapper.m_Player_Run;
        public InputAction @TogglePeople => m_Wrapper.m_Player_TogglePeople;
        public InputAction @StartGuidedTour => m_Wrapper.m_Player_StartGuidedTour;
        public InputAction @EndGuidedTour => m_Wrapper.m_Player_EndGuidedTour;
        public InputAction @ToggleGuidedTour => m_Wrapper.m_Player_ToggleGuidedTour;
        public InputAction @NextPhoto => m_Wrapper.m_Player_NextPhoto;
        public InputAction @PreviousPhoto => m_Wrapper.m_Player_PreviousPhoto;
        public InputActionMap Get() { return m_Wrapper.m_Player; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerActions set) { return set.Get(); }
        public void SetCallbacks(IPlayerActions instance)
        {
            if (m_Wrapper.m_PlayerActionsCallbackInterface != null)
            {
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
                @ToggleHistoricPhotos.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnToggleHistoricPhotos;
                @ToggleHistoricPhotos.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnToggleHistoricPhotos;
                @ToggleHistoricPhotos.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnToggleHistoricPhotos;
                @Run.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnRun;
                @Run.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnRun;
                @Run.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnRun;
                @TogglePeople.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnTogglePeople;
                @TogglePeople.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnTogglePeople;
                @TogglePeople.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnTogglePeople;
                @StartGuidedTour.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnStartGuidedTour;
                @StartGuidedTour.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnStartGuidedTour;
                @StartGuidedTour.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnStartGuidedTour;
                @EndGuidedTour.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnEndGuidedTour;
                @EndGuidedTour.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnEndGuidedTour;
                @EndGuidedTour.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnEndGuidedTour;
                @ToggleGuidedTour.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnToggleGuidedTour;
                @ToggleGuidedTour.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnToggleGuidedTour;
                @ToggleGuidedTour.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnToggleGuidedTour;
                @NextPhoto.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnNextPhoto;
                @NextPhoto.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnNextPhoto;
                @NextPhoto.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnNextPhoto;
                @PreviousPhoto.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnPreviousPhoto;
                @PreviousPhoto.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnPreviousPhoto;
                @PreviousPhoto.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnPreviousPhoto;
            }
            m_Wrapper.m_PlayerActionsCallbackInterface = instance;
            if (instance != null)
            {
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
                @ToggleHistoricPhotos.started += instance.OnToggleHistoricPhotos;
                @ToggleHistoricPhotos.performed += instance.OnToggleHistoricPhotos;
                @ToggleHistoricPhotos.canceled += instance.OnToggleHistoricPhotos;
                @Run.started += instance.OnRun;
                @Run.performed += instance.OnRun;
                @Run.canceled += instance.OnRun;
                @TogglePeople.started += instance.OnTogglePeople;
                @TogglePeople.performed += instance.OnTogglePeople;
                @TogglePeople.canceled += instance.OnTogglePeople;
                @StartGuidedTour.started += instance.OnStartGuidedTour;
                @StartGuidedTour.performed += instance.OnStartGuidedTour;
                @StartGuidedTour.canceled += instance.OnStartGuidedTour;
                @EndGuidedTour.started += instance.OnEndGuidedTour;
                @EndGuidedTour.performed += instance.OnEndGuidedTour;
                @EndGuidedTour.canceled += instance.OnEndGuidedTour;
                @ToggleGuidedTour.started += instance.OnToggleGuidedTour;
                @ToggleGuidedTour.performed += instance.OnToggleGuidedTour;
                @ToggleGuidedTour.canceled += instance.OnToggleGuidedTour;
                @NextPhoto.started += instance.OnNextPhoto;
                @NextPhoto.performed += instance.OnNextPhoto;
                @NextPhoto.canceled += instance.OnNextPhoto;
                @PreviousPhoto.started += instance.OnPreviousPhoto;
                @PreviousPhoto.performed += instance.OnPreviousPhoto;
                @PreviousPhoto.canceled += instance.OnPreviousPhoto;
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
        void OnTimeTravelForward(InputAction.CallbackContext context);
        void OnLook(InputAction.CallbackContext context);
        void OnTimeTravelBackward(InputAction.CallbackContext context);
        void OnPause(InputAction.CallbackContext context);
        void OnInvertYAxis(InputAction.CallbackContext context);
        void OnToggleHistoricPhotos(InputAction.CallbackContext context);
        void OnRun(InputAction.CallbackContext context);
        void OnTogglePeople(InputAction.CallbackContext context);
        void OnStartGuidedTour(InputAction.CallbackContext context);
        void OnEndGuidedTour(InputAction.CallbackContext context);
        void OnToggleGuidedTour(InputAction.CallbackContext context);
        void OnNextPhoto(InputAction.CallbackContext context);
        void OnPreviousPhoto(InputAction.CallbackContext context);
    }
}

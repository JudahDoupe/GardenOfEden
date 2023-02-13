//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.4.4
//     from Assets/_Gameplay/Input/Controls.inputactions
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

public partial class @Controls : IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @Controls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""Controls"",
    ""maps"": [
        {
            ""name"": ""Exhibit"",
            ""id"": ""6e55ae37-b0c3-49e7-947a-14d650e4cbde"",
            ""actions"": [
                {
                    ""name"": ""Forward"",
                    ""type"": ""Button"",
                    ""id"": ""344b913c-fe8e-464a-bb1c-3baf13547dac"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press"",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Back"",
                    ""type"": ""Button"",
                    ""id"": ""8800ee36-67e1-4fd6-90ec-1478c4b6cd8e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press"",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""4ae8c2f3-893b-4dcb-8b29-d1db1b9bd95d"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Forward"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""3d83a0bd-2840-4478-ba63-0f9e2ffd831d"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Forward"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""4e8f3a16-44f3-41a2-b11f-b6bf190308b3"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Back"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Standard"",
            ""id"": ""c2c66244-830c-486a-b079-6c4a4cc20d1c"",
            ""actions"": [
                {
                    ""name"": ""Click"",
                    ""type"": ""Button"",
                    ""id"": ""f2dcf9e0-0172-4dd0-a922-74918963f452"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Drag"",
                    ""type"": ""Value"",
                    ""id"": ""c1fd6537-cf35-4766-9d47-d8137945e846"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""LeftMove"",
                    ""type"": ""Value"",
                    ""id"": ""c3026d38-d552-4201-b9c8-b16153b098b8"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""RightMove"",
                    ""type"": ""Value"",
                    ""id"": ""23d25871-427a-41d7-9ac7-52807bc737df"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Confirm"",
                    ""type"": ""Button"",
                    ""id"": ""a07dbf1c-7948-4be5-b48b-678db6f9ecd9"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Cancel"",
                    ""type"": ""Button"",
                    ""id"": ""d83aa807-b7b7-4cef-81c7-2f90b2f71c85"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""RightClick"",
                    ""type"": ""Button"",
                    ""id"": ""af5eda91-c540-4225-b9b8-20ad9aac55e9"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Scroll"",
                    ""type"": ""Value"",
                    ""id"": ""83e98059-95f2-4da4-b705-9f132d1b327a"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": ""Invert"",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""MoveModifier"",
                    ""type"": ""Value"",
                    ""id"": ""85cd4c22-55a2-4f00-953e-32ea6bd7d599"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Debug"",
                    ""type"": ""Button"",
                    ""id"": ""8006165d-33cb-426f-9664-b67669adf3ab"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""433d544f-e85b-4576-aac1-edd097450782"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Click"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""5935ec2a-16d0-457e-80dc-ce8e551484dd"",
                    ""path"": ""<Mouse>/delta"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Drag"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Keys"",
                    ""id"": ""aa545f30-c848-451f-8fa0-6444ef9086fd"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""LeftMove"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""a9d3a05f-a216-425f-8c01-2fdcf5a4307d"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""LeftMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""73e36077-55b5-44e1-8169-83efad0b9e17"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""LeftMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""a14d5fa5-01e5-44fa-afcf-153ae3f036d0"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""LeftMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""ffca0aff-7abb-45d1-9f19-bc1e7aea8859"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""LeftMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Keys"",
                    ""id"": ""41ae86d6-a732-4afb-8c4d-ed164227ae50"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""RightMove"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""984026ec-b205-4828-89cf-692a8da265cc"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""RightMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""096b4f02-f449-4189-a303-1bb5a1c0ccd7"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""RightMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""88eed4c0-26da-41ca-b582-f1f76d19a3a3"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""RightMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""6f909522-8ad1-4adb-8a20-e00188f875f2"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""RightMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""2cf33b1c-0b4d-4c23-bfcc-758ca3f43977"",
                    ""path"": ""<Keyboard>/enter"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Confirm"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e3baaa16-fec1-4613-a7e2-8c3ef5a9d7c0"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Cancel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8b859901-d7d2-4e62-b73d-8acb05e891b9"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""RightClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2c883648-2abe-4e62-bb54-7977ac2ccc55"",
                    ""path"": ""<Mouse>/scroll/y"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Scroll"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""67f8cf0d-a4ce-4d7c-b3d1-b94a9c4b93f3"",
                    ""path"": ""1DAxis(minValue=-0.7,maxValue=1.5)"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveModifier"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""39a4de9c-8994-465e-81d1-bf82c0fe7318"",
                    ""path"": ""<Keyboard>/ctrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveModifier"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""ff9f47ae-3c88-4895-98bd-cb284389d2d5"",
                    ""path"": ""<Keyboard>/shift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MoveModifier"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""ba484745-ba15-4348-a8e9-b3bec0035564"",
                    ""path"": ""<Keyboard>/f5"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Debug"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Keyboard/Mouse"",
            ""bindingGroup"": ""Keyboard/Mouse"",
            ""devices"": []
        }
    ]
}");
        // Exhibit
        m_Exhibit = asset.FindActionMap("Exhibit", throwIfNotFound: true);
        m_Exhibit_Forward = m_Exhibit.FindAction("Forward", throwIfNotFound: true);
        m_Exhibit_Back = m_Exhibit.FindAction("Back", throwIfNotFound: true);
        // Standard
        m_Standard = asset.FindActionMap("Standard", throwIfNotFound: true);
        m_Standard_Click = m_Standard.FindAction("Click", throwIfNotFound: true);
        m_Standard_Drag = m_Standard.FindAction("Drag", throwIfNotFound: true);
        m_Standard_LeftMove = m_Standard.FindAction("LeftMove", throwIfNotFound: true);
        m_Standard_RightMove = m_Standard.FindAction("RightMove", throwIfNotFound: true);
        m_Standard_Confirm = m_Standard.FindAction("Confirm", throwIfNotFound: true);
        m_Standard_Cancel = m_Standard.FindAction("Cancel", throwIfNotFound: true);
        m_Standard_RightClick = m_Standard.FindAction("RightClick", throwIfNotFound: true);
        m_Standard_Scroll = m_Standard.FindAction("Scroll", throwIfNotFound: true);
        m_Standard_MoveModifier = m_Standard.FindAction("MoveModifier", throwIfNotFound: true);
        m_Standard_Debug = m_Standard.FindAction("Debug", throwIfNotFound: true);
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

    // Exhibit
    private readonly InputActionMap m_Exhibit;
    private IExhibitActions m_ExhibitActionsCallbackInterface;
    private readonly InputAction m_Exhibit_Forward;
    private readonly InputAction m_Exhibit_Back;
    public struct ExhibitActions
    {
        private @Controls m_Wrapper;
        public ExhibitActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Forward => m_Wrapper.m_Exhibit_Forward;
        public InputAction @Back => m_Wrapper.m_Exhibit_Back;
        public InputActionMap Get() { return m_Wrapper.m_Exhibit; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(ExhibitActions set) { return set.Get(); }
        public void SetCallbacks(IExhibitActions instance)
        {
            if (m_Wrapper.m_ExhibitActionsCallbackInterface != null)
            {
                @Forward.started -= m_Wrapper.m_ExhibitActionsCallbackInterface.OnForward;
                @Forward.performed -= m_Wrapper.m_ExhibitActionsCallbackInterface.OnForward;
                @Forward.canceled -= m_Wrapper.m_ExhibitActionsCallbackInterface.OnForward;
                @Back.started -= m_Wrapper.m_ExhibitActionsCallbackInterface.OnBack;
                @Back.performed -= m_Wrapper.m_ExhibitActionsCallbackInterface.OnBack;
                @Back.canceled -= m_Wrapper.m_ExhibitActionsCallbackInterface.OnBack;
            }
            m_Wrapper.m_ExhibitActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Forward.started += instance.OnForward;
                @Forward.performed += instance.OnForward;
                @Forward.canceled += instance.OnForward;
                @Back.started += instance.OnBack;
                @Back.performed += instance.OnBack;
                @Back.canceled += instance.OnBack;
            }
        }
    }
    public ExhibitActions @Exhibit => new ExhibitActions(this);

    // Standard
    private readonly InputActionMap m_Standard;
    private IStandardActions m_StandardActionsCallbackInterface;
    private readonly InputAction m_Standard_Click;
    private readonly InputAction m_Standard_Drag;
    private readonly InputAction m_Standard_LeftMove;
    private readonly InputAction m_Standard_RightMove;
    private readonly InputAction m_Standard_Confirm;
    private readonly InputAction m_Standard_Cancel;
    private readonly InputAction m_Standard_RightClick;
    private readonly InputAction m_Standard_Scroll;
    private readonly InputAction m_Standard_MoveModifier;
    private readonly InputAction m_Standard_Debug;
    public struct StandardActions
    {
        private @Controls m_Wrapper;
        public StandardActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Click => m_Wrapper.m_Standard_Click;
        public InputAction @Drag => m_Wrapper.m_Standard_Drag;
        public InputAction @LeftMove => m_Wrapper.m_Standard_LeftMove;
        public InputAction @RightMove => m_Wrapper.m_Standard_RightMove;
        public InputAction @Confirm => m_Wrapper.m_Standard_Confirm;
        public InputAction @Cancel => m_Wrapper.m_Standard_Cancel;
        public InputAction @RightClick => m_Wrapper.m_Standard_RightClick;
        public InputAction @Scroll => m_Wrapper.m_Standard_Scroll;
        public InputAction @MoveModifier => m_Wrapper.m_Standard_MoveModifier;
        public InputAction @Debug => m_Wrapper.m_Standard_Debug;
        public InputActionMap Get() { return m_Wrapper.m_Standard; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(StandardActions set) { return set.Get(); }
        public void SetCallbacks(IStandardActions instance)
        {
            if (m_Wrapper.m_StandardActionsCallbackInterface != null)
            {
                @Click.started -= m_Wrapper.m_StandardActionsCallbackInterface.OnClick;
                @Click.performed -= m_Wrapper.m_StandardActionsCallbackInterface.OnClick;
                @Click.canceled -= m_Wrapper.m_StandardActionsCallbackInterface.OnClick;
                @Drag.started -= m_Wrapper.m_StandardActionsCallbackInterface.OnDrag;
                @Drag.performed -= m_Wrapper.m_StandardActionsCallbackInterface.OnDrag;
                @Drag.canceled -= m_Wrapper.m_StandardActionsCallbackInterface.OnDrag;
                @LeftMove.started -= m_Wrapper.m_StandardActionsCallbackInterface.OnLeftMove;
                @LeftMove.performed -= m_Wrapper.m_StandardActionsCallbackInterface.OnLeftMove;
                @LeftMove.canceled -= m_Wrapper.m_StandardActionsCallbackInterface.OnLeftMove;
                @RightMove.started -= m_Wrapper.m_StandardActionsCallbackInterface.OnRightMove;
                @RightMove.performed -= m_Wrapper.m_StandardActionsCallbackInterface.OnRightMove;
                @RightMove.canceled -= m_Wrapper.m_StandardActionsCallbackInterface.OnRightMove;
                @Confirm.started -= m_Wrapper.m_StandardActionsCallbackInterface.OnConfirm;
                @Confirm.performed -= m_Wrapper.m_StandardActionsCallbackInterface.OnConfirm;
                @Confirm.canceled -= m_Wrapper.m_StandardActionsCallbackInterface.OnConfirm;
                @Cancel.started -= m_Wrapper.m_StandardActionsCallbackInterface.OnCancel;
                @Cancel.performed -= m_Wrapper.m_StandardActionsCallbackInterface.OnCancel;
                @Cancel.canceled -= m_Wrapper.m_StandardActionsCallbackInterface.OnCancel;
                @RightClick.started -= m_Wrapper.m_StandardActionsCallbackInterface.OnRightClick;
                @RightClick.performed -= m_Wrapper.m_StandardActionsCallbackInterface.OnRightClick;
                @RightClick.canceled -= m_Wrapper.m_StandardActionsCallbackInterface.OnRightClick;
                @Scroll.started -= m_Wrapper.m_StandardActionsCallbackInterface.OnScroll;
                @Scroll.performed -= m_Wrapper.m_StandardActionsCallbackInterface.OnScroll;
                @Scroll.canceled -= m_Wrapper.m_StandardActionsCallbackInterface.OnScroll;
                @MoveModifier.started -= m_Wrapper.m_StandardActionsCallbackInterface.OnMoveModifier;
                @MoveModifier.performed -= m_Wrapper.m_StandardActionsCallbackInterface.OnMoveModifier;
                @MoveModifier.canceled -= m_Wrapper.m_StandardActionsCallbackInterface.OnMoveModifier;
                @Debug.started -= m_Wrapper.m_StandardActionsCallbackInterface.OnDebug;
                @Debug.performed -= m_Wrapper.m_StandardActionsCallbackInterface.OnDebug;
                @Debug.canceled -= m_Wrapper.m_StandardActionsCallbackInterface.OnDebug;
            }
            m_Wrapper.m_StandardActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Click.started += instance.OnClick;
                @Click.performed += instance.OnClick;
                @Click.canceled += instance.OnClick;
                @Drag.started += instance.OnDrag;
                @Drag.performed += instance.OnDrag;
                @Drag.canceled += instance.OnDrag;
                @LeftMove.started += instance.OnLeftMove;
                @LeftMove.performed += instance.OnLeftMove;
                @LeftMove.canceled += instance.OnLeftMove;
                @RightMove.started += instance.OnRightMove;
                @RightMove.performed += instance.OnRightMove;
                @RightMove.canceled += instance.OnRightMove;
                @Confirm.started += instance.OnConfirm;
                @Confirm.performed += instance.OnConfirm;
                @Confirm.canceled += instance.OnConfirm;
                @Cancel.started += instance.OnCancel;
                @Cancel.performed += instance.OnCancel;
                @Cancel.canceled += instance.OnCancel;
                @RightClick.started += instance.OnRightClick;
                @RightClick.performed += instance.OnRightClick;
                @RightClick.canceled += instance.OnRightClick;
                @Scroll.started += instance.OnScroll;
                @Scroll.performed += instance.OnScroll;
                @Scroll.canceled += instance.OnScroll;
                @MoveModifier.started += instance.OnMoveModifier;
                @MoveModifier.performed += instance.OnMoveModifier;
                @MoveModifier.canceled += instance.OnMoveModifier;
                @Debug.started += instance.OnDebug;
                @Debug.performed += instance.OnDebug;
                @Debug.canceled += instance.OnDebug;
            }
        }
    }
    public StandardActions @Standard => new StandardActions(this);
    private int m_KeyboardMouseSchemeIndex = -1;
    public InputControlScheme KeyboardMouseScheme
    {
        get
        {
            if (m_KeyboardMouseSchemeIndex == -1) m_KeyboardMouseSchemeIndex = asset.FindControlSchemeIndex("Keyboard/Mouse");
            return asset.controlSchemes[m_KeyboardMouseSchemeIndex];
        }
    }
    public interface IExhibitActions
    {
        void OnForward(InputAction.CallbackContext context);
        void OnBack(InputAction.CallbackContext context);
    }
    public interface IStandardActions
    {
        void OnClick(InputAction.CallbackContext context);
        void OnDrag(InputAction.CallbackContext context);
        void OnLeftMove(InputAction.CallbackContext context);
        void OnRightMove(InputAction.CallbackContext context);
        void OnConfirm(InputAction.CallbackContext context);
        void OnCancel(InputAction.CallbackContext context);
        void OnRightClick(InputAction.CallbackContext context);
        void OnScroll(InputAction.CallbackContext context);
        void OnMoveModifier(InputAction.CallbackContext context);
        void OnDebug(InputAction.CallbackContext context);
    }
}
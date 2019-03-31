using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using RaycastHit = UnityEngine.RaycastHit;

[AddComponentMenu("First Person Controller")]
public class FirstPersonController : MonoBehaviour {

    #region Look Settings
    
    [Space(8)]
    [Header("Look Settings")]
    [Space(8)]
    [Tooltip("Determines whether the player can move camera or not.")]
    public bool IsCameraMovable = true;

    [Tooltip("The range of rotation in degrees your neck can rotate along each axis.")]
    public Vector2 RangeOfMotion = new Vector2(170, Mathf.Infinity);

    [Tooltip("Determines how sensitive the mouse is.")]
    [Range(0.01f, 100)]
    public float MouseSensitivity = 10f;

    [Tooltip("Mouse Smoothness.")]
    [Range(0.01f, 100)]
    public float LookDamping = 0.05f;

    [Tooltip("For Debuging or if You don't plan on having a pause menu or quit button.")]
    public bool IsMouseHidden = false;
    
    public Camera Camera { get; set; }
    private Vector3 _targetAngles;
    private Vector3 _followAngles;
    private Vector3 _followVelocity;
    private Vector3 _originalRotation;

    #endregion

    #region Movement Settings

    [Space(8)]
    [Header("Movement Settings")]
    [Space(8)]
    [Tooltip ("Determines whether the player can move.")]
    public bool IsPlayerMovable = true;

    [Tooltip("Determines how fast Player walks.")]
    [Range(0.1f, 50)]
    public float WalkSpeed = 4f;

    [Tooltip("Determines how fast Player Sprints.")]
    [Range(0.1f, 50)]
    public float SprintSpeed = 8f;

    [Tooltip("Determines how fast Player Strafes.")]
    [Range(0.1f, 50)]
    public float StrafeSpeed = 4f;

    [Tooltip("Determines how high Player Jumps.")]
    [Range(0.1f, 50)]
    public float JumpPower = 5f;

    [Tooltip("Determines if the jump button needs to be pressed down to jump, or if the player can hold the jump button to automaticly jump every time the it hits the ground.")]
    public bool CanHoldJump = false;

    public bool IsGrounded { get; private set; }
    public bool IsCrouching { get; private set; }
    public bool IsSprinting { get; private set; }
    private Rigidbody _rigidbody;

    #endregion

    #region Focus Settings

    [Space(8)]
    [Header("In Focus Settings")]
    [Space(8)]
    [Tooltip("Determines how far the cursor will project.")]
    public float ReachDistance = 5;

    [Tooltip("Determines when the cursor will attach to a near by object.")]
    public float SnapDistance = 0.5f;

    [Tooltip("Determines if the cursor will connect with objects in the world, or float in front of the player.")]
    public bool IsCursorFreeFloating = false;

    #endregion

    public Transform Focus { get; set; }

    public Transform RightHand { get; set; }
    public Item RightHandItem { get; set; }
    public Transform LeftHand { get; set; }
    public Item LeftHandItem { get; set; }

    void Start()
    {
        _originalRotation = transform.localRotation.eulerAngles;
        _rigidbody = GetComponent<Rigidbody>();
        Camera = transform.GetComponentInChildren<Camera>();
        RightHand = transform.Find("Body/RightArm/RightForeArm/RightHand");
        LeftHand = transform.Find("Body/LeftArm/LeftForeArm/LeftHand");
        Focus = transform.Find("Body/Head/Focus");
    }
    void LateUpdate()
    {
        //Camera
        if(IsCameraMovable)
        {
            _targetAngles.y += Input.GetAxis("Mouse X") * MouseSensitivity;
            _targetAngles.x += Input.GetAxis("Mouse Y") * MouseSensitivity;
            _targetAngles.y = Mathf.Clamp(_targetAngles.y, -0.5f * RangeOfMotion.y, 0.5f * RangeOfMotion.y);
            _targetAngles.x = Mathf.Clamp(_targetAngles.x, -0.5f * RangeOfMotion.x, 0.5f * RangeOfMotion.x);
        }

        _followAngles = Vector3.SmoothDamp(_followAngles, _targetAngles, ref _followVelocity, LookDamping);
        Camera.transform.localRotation = Quaternion.Euler(-_followAngles.x + _originalRotation.x,0,0);
        transform.localRotation =  Quaternion.Euler(0, _followAngles.y+_originalRotation.y, 0);

        Cursor.lockState = IsMouseHidden ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !IsMouseHidden;

        //Focus
        if (Focus != null)
        {
            Focus.transform.position = Camera.transform.position + Camera.transform.forward * ReachDistance;
            Focus.Find("FocusModel").gameObject.SetActive(false);

            if (IsCursorFreeFloating)
            {
                Focus.Find("FocusModel").gameObject.SetActive(true);
            }
            else
            {
                RaycastHit hit;
                if (Physics.Raycast(new Ray(Camera.transform.position, Camera.transform.forward), out hit, ReachDistance))
                {
                    Focus.transform.position = hit.point;
                    Focus.LookAt(Camera.transform);
                    Focus.Find("FocusModel").gameObject.SetActive(true);
                }

                var x1 = Physics.SphereCastAll(Focus.transform.position, SnapDistance, Camera.transform.forward);
                var x2 = x1.Select(x => GetInteractableTransformInParents(x.transform)?.GetComponent<Interactable>()).ToList();
                var x3 = x2.Where(x => x != null).ToList();
                var x4 = x3.Where(x => (RightHandItem?.IsUsable(this,x) ?? x.IsInteractable(this)) ||
                                (LeftHandItem?.IsUsable(this,x) ?? x.IsInteractable(this))).ToList();
                var x5 = x4.OrderBy(x => Vector3.Distance(x.InteractionPosition(), hit.point)).ToList();
                var interactable = x5.FirstOrDefault();

                if (interactable != null)
                {
                    //TODO: Have a right hand and left hand focus
                    Focus.transform.position = interactable.InteractionPosition();
                    Focus.LookAt(Camera.transform);
                }

                if (Input.GetMouseButtonDown(0))
                {
                    UseItem(LeftHandItem, interactable);
                }

                if (Input.GetMouseButtonDown(1))
                {
                    UseItem(RightHandItem, interactable);
                }
            }
        }
    }
    void FixedUpdate()
    {
        var inputVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (inputVector.magnitude > 1)
        {
            inputVector.Normalize();
        }

        IsSprinting = Input.GetKey(KeyCode.LeftShift);
        IsCrouching = Input.GetKey(KeyCode.LeftControl);
        var movementSpeed = IsSprinting && !IsCrouching ? SprintSpeed : WalkSpeed;
        var movementVelocity = transform.forward * inputVector.y * movementSpeed 
                             + transform.right * inputVector.x * StrafeSpeed
                             + transform.up * _rigidbody.velocity.y;

        var groundedRay = new Ray(transform.position, -transform.up);
        var groundedRayLength = 0.55f;
        IsGrounded = Physics.RaycastAll(groundedRay, groundedRayLength).Where(x => !x.collider.isTrigger).Any();

        var isJumping = CanHoldJump ? Input.GetButton("Jump") : Input.GetButtonDown("Jump");
        if (IsGrounded && isJumping)
        {
            movementVelocity += Vector3.up * JumpPower;
            IsGrounded = false;
        }

        _rigidbody.velocity = IsPlayerMovable ? movementVelocity : Vector3.zero;
    }

    public void GrabItem(Item item)
    {

        if (Input.GetMouseButtonDown(0))
        {
            DropItem(LeftHandItem);
            LeftHandItem = item;
            LeftHandItem.transform.parent = LeftHand;
            LeftHandItem.transform.localEulerAngles = Vector3.zero;
            LeftHandItem.transform.localPosition = -LeftHandItem.transform.InverseTransformPoint(LeftHandItem.InteractionPosition());
        }
        else
        {
            DropItem(RightHandItem);
            RightHandItem = item;
            RightHandItem.transform.parent = RightHand;
            RightHandItem.transform.localEulerAngles = Vector3.zero;
            RightHandItem.transform.localPosition = -RightHandItem.transform.InverseTransformPoint(RightHandItem.InteractionPosition());
        }

        Destroy(item.GetComponent<Rigidbody>());
    }
    public Item DropItem(Item droppedItem)
    {
        if (droppedItem == LeftHandItem)
        {
            LeftHandItem = null;
        }
        else if (droppedItem == RightHandItem)
        {
            RightHandItem = null;
        }

        if (droppedItem != null)
        {
            droppedItem.transform.parent = null;
            droppedItem.Fall();
            droppedItem.GetComponent<Rigidbody>()?.AddForce(transform.forward * 200);
        }

        return droppedItem;
    }
    private void UseItem(Item item, Interactable interactable)
    {
        if (item != null)
        {
            if (item.IsUsable(this, interactable))
            {
                item.Use(this, interactable);
            }
            else if (Focus.Find("FocusModel").gameObject.activeSelf)
            {
                DropItem(item);
            }
        }
        else if (interactable != null && interactable.IsInteractable(this))
        {
            interactable.Interact(this);
        }
    }

    private Transform GetInteractableTransformInParents(Transform t)
    {
        while (t != null && (t.GetComponent<Interactable>() == null))
        {
            t = t.parent;
        }
        return t;
    }
}




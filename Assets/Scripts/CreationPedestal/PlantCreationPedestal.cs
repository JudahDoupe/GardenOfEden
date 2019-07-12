using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public class PlantCreationPedestal : Interactable
{
    private FirstPersonController _creator;
    private Vector3 _cameraOffset = new Vector3(0, 0.5f, -2);

    public Transform _cameraTarget;
    public Vector3 CameraMoveSpeed = new Vector3(1,0.1f,0.5f);

    public DnaSelector SelectedDna { get; set; }
    public StructureSelector SelectedStructure { get; set; }
    public Plant Plant { get; set; }

    public override void Interact(FirstPersonController player)
    {
        _creator = player;
        StartCreation();
    }
    public override Vector3 InteractionPosition()
    {
        return transform.Find("Soil").transform.position;
    }

    private void StartCreation()
    {
        _creator.IsCameraMovable = false;
        _creator.IsPlayerMovable = false;
        _creator.IsMouseHidden = false;
        _creator.IsFocusEnabled = false;

        _creator.Camera.transform.parent = _cameraTarget;
    }
    public void EndCreation()
    {
        StartCoroutine(_endCreation());
    }
    private IEnumerator _endCreation()
    {
        var creator = _creator;
        _creator = null;
        SelectedDna?.Deselect();
        SelectedStructure?.ToggleSelect(this);
        creator.Camera.transform.parent = creator.transform.RecursiveFind("Head");
        var head = creator.transform.RecursiveFind("HeadModel").position;

        while (Vector3.Distance(creator.Camera.transform.localPosition, Vector3.zero) > 0.05f)
        {
            creator.Camera.transform.localPosition =
                Vector3.Lerp(creator.Camera.transform.localPosition, Vector3.zero, Time.unscaledDeltaTime * 5);
            creator.Camera.transform.localRotation =
                Quaternion.Lerp(creator.Camera.transform.localRotation, Quaternion.identity, Time.unscaledDeltaTime * 5);
            yield return new WaitForEndOfFrame();
        }

        creator.Camera.transform.localPosition = Vector3.zero;
        creator.IsCameraMovable = true;
        creator.IsPlayerMovable = true;
        creator.IsMouseHidden = true;
        creator.IsFocusEnabled = true;
    }

    void LateUpdate()
    {
        if (_creator != null)
        {
            UpdateCamera();
        }
    }
    public void UpdateCamera()
    {
        _cameraTarget.Translate(0, Input.GetAxis("Vertical") * CameraMoveSpeed.y, 0);
        _cameraTarget.position = _cameraTarget.position.Clamp(new Vector3(0, InteractionPosition().y, 0), new Vector3(0, InteractionPosition().y + 3, 0));

        _cameraOffset += new Vector3(0, 0, Input.mouseScrollDelta.y * CameraMoveSpeed.z);
        _cameraOffset = _cameraOffset.Clamp(new Vector3(0,0,-5), new Vector3(0,5,-1));
        _creator.Camera.transform.localPosition = Vector3.Lerp(_creator.Camera.transform.localPosition,
                                                              _cameraOffset, 
                                                              Time.unscaledDeltaTime * 5);

        _cameraTarget.Rotate(0, Input.GetAxis("Horizontal") * CameraMoveSpeed.x, 0);
        _creator.Camera.transform.LookAt(_cameraTarget);
    }

    void Update()
    {
        if (_creator != null)
        {
            DetectClicks();
        }
    }
    public void DetectClicks()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = _creator.Camera.ScreenPointToRay(Input.mousePosition);
            var hits = Physics.RaycastAll(ray).ToList();
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                if (hits.Any(x => x.transform.GetComponent<Bender>() != null || x.transform.GetComponent<Mover>() != null))
                {
                    hit = hits.Last(x => x.transform.GetComponent<Bender>() != null || x.transform.GetComponent<Mover>() != null);
                }

                hit.transform.gameObject.SendMessage("Clicked", hit.point, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}

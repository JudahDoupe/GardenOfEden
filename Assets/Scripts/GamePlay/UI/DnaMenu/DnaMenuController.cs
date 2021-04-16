using Assets.Scripts.Plants.Dna;
using Unity.Entities;
using UnityEngine;

public class DnaMenuController : MonoBehaviour
{
    public GameObject OpenMenuButton;

    private Entity _focusedPlant;
    private Bounds _focusedBounds;
    private bool _isMenuOpen = false;

    void Update()
    {
        if (!_isMenuOpen) _focusedPlant = CameraUtils.GetClosestEntity(Singleton.CameraController.FocusPos);
        if (_focusedPlant != Entity.Null) _focusedBounds = CameraUtils.EncapsulateChildren(_focusedPlant);

        PositionOpenMenuButton();

        if (_isMenuOpen) DriftCamera();
    }

    public void OpenMenu()
    {
        _isMenuOpen = true;
        Singleton.CameraController.LockRotation = true;
        Singleton.CameraController.LockMovement = true;
        var dnaReference = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<DnaReference>(_focusedPlant);
        transform.GetComponentInChildren<CategorySelectionController>().Open(DnaService.GetSpeciesDna(dnaReference.SpeciesId));
    }

    public void CloseMenu()
    {
        _isMenuOpen = false;
        Singleton.CameraController.LockRotation = false;
        Singleton.CameraController.LockMovement = false;
        transform.GetComponentInChildren<CategorySelectionController>().Close();
    }

    private void PositionOpenMenuButton()
    {
        if (_isMenuOpen || _focusedPlant == Entity.Null)
        {
            OpenMenuButton.SetActive(false);
        }
        else
        {
            OpenMenuButton.SetActive(true);
            var direction = Vector3.Normalize(_focusedBounds.center);
            OpenMenuButton.transform.position = _focusedBounds.ClosestPoint(2 * _focusedBounds.center) + direction;
        }
    }

    private void DriftCamera()
    {
        var distance = CameraUtils.GetDistanceToIncludeBounds(_focusedBounds, 2.5f);
        Singleton.CameraController.Rotate(new Vector3(distance / 100000, 0));
        Singleton.CameraController.MoveTo(new Coordinate(_focusedBounds.center));
        Singleton.CameraController.Zoom(distance);
    }
}

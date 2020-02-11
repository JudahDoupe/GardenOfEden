using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CameraPostProcessor : MonoBehaviour
{
    public PostProcessProfile PPProfile;

    void LateUpdate()
    {
        UpdateDepthOfField(DI.CameraFocus.PrimaryFocus.Position(0));
    }

    private void UpdateDepthOfField(Vector3 target)
    {
        var dof = PPProfile.GetSetting<DepthOfField>();
        dof.focusDistance.value = Mathf.Lerp(dof.focusDistance.value, Vector3.Distance(Camera.main.transform.position, target), Time.deltaTime);
    }
}

using System;
using System.Collections;
using UnityEngine;

public class CameraFocus : MonoBehaviour
{
    public class Focus
    {
        public Transform Object;
        public float HorizontalOffsetRatio = 0;
        public bool IsDrifting = true;

        public Vector3 GetPosition(float cameraDistance)
        {
            if (Object == null)
            {
                return Camera.main.transform.position;
            }
            else
            {
                var offset = Camera.main.transform.TransformVector(new Vector3(cameraDistance * HorizontalOffsetRatio, 0, 0));
                return offset + Object.GetBounds().center;
            }
        }
        public void RandomizeHorizontalOffsetRatio()
        {
            var offsetRatios = new[] { -0.66f, -0.5f, 0, 0.5f, 0.66f };
            var ratio = offsetRatios[Mathf.RoundToInt(UnityEngine.Random.Range(0, 4))];
            HorizontalOffsetRatio = ratio;
        }
    }

    public Focus PrimaryFocus { get; private set; } = new Focus();
    public Focus SecondaryFocus { get; private set; } = new Focus();

    public void HoldFocus(Focus focus, Transform target, TimeSpan time)
    {
        StartCoroutine(HoldFocusAsync(focus, target, time));
    }

    private IEnumerator HoldFocusAsync(Focus focus, Transform target, TimeSpan time)
    {
        focus.IsDrifting = false;
        focus.Object = target;

        yield return new WaitForSeconds((float) time.TotalSeconds);

        focus.IsDrifting = true;
    }
}

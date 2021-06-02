using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Utils
{
    public static class AnimationUtils
    {
        public static void AnimateTransform(this Transform transform, float seconds, Vector3 localPosition, Vector3 localScale, bool isActive = true)
        {
            Singleton.GameService.StartCoroutine(AnimateVector3(seconds, transform.localPosition, localPosition, pos => transform.localPosition = pos));
            Singleton.GameService.StartCoroutine(AnimateVector3(seconds, transform.localScale, localScale, pos => transform.localScale = pos));
            Singleton.GameService.StartCoroutine(AnimateBool(seconds, isActive, active => transform.gameObject.SetActive(active)));
        }

        public static void AnimatePosition(this Transform transform, float seconds, Vector3 localPosition)
        {
            Singleton.GameService.StartCoroutine(AnimateVector3(seconds, transform.localPosition, localPosition, pos => transform.localPosition = pos));
        }
        public static void AnimateRotation(this Transform transform, float seconds, Quaternion localRotation)
        {
            Singleton.GameService.StartCoroutine(AnimateQuaternion(seconds, transform.localRotation, localRotation, rot => transform.localRotation = rot));
        }

        public static void AnimateUiOpacity(this Transform transform, float seconds, float alpha)
        {
            foreach (var image in transform.GetComponentsInChildren<Image>())
            {
                Singleton.GameService.StartCoroutine(AnimateFloat(seconds, image.color.a, alpha, a => image.color = new Color(image.color.r, image.color.g, image.color.b, a)));
            }
            foreach (var text in transform.GetComponentsInChildren<Text>())
            {
                Singleton.GameService.StartCoroutine(AnimateFloat(seconds, text.color.a, alpha, a => text.color = new Color(text.color.r, text.color.g, text.color.b, a)));
            }
        }

        public static IEnumerator AnimateBool(float seconds, bool end, Action<bool> set)
        {
            yield return new WaitForSeconds(seconds);
            set(end);
        }
        public static IEnumerator AnimateFloat(float seconds, float start, float end, Action<float> set)
        {
            var remainingSeconds = seconds;
            var t = 0f;
            while (t < 1)
            {
                set(math.lerp(start, end, t));
                yield return new WaitForEndOfFrame();
                remainingSeconds -= Time.deltaTime;
                t = 1 - (remainingSeconds / seconds);
            }

            set(end);
        }
        public static IEnumerator AnimateVector3(float seconds, Vector3 start, Vector3 end, Action<Vector3> set)
        {
            var remainingSeconds = seconds;
            var t = 0f;
            while (t < 1)
            {
                set(Vector3.Lerp(start, end, t));
                yield return new WaitForEndOfFrame();
                remainingSeconds -= Time.deltaTime;
                t = 1 - (remainingSeconds / seconds);
            }

            set(end);
        }

        public static IEnumerator AnimateQuaternion(float seconds, Quaternion start, Quaternion end, Action<Quaternion> set)
        {
            var remainingSeconds = seconds;
            var t = 0f;
            while (t < 1)
            {
                set(Quaternion.Lerp(start, end, t));
                yield return new WaitForEndOfFrame();
                remainingSeconds -= Time.deltaTime;
                t = 1 - (remainingSeconds / seconds);
            }

            set(end);
        }

    }
}

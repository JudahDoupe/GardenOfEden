using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Utils
{
    public static class AnimationUtils
    {
        public static void AnimateTransform(this MonoBehaviour script, float seconds, Vector3 localPosition, Vector3 localScale, bool isActive = true)
        {
            script.StartCoroutine(AnimateVector3(seconds, script.transform.localPosition, localPosition, pos => script.transform.localPosition = pos));
            script.StartCoroutine(AnimateVector3(seconds, script.transform.localScale, localScale, pos => script.transform.localScale = pos));
            script.StartCoroutine(AnimateBool(seconds, isActive, active => script.gameObject.SetActive(active)));
        }

        public static void AnimateUiOpacity(this MonoBehaviour script, float seconds, float alpha)
        {
            foreach (var image in script.transform.GetComponentsInChildren<Image>())
            {
                script.StartCoroutine(AnimateFloat(seconds, image.color.a, alpha, a => image.color = new Color(image.color.r, image.color.g, image.color.b, a)));
            }
            foreach (var text in script.transform.GetComponentsInChildren<Text>())
            {
                script.StartCoroutine(AnimateFloat(seconds, text.color.a, alpha, a => text.color = new Color(text.color.r, text.color.g, text.color.b, a)));
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

    }
}

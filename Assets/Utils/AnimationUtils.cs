﻿using System;
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
            Singleton.Instance.StartCoroutine(AnimateVector3(seconds, transform.localPosition, localPosition, pos => transform.localPosition = pos));
            Singleton.Instance.StartCoroutine(AnimateVector3(seconds, transform.localScale, localScale, pos => transform.localScale = pos));
            Singleton.Instance.StartCoroutine(AnimateBool(seconds, isActive, active => transform.gameObject.SetActive(active)));
        }

        public static void AnimatePosition(this Transform transform, float seconds, Vector3 localPosition, Action callback = null)
        {
            Singleton.Instance.StartCoroutine(AnimateVector3(seconds, transform.localPosition, localPosition, pos => transform.localPosition = pos, callback));
        }
        public static void AnimateScale(this Transform transform, float seconds, Vector3 localScale, Action callback = null)
        {
            Singleton.Instance.StartCoroutine(AnimateVector3(seconds, transform.localScale, localScale, scale => transform.localScale = scale, callback));
        }
        public static void AnimateRotation(this Transform transform, float seconds, Quaternion localRotation, Action callback = null)
        {
            Singleton.Instance.StartCoroutine(AnimateQuaternion(seconds, transform.localRotation, localRotation, rot => transform.localRotation = rot, callback));
        }
        public static void AnimateFov(this Camera camera, float seconds, float fov, Action callback = null)
        {
            Singleton.Instance.StartCoroutine(AnimateFloat(seconds, camera.fieldOfView, fov, f => camera.fieldOfView = f, callback));
        }
        public static void AnimateOpacity(this Transform transform, float seconds, float alpha)
        {
            var material = transform.GetComponent<Renderer>().material;
            Singleton.Instance.StartCoroutine(AnimateFloat(seconds, material.color.a, alpha, a => material.color = new Color(material.color.r, material.color.g, material.color.b, a)));
        }
        public static void AnimateUiOpacity(this Transform transform, float seconds, float alpha)
        {
            foreach (var image in transform.GetComponentsInChildren<Image>())
            {
                Singleton.Instance.StartCoroutine(AnimateFloat(seconds, image.color.a, alpha, a => image.color = new Color(image.color.r, image.color.g, image.color.b, a)));
            }
            foreach (var text in transform.GetComponentsInChildren<Text>())
            {
                Singleton.Instance.StartCoroutine(AnimateFloat(seconds, text.color.a, alpha, a => text.color = new Color(text.color.r, text.color.g, text.color.b, a)));
            }
        }

        public static IEnumerator AnimateBool(float seconds, bool end, Action<bool> set)
        {
            yield return new WaitForSeconds(seconds);
            set(end);
        }
        public static IEnumerator AnimateFloat(float seconds, float start, float end, Action<float> set, Action callback = null)
        {
            var remainingSeconds = seconds;
            var t = 0f;
            while (t < 1)
            {
                yield return new WaitForEndOfFrame();
                set(math.lerp(start, end, t));
                remainingSeconds -= Time.deltaTime;
                t = 1 - (remainingSeconds / seconds);
            }

            set(end);
            callback?.Invoke();
        }
        public static IEnumerator AnimateVector3(float seconds, Vector3 start, Vector3 end, Action<Vector3> set, Action callback = null)
        {
            var remainingSeconds = seconds;
            var t = 0f;
            while (t < 1)
            {
                yield return new WaitForEndOfFrame();
                set(Vector3.Lerp(start, end, t));
                remainingSeconds -= Time.deltaTime;
                t = 1 - (remainingSeconds / seconds);
            }

            set(end);
            callback?.Invoke();
        }

        public static IEnumerator AnimateQuaternion(float seconds, Quaternion start, Quaternion end, Action<Quaternion> set, Action callback = null)
        {
            var remainingSeconds = seconds;
            var t = 0f;
            while (t < 1)
            {
                yield return new WaitForEndOfFrame();
                set(Quaternion.Lerp(start, end, t));
                remainingSeconds -= Time.deltaTime;
                t = 1 - (remainingSeconds / seconds);
            }

            set(end);
            callback?.Invoke();
        }

    }
}

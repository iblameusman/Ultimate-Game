using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DodgeGame.Core
{
    public enum EaseType
    {
        Linear,
        EaseOutCubic,
        EaseInOutQuad,
        EaseOutBack
    }

    public class TweenHandle
    {
        internal Coroutine Coroutine;
        internal MonoBehaviour Runner;
        internal bool IsActive;
        internal bool HasCompleted;

        public bool IsRunning => IsActive;
        public bool IsComplete => HasCompleted;

        public void Stop()
        {
            if (IsActive && Runner != null && Coroutine != null)
            {
                Runner.StopCoroutine(Coroutine);
            }
            IsActive = false;
            Coroutine = null;
        }
    }

    public static class Tween
    {
        private class TweenDriver : MonoBehaviour
        {
            private void Awake()
            {
                hideFlags = HideFlags.HideAndDontSave;
            }
        }

        private static TweenDriver driverInstance;

        private static TweenDriver GetDriver()
        {
            if (driverInstance == null)
            {
                var go = new GameObject("[TweenDriver]");
                if (Application.isPlaying)
                {
                    UnityEngine.Object.DontDestroyOnLoad(go);
                }
                go.hideFlags = HideFlags.HideAndDontSave;
                driverInstance = go.AddComponent<TweenDriver>();
            }
            return driverInstance;
        }

        public static float EvaluateEase(float t, EaseType ease)
        {
            t = Mathf.Clamp01(t);
            switch (ease)
            {
                case EaseType.Linear:
                    return t;

                case EaseType.EaseOutCubic:
                    float f1 = 1f - t;
                    return 1f - f1 * f1 * f1;

                case EaseType.EaseInOutQuad:
                    return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) * 0.5f;

                case EaseType.EaseOutBack:
                    const float c1 = 1.70158f;
                    const float c3 = c1 + 1f;
                    float f2 = t - 1f;
                    return 1f + c3 * f2 * f2 * f2 + c1 * f2 * f2;

                default:
                    return t;
            }
        }

        public static TweenHandle Value(float from, float to, float duration, Action<float> onUpdate,
            EaseType easing = EaseType.Linear, bool useUnscaledTime = false, Action onComplete = null)
        {
            var driver = GetDriver();
            var handle = new TweenHandle { Runner = driver, IsActive = true, HasCompleted = false };
            handle.Coroutine = driver.StartCoroutine(ValueRoutine(handle, from, to, duration, onUpdate, easing, useUnscaledTime, onComplete));
            return handle;
        }

        public static TweenHandle Value(Vector3 from, Vector3 to, float duration, Action<Vector3> onUpdate,
            EaseType easing = EaseType.Linear, bool useUnscaledTime = false, Action onComplete = null)
        {
            var driver = GetDriver();
            var handle = new TweenHandle { Runner = driver, IsActive = true, HasCompleted = false };
            handle.Coroutine = driver.StartCoroutine(Vector3Routine(handle, from, to, duration, onUpdate, easing, useUnscaledTime, onComplete));
            return handle;
        }

        public static TweenHandle Position(Transform target, Vector3 to, float duration,
            EaseType easing = EaseType.Linear, bool useUnscaledTime = false, Action onComplete = null, bool isLocal = false)
        {
            if (target == null) return null;
            Vector3 from = isLocal ? target.localPosition : target.position;
            var driver = GetDriver();
            var handle = new TweenHandle { Runner = driver, IsActive = true, HasCompleted = false };
            handle.Coroutine = driver.StartCoroutine(TransformPositionRoutine(handle, target, from, to, duration, easing, useUnscaledTime, onComplete, isLocal));
            return handle;
        }

        public static TweenHandle Position(RectTransform target, Vector2 to, float duration,
            EaseType easing = EaseType.Linear, bool useUnscaledTime = false, Action onComplete = null)
        {
            if (target == null) return null;
            Vector2 from = target.anchoredPosition;
            var driver = GetDriver();
            var handle = new TweenHandle { Runner = driver, IsActive = true, HasCompleted = false };
            handle.Coroutine = driver.StartCoroutine(RectPositionRoutine(handle, target, from, to, duration, easing, useUnscaledTime, onComplete));
            return handle;
        }

        public static TweenHandle Scale(Transform target, Vector3 to, float duration,
            EaseType easing = EaseType.Linear, bool useUnscaledTime = false, Action onComplete = null)
        {
            if (target == null) return null;
            Vector3 from = target.localScale;
            var driver = GetDriver();
            var handle = new TweenHandle { Runner = driver, IsActive = true, HasCompleted = false };
            handle.Coroutine = driver.StartCoroutine(TransformScaleRoutine(handle, target, from, to, duration, easing, useUnscaledTime, onComplete));
            return handle;
        }

        public static TweenHandle Scale(RectTransform target, Vector3 to, float duration,
            EaseType easing = EaseType.Linear, bool useUnscaledTime = false, Action onComplete = null)
        {
            if (target == null) return null;
            Vector3 from = target.localScale;
            var driver = GetDriver();
            var handle = new TweenHandle { Runner = driver, IsActive = true, HasCompleted = false };
            handle.Coroutine = driver.StartCoroutine(RectScaleRoutine(handle, target, from, to, duration, easing, useUnscaledTime, onComplete));
            return handle;
        }

        public static TweenHandle Alpha(CanvasGroup target, float to, float duration,
            EaseType easing = EaseType.Linear, bool useUnscaledTime = false, Action onComplete = null)
        {
            if (target == null) return null;
            float from = target.alpha;
            var driver = GetDriver();
            var handle = new TweenHandle { Runner = driver, IsActive = true, HasCompleted = false };
            handle.Coroutine = driver.StartCoroutine(CanvasAlphaRoutine(handle, target, from, to, duration, easing, useUnscaledTime, onComplete));
            return handle;
        }

        public static TweenHandle Color(Graphic target, Color to, float duration,
            EaseType easing = EaseType.Linear, bool useUnscaledTime = false, Action onComplete = null)
        {
            if (target == null) return null;
            Color from = target.color;
            return Value(0f, 1f, duration, t =>
            {
                if (target != null) target.color = UnityEngine.Color.Lerp(from, to, t);
            }, easing, useUnscaledTime, onComplete);
        }


        private static IEnumerator ValueRoutine(TweenHandle handle, float from, float to, float duration,
            Action<float> onUpdate, EaseType easing, bool useUnscaledTime, Action onComplete)
        {
            float elapsed = 0f;
            float dur = Mathf.Max(duration, 0.0001f);

            while (elapsed < dur)
            {
                elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                float curvedT = EvaluateEase(t, easing);
                onUpdate?.Invoke(Mathf.LerpUnclamped(from, to, curvedT));
                yield return null;
            }

            onUpdate?.Invoke(to);
            handle.IsActive = false;
            handle.HasCompleted = true;
            onComplete?.Invoke();
        }

        private static IEnumerator Vector3Routine(TweenHandle handle, Vector3 from, Vector3 to, float duration,
            Action<Vector3> onUpdate, EaseType easing, bool useUnscaledTime, Action onComplete)
        {
            float elapsed = 0f;
            float dur = Mathf.Max(duration, 0.0001f);

            while (elapsed < dur)
            {
                elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                float curvedT = EvaluateEase(t, easing);
                onUpdate?.Invoke(Vector3.LerpUnclamped(from, to, curvedT));
                yield return null;
            }

            onUpdate?.Invoke(to);
            handle.IsActive = false;
            handle.HasCompleted = true;
            onComplete?.Invoke();
        }

        private static IEnumerator TransformPositionRoutine(TweenHandle handle, Transform target, Vector3 from, Vector3 to,
            float duration, EaseType easing, bool useUnscaledTime, Action onComplete, bool isLocal)
        {
            float elapsed = 0f;
            float dur = Mathf.Max(duration, 0.0001f);

            while (elapsed < dur)
            {
                if (target == null) yield break;
                elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                float curvedT = EvaluateEase(t, easing);
                Vector3 pos = Vector3.LerpUnclamped(from, to, curvedT);

                if (isLocal) target.localPosition = pos;
                else target.position = pos;

                yield return null;
            }

            if (target != null)
            {
                if (isLocal) target.localPosition = to;
                else target.position = to;
            }

            handle.IsActive = false;
            handle.HasCompleted = true;
            onComplete?.Invoke();
        }

        private static IEnumerator RectPositionRoutine(TweenHandle handle, RectTransform target, Vector2 from, Vector2 to,
            float duration, EaseType easing, bool useUnscaledTime, Action onComplete)
        {
            float elapsed = 0f;
            float dur = Mathf.Max(duration, 0.0001f);

            while (elapsed < dur)
            {
                if (target == null) yield break;
                elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                float curvedT = EvaluateEase(t, easing);
                target.anchoredPosition = Vector2.LerpUnclamped(from, to, curvedT);
                yield return null;
            }

            if (target != null) target.anchoredPosition = to;

            handle.IsActive = false;
            handle.HasCompleted = true;
            onComplete?.Invoke();
        }

        private static IEnumerator TransformScaleRoutine(TweenHandle handle, Transform target, Vector3 from, Vector3 to,
            float duration, EaseType easing, bool useUnscaledTime, Action onComplete)
        {
            float elapsed = 0f;
            float dur = Mathf.Max(duration, 0.0001f);

            while (elapsed < dur)
            {
                if (target == null) yield break;
                elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                float curvedT = EvaluateEase(t, easing);
                target.localScale = Vector3.LerpUnclamped(from, to, curvedT);
                yield return null;
            }

            if (target != null) target.localScale = to;

            handle.IsActive = false;
            handle.HasCompleted = true;
            onComplete?.Invoke();
        }

        private static IEnumerator RectScaleRoutine(TweenHandle handle, RectTransform target, Vector3 from, Vector3 to,
            float duration, EaseType easing, bool useUnscaledTime, Action onComplete)
        {
            float elapsed = 0f;
            float dur = Mathf.Max(duration, 0.0001f);

            while (elapsed < dur)
            {
                if (target == null) yield break;
                elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                float curvedT = EvaluateEase(t, easing);
                target.localScale = Vector3.LerpUnclamped(from, to, curvedT);
                yield return null;
            }

            if (target != null) target.localScale = to;

            handle.IsActive = false;
            handle.HasCompleted = true;
            onComplete?.Invoke();
        }

        private static IEnumerator CanvasAlphaRoutine(TweenHandle handle, CanvasGroup target, float from, float to,
            float duration, EaseType easing, bool useUnscaledTime, Action onComplete)
        {
            float elapsed = 0f;
            float dur = Mathf.Max(duration, 0.0001f);

            while (elapsed < dur)
            {
                if (target == null) yield break;
                elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                float curvedT = EvaluateEase(t, easing);
                target.alpha = Mathf.LerpUnclamped(from, to, curvedT);
                yield return null;
            }

            if (target != null) target.alpha = to;

            handle.IsActive = false;
            handle.HasCompleted = true;
            onComplete?.Invoke();
        }
    }
}

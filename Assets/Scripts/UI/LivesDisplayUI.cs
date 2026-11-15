using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DodgeGame.Core;
using DodgeGame.Data;
using DodgeGame.Player;

namespace DodgeGame.UI
{
    public class LivesDisplayUI : MonoBehaviour
    {
        [System.Serializable]
        public class HeartSlot
        {
            public RectTransform Root;
            public Image EmptyIcon;
            public Image FullIcon;
            public CanvasGroup FullCg;
            public TweenHandle ScaleTween;
            public TweenHandle AlphaTween;
        }

        [Header("Configuration")]
        [SerializeField] private HudTheme theme;
        [SerializeField] private Button pauseButton;
        [SerializeField] private RectTransform heartsContainer;
        [SerializeField] private List<HeartSlot> heartSlots = new List<HeartSlot>();

        private PlayerHealth playerHealth;
        private int currentLives = 3;
        private int maxLives = 3;

        public int CurrentLives => currentLives;
        public int SlotCount => heartSlots.Count;
        public List<HeartSlot> Slots => heartSlots;
        public Button PauseButton => pauseButton;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (playerHealth != null)
            {
                playerHealth.OnLivesChanged += UpdateLives;
            }

            if (pauseButton != null)
            {
                pauseButton.onClick.RemoveListener(OnPauseClicked);
                pauseButton.onClick.AddListener(OnPauseClicked);
            }
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.OnLivesChanged -= UpdateLives;
            }

            if (pauseButton != null)
            {
                pauseButton.onClick.RemoveListener(OnPauseClicked);
            }

            for (int i = 0; i < heartSlots.Count; i++)
            {
                heartSlots[i].ScaleTween?.Stop();
                heartSlots[i].AlphaTween?.Stop();
            }
        }

        private void Start()
        {
            int startingLives = playerHealth != null ? playerHealth.CurrentLives : 3;
            maxLives = playerHealth != null ? playerHealth.MaxLives : Mathf.Max(3, startingLives);
            SetLivesImmediate(startingLives);
        }

        private void ResolveReferences()
        {
            if (playerHealth == null) playerHealth = FindAnyObjectByType<PlayerHealth>();

            if (theme == null)
            {
                theme = Resources.Load<HudTheme>("HudTheme");
#if UNITY_EDITOR
                if (theme == null)
                {
                    theme = UnityEditor.AssetDatabase.LoadAssetAtPath<HudTheme>("Assets/Scripts/Data/HudTheme.asset");
                }
#endif
            }

            if (pauseButton == null)
            {
                pauseButton = transform.Find("PauseButton")?.GetComponent<Button>();
            }

            if (heartsContainer == null)
            {
                heartsContainer = transform.Find("LivesPanel/HeartsRow")?.GetComponent<RectTransform>()
                               ?? transform.Find("HeartsRow")?.GetComponent<RectTransform>();
            }
        }

        public void SetLivesImmediate(int lives)
        {
            currentLives = Mathf.Clamp(lives, 0, heartSlots.Count);

            for (int i = 0; i < heartSlots.Count; i++)
            {
                var slot = heartSlots[i];
                if (slot == null || slot.FullIcon == null || slot.FullCg == null) continue;

                slot.ScaleTween?.Stop();
                slot.AlphaTween?.Stop();

                bool isFull = i < currentLives;
                slot.FullIcon.gameObject.SetActive(isFull);
                slot.FullIcon.transform.localScale = isFull ? Vector3.one : Vector3.zero;
                slot.FullCg.alpha = isFull ? 1f : 0f;
            }
        }

        public void UpdateLives(int newLives)
        {
            newLives = Mathf.Clamp(newLives, 0, heartSlots.Count);

            if (newLives < currentLives)
            {
                for (int i = currentLives - 1; i >= newLives; i--)
                {
                    if (i < heartSlots.Count)
                    {
                        AnimateLostHeart(heartSlots[i]);
                    }
                }
            }
            else if (newLives > currentLives)
            {
                for (int i = currentLives; i < newLives; i++)
                {
                    if (i < heartSlots.Count)
                    {
                        AnimateGainedHeart(heartSlots[i]);
                    }
                }
            }

            currentLives = newLives;
        }

        private void AnimateLostHeart(HeartSlot slot)
        {
            if (slot == null || slot.FullIcon == null || slot.FullCg == null) return;

            slot.ScaleTween?.Stop();
            slot.AlphaTween?.Stop();

            slot.ScaleTween = Tween.Scale(slot.FullIcon.transform, Vector3.zero, 0.25f, EaseType.EaseOutCubic, useUnscaledTime: true);
            slot.AlphaTween = Tween.Alpha(slot.FullCg, 0f, 0.25f, EaseType.Linear, useUnscaledTime: true, onComplete: () =>
            {
                slot.FullIcon.gameObject.SetActive(false);
            });
        }

        private void AnimateGainedHeart(HeartSlot slot)
        {
            if (slot == null || slot.FullIcon == null || slot.FullCg == null) return;

            slot.ScaleTween?.Stop();
            slot.AlphaTween?.Stop();

            slot.FullIcon.gameObject.SetActive(true);
            slot.FullIcon.transform.localScale = Vector3.zero;
            slot.FullCg.alpha = 1f;

            slot.ScaleTween = Tween.Scale(slot.FullIcon.transform, Vector3.one, 0.25f, EaseType.EaseOutBack, useUnscaledTime: true);
        }

        public void OnPauseClicked()
        {
            if (PauseController.Instance != null)
            {
                PauseController.Instance.SetPaused(true);
            }
            else
            {
                var pauseUI = FindAnyObjectByType<PauseUI>();
                if (pauseUI != null) pauseUI.OpenPauseMenu();
            }
        }
    }
}

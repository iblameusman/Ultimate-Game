using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DodgeGame.UI
{
    [RequireComponent(typeof(Button))]
    public class CloseButtonFX : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler,
        ISelectHandler, IDeselectHandler
    {
        [Header("Visual Components")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image focusGlowImage;
        [SerializeField] private RectTransform scaleTarget;

        [Header("Colors")]
        private static readonly Color NormalColor = new Color(0.898f, 0.282f, 0.302f, 0.90f);
        private static readonly Color HoverColor = new Color(0.988f, 0.353f, 0.373f, 0.95f);
        private static readonly Color FocusGlowColor = new Color(0.239f, 0.863f, 0.165f, 0.40f);

        private Button button;
        private bool isHovered = false;
        private bool isFocused = false;
        private Coroutine activeScaleRoutine;

        private void Awake()
        {
            button = GetComponent<Button>();
            if (scaleTarget == null) scaleTarget = GetComponent<RectTransform>();
            if (backgroundImage == null) backgroundImage = GetComponent<Image>();

            EnsureFocusGlow();
            ApplyState();
        }

        private void OnEnable()
        {
            ApplyState();
        }

        private void EnsureFocusGlow()
        {
            if (focusGlowImage == null)
            {
                var glowTr = transform.Find("FocusGlow");
                if (glowTr != null)
                {
                    focusGlowImage = glowTr.GetComponent<Image>();
                }
                else
                {
                    var glowGo = new GameObject("FocusGlow");
                    glowGo.transform.SetParent(transform, false);
                    glowGo.transform.SetAsFirstSibling();

                    var rt = glowGo.AddComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = new Vector2(-4f, -4f);
                    rt.offsetMax = new Vector2(4f, 4f);

                    focusGlowImage = glowGo.AddComponent<Image>();
                    focusGlowImage.color = new Color(FocusGlowColor.r, FocusGlowColor.g, FocusGlowColor.b, 0f);
                    focusGlowImage.raycastTarget = false;
                }
            }
        }

        private void ApplyState()
        {
            if (scaleTarget != null) scaleTarget.localScale = Vector3.one;
            if (backgroundImage != null)
            {
                backgroundImage.color = isHovered ? HoverColor : NormalColor;
            }
            if (focusGlowImage != null)
            {
                focusGlowImage.color = new Color(FocusGlowColor.r, FocusGlowColor.g, FocusGlowColor.b, isFocused ? FocusGlowColor.a : 0f);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (button != null && !button.interactable) return;
            isHovered = true;
            ApplyState();
            DodgeGame.Core.SoundManager.Instance?.PlayButtonHover();
            DodgeGame.Core.AudioManager.Instance?.PlayUI("UI_HOVER");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            ApplyState();
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (button != null && !button.interactable) return;
            isFocused = true;
            ApplyState();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            isFocused = false;
            ApplyState();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (button != null && !button.interactable) return;
            StartScaleAnimation(new Vector3(0.95f, 0.95f, 1f), 0.05f);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            StartScaleAnimation(Vector3.one, 0.05f);
        }

        private void StartScaleAnimation(Vector3 targetScale, float duration)
        {
            if (scaleTarget == null) return;
            if (activeScaleRoutine != null) StopCoroutine(activeScaleRoutine);
            activeScaleRoutine = StartCoroutine(ScaleRoutine(targetScale, duration));
        }

        private IEnumerator ScaleRoutine(Vector3 targetScale, float duration)
        {
            Vector3 startScale = scaleTarget.localScale;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                scaleTarget.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }
            scaleTarget.localScale = targetScale;
            activeScaleRoutine = null;
        }
    }
}

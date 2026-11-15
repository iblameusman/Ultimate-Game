using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DodgeGame.Data;

namespace DodgeGame.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(TMP_Text))]
    public class TextStyler : MonoBehaviour
    {
        private const float BASE_REFERENCE_HEIGHT = 1080f;

        [Header("Role")]
        [SerializeField] private TypographyRole role = TypographyRole.Body;

        [Header("Configuration")]
        [Tooltip("Optional TypographyConfig override. If null, automatically resolves DefaultTypography.")]
        [SerializeField] private TypographyConfig config;

        [Header("HUD Overrides")]
        [SerializeField] private bool overrideFontSize = false;
        [SerializeField] private float customFontSize = 24f;
        [SerializeField] private bool overrideColor = false;
        [SerializeField] private Color customColor = Color.white;
        [SerializeField] private bool overrideFontStyle = false;
        [SerializeField] private FontStyles customFontStyle = FontStyles.Normal;
        [SerializeField] private bool overrideOutline = false;
        [SerializeField] private float customOutlineWidth = 0f;
        [SerializeField] private Color customOutlineColor = Color.black;
        [SerializeField] private bool enforceNoWrap = true;

        private TMP_Text targetText;
        private CanvasScaler cachedScaler;
        private int lastScreenWidth = -1;
        private int lastScreenHeight = -1;

        public TypographyRole Role
        {
            get => role;
            set
            {
                role = value;
                ApplyStyle();
            }
        }

        public TypographyConfig Config
        {
            get => config;
            set
            {
                config = value;
                ApplyStyle();
            }
        }

        public bool OverrideFontSize
        {
            get => overrideFontSize;
            set
            {
                overrideFontSize = value;
                ApplyStyle();
            }
        }

        public float CustomFontSize
        {
            get => customFontSize;
            set
            {
                customFontSize = value;
                ApplyStyle();
            }
        }

        public bool OverrideColor
        {
            get => overrideColor;
            set
            {
                overrideColor = value;
                ApplyStyle();
            }
        }

        public Color CustomColor
        {
            get => customColor;
            set
            {
                customColor = value;
                ApplyStyle();
            }
        }

        public bool OverrideFontStyle
        {
            get => overrideFontStyle;
            set
            {
                overrideFontStyle = value;
                ApplyStyle();
            }
        }

        public FontStyles CustomFontStyle
        {
            get => customFontStyle;
            set
            {
                customFontStyle = value;
                ApplyStyle();
            }
        }

        public bool OverrideOutline
        {
            get => overrideOutline;
            set
            {
                overrideOutline = value;
                ApplyStyle();
            }
        }

        public float CustomOutlineWidth
        {
            get => customOutlineWidth;
            set
            {
                customOutlineWidth = value;
                ApplyStyle();
            }
        }

        public Color CustomOutlineColor
        {
            get => customOutlineColor;
            set
            {
                customOutlineColor = value;
                ApplyStyle();
            }
        }

        public bool EnforceNoWrap
        {
            get => enforceNoWrap;
            set
            {
                enforceNoWrap = value;
                ApplyStyle();
            }
        }

        public TMP_Text TargetText
        {
            get
            {
                if (targetText == null) targetText = GetComponent<TMP_Text>();
                return targetText;
            }
        }

        private void Awake()
        {
            ResolveReferences();
            ApplyStyle();
        }

        private void OnEnable()
        {
            ResolveReferences();
            ApplyStyle();
        }

        private void Start()
        {
            ApplyStyle();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
            {
                lastScreenWidth = Screen.width;
                lastScreenHeight = Screen.height;
                ApplyStyle();
            }
        }

        private void Update()
        {
            if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
            {
                lastScreenWidth = Screen.width;
                lastScreenHeight = Screen.height;
                ApplyStyle();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ResolveReferences();
            ApplyStyle();
        }
#endif

        private void ResolveReferences()
        {
            if (targetText == null)
            {
                targetText = GetComponent<TMP_Text>();
            }

            if (cachedScaler == null)
            {
                cachedScaler = GetComponentInParent<CanvasScaler>();
            }

            if (config == null)
            {
                config = Resources.Load<TypographyConfig>("SO_Typography") ?? Resources.Load<TypographyConfig>("DefaultTypography");
#if UNITY_EDITOR
                if (config == null)
                {
                    config = UnityEditor.AssetDatabase.LoadAssetAtPath<TypographyConfig>("Assets/ScriptableObjects/SO_Typography.asset")
                          ?? UnityEditor.AssetDatabase.LoadAssetAtPath<TypographyConfig>("Assets/ScriptableObjects/DefaultTypography.asset");
                }
#endif
            }
        }

        public void ApplyStyle()
        {
            if (targetText == null)
            {
                targetText = GetComponent<TMP_Text>();
                if (targetText == null) return;
            }

            if (config == null)
            {
                ResolveReferences();
                if (config == null) return;
            }

            TextStyle style = config.GetStyle(role);

            if (style.Font != null)
            {
                targetText.font = style.Font;
                if (targetText.fontSharedMaterial == null && style.Font.material != null)
                {
                    targetText.fontSharedMaterial = style.Font.material;
                }
            }

            float scaleMultiplier = CalculateScaleMultiplier();
            float baseSize = overrideFontSize ? customFontSize : style.Size;
            targetText.fontSize = baseSize * scaleMultiplier;

            targetText.color = overrideColor ? customColor : style.Color;

            if (overrideFontStyle)
            {
                targetText.fontStyle = customFontStyle;
            }

            targetText.characterSpacing = style.CharacterSpacing;

            if (style.LineHeight != 0f)
            {
                targetText.lineSpacing = style.LineHeight;
            }

            if (enforceNoWrap)
            {
                targetText.textWrappingMode = TextWrappingModes.NoWrap;
                targetText.overflowMode = TextOverflowModes.Overflow;
            }

            if (overrideOutline)
            {
                targetText.outlineWidth = customOutlineWidth;
                targetText.outlineColor = customOutlineColor;
            }
            else if (targetText.fontSharedMaterial != null)
            {
                if (style.OutlineWidth > 0f)
                {
                    targetText.outlineWidth = style.OutlineWidth;
                    targetText.outlineColor = style.OutlineColor;
                }
                else
                {
                    targetText.outlineWidth = 0f;
                }
            }
        }

        private float CalculateScaleMultiplier()
        {
            if (cachedScaler == null)
            {
                cachedScaler = GetComponentInParent<CanvasScaler>();
            }

            if (cachedScaler != null)
            {
                if (cachedScaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
                {
                    float refH = cachedScaler.referenceResolution.y > 0 ? cachedScaler.referenceResolution.y : BASE_REFERENCE_HEIGHT;
                    return refH / BASE_REFERENCE_HEIGHT;
                }
                else if (cachedScaler.uiScaleMode == CanvasScaler.ScaleMode.ConstantPixelSize)
                {
                    return Screen.height > 0 ? (float)Screen.height / BASE_REFERENCE_HEIGHT : 1f;
                }
            }

            return 1f;
        }

        public void SetCustomFontSize(float size)
        {
            overrideFontSize = true;
            customFontSize = size;
            ApplyStyle();
        }

        public void SetCustomColor(Color color)
        {
            overrideColor = true;
            customColor = color;
            ApplyStyle();
        }

        public void SetCustomFontStyle(FontStyles fontStyle)
        {
            overrideFontStyle = true;
            customFontStyle = fontStyle;
            ApplyStyle();
        }

        public void SetOutline(float width, Color color)
        {
            overrideOutline = true;
            customOutlineWidth = width;
            customOutlineColor = color;
            ApplyStyle();
        }

        public void SetCustomOutline(float width, Color color) => SetOutline(width, color);

        public static string FormatTabular(string text, string width = "0.6em")
        {
            return $"<mspace={width}>{text}</mspace>";
        }
    }
}

using System;
using UnityEngine;
using TMPro;

namespace DodgeGame.Data
{
    public enum TypographyRole
    {
        Display,
        Heading,
        Button,
        Label,
        Body,
        Caption,
        Subtitle,
        Tagline
    }

    [Serializable]
    public struct TextStyle
    {
        [SerializeField] private TMP_FontAsset font;
        [SerializeField] private float size;
        [SerializeField] private Color color;
        [SerializeField] private float characterSpacing;
        [SerializeField] private float lineHeight;
        [SerializeField] private float outlineWidth;
        [SerializeField] private Color outlineColor;

        public TMP_FontAsset Font => font;
        public float Size => size;
        public Color Color => color;
        public float CharacterSpacing => characterSpacing;
        public float LineHeight => lineHeight;
        public float OutlineWidth => outlineWidth;
        public Color OutlineColor => outlineColor;

        public TextStyle(
            TMP_FontAsset font,
            float size,
            Color color,
            float characterSpacing = 0f,
            float lineHeight = 0f,
            float outlineWidth = 0f,
            Color outlineColor = default)
        {
            this.font = font;
            this.size = size;
            this.color = color;
            this.characterSpacing = characterSpacing;
            this.lineHeight = lineHeight;
            this.outlineWidth = outlineWidth;
            this.outlineColor = outlineColor;
        }

        public void SetFont(TMP_FontAsset newFont) => font = newFont;
        public void SetSize(float newSize) => size = newSize;
        public void SetColor(Color newColor) => color = newColor;
        public void SetOutline(float width, Color color) { outlineWidth = width; outlineColor = color; }
    }

    [CreateAssetMenu(fileName = "TypographyConfig", menuName = "DodgeGame/Typography Config")]
    public class TypographyConfig : ScriptableObject
    {
        [Header("Styles")]
        [SerializeField] private TextStyle display;
        [SerializeField] private TextStyle heading;
        [SerializeField] private TextStyle button;
        [SerializeField] private TextStyle label;
        [SerializeField] private TextStyle body;
        [SerializeField] private TextStyle caption;
        [SerializeField] private TextStyle subtitle;
        [SerializeField] private TextStyle tagline;

        public TextStyle Display => display;
        public TextStyle Heading => heading;
        public TextStyle Button => button;
        public TextStyle Label => label;
        public TextStyle Body => body;
        public TextStyle Caption => caption;
        public TextStyle Subtitle => subtitle.Font != null ? subtitle : label;
        public TextStyle Tagline => tagline.Font != null ? tagline : heading;

        public TextStyle GetStyle(TypographyRole role)
        {
            switch (role)
            {
                case TypographyRole.Display: return display;
                case TypographyRole.Heading: return heading;
                case TypographyRole.Button: return button;
                case TypographyRole.Label: return label;
                case TypographyRole.Body: return body;
                case TypographyRole.Caption: return caption;
                case TypographyRole.Subtitle: return Subtitle;
                case TypographyRole.Tagline: return Tagline;
                default: return body;
            }
        }

        public void InitializeDefaults(TMP_FontAsset fontDisplay, TMP_FontAsset fontUI, TMP_FontAsset fontBody)
        {
            Color textNavy = new Color(0.118f, 0.165f, 0.200f, 1f);
            Color brightWhite = Color.white;
            Color subtitleWhite = new Color(0.92f, 0.94f, 0.96f, 1f);
            Color captionMuted = new Color(0.118f, 0.165f, 0.200f, 0.75f);

            display = new TextStyle(fontDisplay, 96f, brightWhite, 0f, 0f, 0f, Color.clear);

            heading = new TextStyle(fontDisplay, 56f, textNavy, 0f, 0f, 0f, Color.clear);

            button = new TextStyle(fontUI, 52f, brightWhite, 0.5f, 0f, 0f, Color.clear);

            label = new TextStyle(fontUI, 34f, textNavy, 0f, 0f, 0f, Color.clear);

            body = new TextStyle(fontBody, 30f, textNavy, 0f, 0f, 0f, Color.clear);

            caption = new TextStyle(fontBody, 26f, textNavy, 0f, 0f, 0f, Color.clear);

            subtitle = new TextStyle(fontUI, 34f, textNavy, 0f, 0f, 0f, Color.clear);

            tagline = new TextStyle(fontUI, 38f, textNavy, 15f, 0f, 0f, Color.clear);
        }
    }
}

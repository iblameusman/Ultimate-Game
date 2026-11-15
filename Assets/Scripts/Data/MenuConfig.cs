using UnityEngine;

namespace DodgeGame.Data
{
    [CreateAssetMenu(fileName = "MenuConfig", menuName = "DodgeGame/Menu Config")]
    public class MenuConfig : ScriptableObject
    {
        [Header("Primary Palette")]
        [Tooltip("Accent Green for highlights, active states, and success indicators.")]
        [SerializeField] private Color accentGreen = new Color(0.13f, 0.77f, 0.45f, 1f);

        [Tooltip("Charcoal dark background and card fill.")]
        [SerializeField] private Color charcoal = new Color(0.055f, 0.078f, 0.094f, 1f);

        [Tooltip("Steel Grey for borders, dividers, and disabled elements.")]
        [SerializeField] private Color steelGrey = new Color(0.28f, 0.33f, 0.41f, 1f);

        [Tooltip("Text Navy / Dark contrast tone.")]
        [SerializeField] private Color textNavy = new Color(0.06f, 0.09f, 0.16f, 1f);

        [Tooltip("Light Foreground Text Color.")]
        [SerializeField] private Color lightText = new Color(0.94f, 0.96f, 0.98f, 1f);

        [Header("Ball Visual Indicators (Card Badges Only)")]
        [SerializeField] private Color fireBallColor = new Color(1f, 0.34f, 0.13f, 1f);
        [SerializeField] private Color stickyBallColor = new Color(0f, 0.9f, 1f, 1f);
        [SerializeField] private Color gunBallColor = new Color(0.62f, 0.65f, 0.71f, 1f);
        [SerializeField] private Color nuclearBallColor = new Color(0.46f, 1f, 0.01f, 1f);

        public Color AccentGreen => accentGreen;
        public Color Charcoal => charcoal;
        public Color SteelGrey => steelGrey;
        public Color TextNavy => textNavy;
        public Color LightText => lightText;

        public Color FireBallColor => fireBallColor;
        public Color StickyBallColor => stickyBallColor;
        public Color GunBallColor => gunBallColor;
        public Color NuclearBallColor => nuclearBallColor;
    }
}

using UnityEngine;

namespace DodgeGame.Data
{
    [CreateAssetMenu(fileName = "MenuTheme", menuName = "DodgeGame/Menu Theme")]
    public class MenuTheme : ScriptableObject
    {
        [Header("Background")]
        [Tooltip("#F4F6F8")]
        [SerializeField] private Color backgroundTop = new Color(0.957f, 0.965f, 0.973f, 1.0f);
        [Tooltip("#FFFFFF")]
        [SerializeField] private Color backgroundBottom = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        [Tooltip("#DDE3E8 at 25% opacity")]
        [SerializeField] private Color hexPatternColor = new Color(0.867f, 0.890f, 0.910f, 0.25f);

        [Header("Accents")]
        [Tooltip("#3DDC2A")]
        [SerializeField] private Color accentGreen = new Color(0.239f, 0.863f, 0.165f, 1.0f);
        [Tooltip("#7CFF5A")]
        [SerializeField] private Color accentGreenGlow = new Color(0.486f, 1.0f, 0.353f, 1.0f);

        [Header("Buttons")]
        [Tooltip("#2F383E")]
        [SerializeField] private Color buttonDarkTop = new Color(0.184f, 0.220f, 0.243f, 1.0f);
        [Tooltip("#222A2F")]
        [SerializeField] private Color buttonDarkBottom = new Color(0.133f, 0.165f, 0.184f, 1.0f);

        [Header("Text & Layout")]
        [Tooltip("#1E2A33")]
        [SerializeField] private Color textNavy = new Color(0.118f, 0.165f, 0.200f, 1.0f);
        [Tooltip("#8A949B")]
        [SerializeField] private Color steelGrey = new Color(0.541f, 0.580f, 0.608f, 1.0f);

        [Header("Header Panel")]
        [Tooltip("#E9EDF0")]
        [SerializeField] private Color headerPanelTop = new Color(0.914f, 0.929f, 0.941f, 1.0f);
        [Tooltip("#D3D9DE")]
        [SerializeField] private Color headerPanelBottom = new Color(0.827f, 0.851f, 0.871f, 1.0f);
        [SerializeField] private Color headerPanelInnerBevel = Color.white;

        public Color BackgroundTop => backgroundTop;
        public Color BackgroundBottom => backgroundBottom;
        public Color HexPatternColor => hexPatternColor;
        public Color AccentGreen => accentGreen;
        public Color AccentGreenGlow => accentGreenGlow;
        public Color ButtonDarkTop => buttonDarkTop;
        public Color ButtonDarkBottom => buttonDarkBottom;
        public Color TextNavy => textNavy;
        public Color SteelGrey => steelGrey;
        public Color HeaderPanelTop => headerPanelTop;
        public Color HeaderPanelBottom => headerPanelBottom;
        public Color HeaderPanelInnerBevel => headerPanelInnerBevel;
    }
}

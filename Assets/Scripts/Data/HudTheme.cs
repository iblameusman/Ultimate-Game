using System;
using UnityEngine;

namespace DodgeGame.Data
{
    [CreateAssetMenu(fileName = "HudTheme", menuName = "DodgeGame/HUD Theme")]
    public class HudTheme : ScriptableObject
    {
        [Header("Panel Style")]
        [Tooltip("#E9EDF0 at 85% opacity")]
        [SerializeField] private Color panelColor = new Color(0.9137f, 0.9294f, 0.9412f, 0.85f);

        [Tooltip("1 px #B8C2CA border")]
        [SerializeField] private Color panelBorderColor = new Color(0.7216f, 0.7608f, 0.7922f, 1f);

        [Tooltip("2 px white inner bevel")]
        [SerializeField] private Color panelInnerBevelColor = new Color(1f, 1f, 1f, 1f);

        [Header("Text Typography")]
        [Tooltip("Text Navy #1E2A33")]
        [SerializeField] private Color textPrimary = new Color(0.1176f, 0.1647f, 0.2000f, 1f);

        [Tooltip("Secondary #4A5863")]
        [SerializeField] private Color textSecondary = new Color(0.2902f, 0.3451f, 0.3882f, 1f);

        [Header("Status Accents")]
        [Tooltip("Accent Ready #3DDC2A")]
        [SerializeField] private Color accentReady = new Color(0.2392f, 0.8627f, 0.1647f, 1f);

        [Tooltip("Accent Active #2F9BFF (used while skill is running)")]
        [SerializeField] private Color accentActive = new Color(0.1843f, 0.6078f, 1.0000f, 1f);

        [Tooltip("Cooldown fill #9AA6AF")]
        [SerializeField] private Color cooldownFill = new Color(0.6039f, 0.6510f, 0.6863f, 1f);

        [Tooltip("Danger #E5484D (lives lost, damage flash)")]
        [SerializeField] private Color danger = new Color(0.8980f, 0.2824f, 0.3020f, 1f);

        [Header("Glow")]
        [Tooltip("Optional glow accent (transparent by default)")]
        [SerializeField] private Color glowColor = new Color(0.2392f, 0.8627f, 0.1647f, 0f);

        public Color PanelColor => panelColor;
        public Color PanelBorderColor => panelBorderColor;
        public Color PanelInnerBevelColor => panelInnerBevelColor;
        public Color TextPrimary => textPrimary;
        public Color TextSecondary => textSecondary;
        public Color AccentReady => accentReady;
        public Color AccentActive => accentActive;
        public Color CooldownFill => cooldownFill;
        public Color Danger => danger;
        public Color GlowColor => glowColor;

        public void ResetToDefaults()
        {
            panelColor = new Color(0.9137f, 0.9294f, 0.9412f, 0.85f);
            panelBorderColor = new Color(0.7216f, 0.7608f, 0.7922f, 1f);
            panelInnerBevelColor = new Color(1f, 1f, 1f, 1f);
            textPrimary = new Color(0.1176f, 0.1647f, 0.2000f, 1f);
            textSecondary = new Color(0.2902f, 0.3451f, 0.3882f, 1f);
            accentReady = new Color(0.2392f, 0.8627f, 0.1647f, 1f);
            accentActive = new Color(0.1843f, 0.6078f, 1.0000f, 1f);
            cooldownFill = new Color(0.6039f, 0.6510f, 0.6863f, 1f);
            danger = new Color(0.8980f, 0.2824f, 0.3020f, 1f);
            glowColor = new Color(0.2392f, 0.8627f, 0.1647f, 0f);
        }
    }
}
